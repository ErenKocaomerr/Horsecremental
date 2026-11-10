using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Horsecremental.Racing.Editor
{
    public static class TextMeshProProjectMigration
    {
        private const string BINDINGS_PATH = "Tools/TmpMigration/Bindings.json";

        [MenuItem("Horsecremental/UI/Migrate Project to m6x11 TextMeshPro")]
        public static void Migrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            PixelFontAssets.Ensure();
            string original = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            JArray savedBindings = File.Exists(BINDINGS_PATH) ? JArray.Parse(File.ReadAllText(BINDINGS_PATH)) : new JArray();
            int count = 0;
            try
            {
                foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab.GetComponentsInChildren<Text>(true).Length == 0 &&
                        prefab.GetComponentsInChildren<TextMesh>(true).Length == 0 &&
                        prefab.GetComponentsInChildren<TMP_Text>(true).Length == 0) continue;
                    GameObject root = PrefabUtility.LoadPrefabContents(path);
                    try
                    {
                        count += ConvertHierarchy(new[] { root }, path, savedBindings);
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                    }
                    finally { PrefabUtility.UnloadPrefabContents(root); }
                }
                foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var scene = EditorSceneManager.OpenScene(path);
                    GameObject[] roots = scene.GetRootGameObjects();
                    if (!roots.Any(root => root.GetComponentsInChildren<Text>(true).Length > 0 ||
                        root.GetComponentsInChildren<TextMesh>(true).Length > 0 ||
                        root.GetComponentsInChildren<TMP_Text>(true).Length > 0)) continue;
                    count += ConvertHierarchy(roots, path, savedBindings);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
                AssetDatabase.SaveAssets();
            }
            finally
            {
                if (!string.IsNullOrEmpty(original)) EditorSceneManager.OpenScene(original);
            }
            Debug.Log("m6x11 TextMeshPro migration completed. Converted " + count + " text components and restored their serialized bindings.");
        }

        private static int ConvertHierarchy(GameObject[] roots, string path, JArray savedBindings)
        {
            List<(Object Owner, string Property, string Text)> bindings = new List<(Object, string, string)>();
            foreach (GameObject root in roots)
            {
                foreach (MonoBehaviour owner in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (owner == null) continue;
                    SerializedProperty property = new SerializedObject(owner).GetIterator();
                    while (property.Next(true))
                    {
                        if (property.propertyType != SerializedPropertyType.ObjectReference) continue;
                        Object value = property.objectReferenceValue;
                        if (value is Text || value is TextMesh)
                            bindings.Add((owner, property.propertyPath, GlobalObjectId.GetGlobalObjectIdSlow(value).ToString()));
                    }
                }
            }
            foreach (JToken binding in savedBindings)
            {
                if ((string)binding["Scene"] != path) continue;
                if (!GlobalObjectId.TryParse((string)binding["Owner"], out GlobalObjectId id)) continue;
                Object owner = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
                if (owner != null) bindings.Add((owner, (string)binding["Property"], (string)binding["Text"]));
            }
            Dictionary<string, TMP_Text> replacements = new Dictionary<string, TMP_Text>();
            foreach (GameObject root in roots)
            {
                foreach (Text old in root.GetComponentsInChildren<Text>(true)) ConvertUI(old, replacements);
                foreach (TextMesh old in root.GetComponentsInChildren<TextMesh>(true)) ConvertWorld(old, replacements);
                foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    text.font = PixelFontAssets.Font;
                    text.fontStyle = FontStyles.Normal;
                    text.text = PixelFontAssets.ReadableText(text.text);
                    EditorUtility.SetDirty(text);
                }
            }
            foreach (var binding in bindings)
            {
                if (!replacements.TryGetValue(binding.Text, out TMP_Text text)) continue;
                SerializedObject owner = new SerializedObject(binding.Owner);
                SerializedProperty property = owner.FindProperty(binding.Property);
                if (property == null) throw new InvalidOperationException("Missing text binding: " + binding.Property);
                property.objectReferenceValue = text;
                owner.ApplyModifiedPropertiesWithoutUndo();
            }
            return replacements.Count;
        }

        private static void ConvertUI(Text old, Dictionary<string, TMP_Text> replacements)
        {
            string id = GlobalObjectId.GetGlobalObjectIdSlow(old).ToString();
            GameObject target = old.gameObject;
            string value = old.text;
            int size = old.fontSize;
            Color color = old.color;
            bool raycast = old.raycastTarget;
            bool enabled = old.enabled;
            TextAnchor alignment = old.alignment;
            bool rich = old.supportRichText;
            Object.DestroyImmediate(old);
            TextMeshProUGUI text = target.AddComponent<TextMeshProUGUI>();
            PixelFontAssets.Configure(text, size, Alignment(alignment));
            text.text = value; text.color = color; text.raycastTarget = raycast;
            text.richText = rich; text.enabled = enabled;
            replacements.Add(id, text);
        }

        private static void ConvertWorld(TextMesh old, Dictionary<string, TMP_Text> replacements)
        {
            string id = GlobalObjectId.GetGlobalObjectIdSlow(old).ToString();
            GameObject target = old.gameObject;
            string value = old.text;
            float size = old.fontSize * old.characterSize;
            Color color = old.color;
            TextAnchor anchor = old.anchor;
            int order = target.GetComponent<MeshRenderer>().sortingOrder;
            Object.DestroyImmediate(old);
            TextMeshPro text = target.AddComponent<TextMeshPro>();
            PixelFontAssets.Configure(text, size, Alignment(anchor));
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.rectTransform.sizeDelta = new Vector2(30, 3);
            text.text = value; text.color = color;
            text.GetComponent<MeshRenderer>().sortingOrder = order;
            replacements.Add(id, text);
        }

        private static TextAlignmentOptions Alignment(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.Center;
            }
        }
    }
}
