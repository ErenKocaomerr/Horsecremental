using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Horsecremental.Racing.Editor
{
    public static class UpgradeTreePresentationSetup
    {
        private const string MATERIAL_PATH = "Assets/Racing/Generated/UI/UpgradeTreeBackground.mat";
        private const string SHAPE_PATH = "Assets/Racing/Generated/UI/TreeNodeShape.png";
        private const string BORDER_PATH = "Assets/Racing/Generated/UI/TreeNodeBorder.png";
        private static readonly Vector2[] POSITIONS = {
            new Vector2(0, 255), new Vector2(-270, 75), new Vector2(-490, -105),
            new Vector2(270, 75), new Vector2(510, 75), new Vector2(-90, -105),
            new Vector2(-90, -275), new Vector2(270, -105), new Vector2(490, -105),
            new Vector2(650, -105), new Vector2(650, -275)
        };

        [MenuItem("Horsecremental/Style Progressive Upgrade Tree")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (SceneManager.GetActiveScene().path != Milestone3SceneSetup.WORLD_PATH)
                throw new InvalidOperationException("Open Milestone3World before styling its Upgrade Tree.");
            GameplayWorld world = UnityEngine.Object.FindAnyObjectByType<GameplayWorld>();
            if (world == null) throw new InvalidOperationException("The existing gameplay world is required.");
            ProgressionSettings settings = AssetDatabase.LoadAssetAtPath<ProgressionSettings>(Milestone2SceneSetup.SETTINGS_PATH);
            ApplyTree(world.transform, settings, UIIconAssets.Ensure());
            EditorSceneManager.MarkSceneDirty(world.gameObject.scene);
            EditorSceneManager.SaveScene(world.gameObject.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Progressive Upgrade Tree styled with procedural UI background. Existing unlock costs and disabled branches preserved.");
        }

        internal static void ApplyTree(Transform root, ProgressionSettings settings, UIPresentationSettings presentation)
        {
            Transform treeRoot = root.Find("Upgrade Tree UI");
            if (treeRoot == null) throw new InvalidOperationException("The existing Upgrade Tree UI is required.");
            Transform panel = treeRoot.Find("Tree Panel");
            EnsureAssets(presentation);
            Transform existingSurround = treeRoot.Find("Tree Surround");
            Image surround = existingSurround != null ? existingSurround.GetComponent<Image>() :
                RaceUIBuilder.Panel(treeRoot, "Tree Surround", Vector2.zero, Vector2.zero, Color.white);
            RectTransform surroundRect = surround.rectTransform;
            surroundRect.anchorMin = Vector2.zero; surroundRect.anchorMax = Vector2.one;
            surroundRect.offsetMin = surroundRect.offsetMax = Vector2.zero;
            surroundRect.SetAsFirstSibling();
            surround.color = new Color(0.035f, 0.04f, 0.06f, 1f);
            surround.raycastTarget = false;
            Image background = panel.GetComponent<Image>();
            background.material = presentation.TreeBackground;
            background.color = Color.white;
            background.raycastTarget = false;
            presentation.TreeBackground.SetFloat("_Aspect", background.rectTransform.sizeDelta.x / background.rectTransform.sizeDelta.y);

            UpgradeTreeRevealUI reveal = Ensure<UpgradeTreeRevealUI>(treeRoot.gameObject);
            RaceSceneBaker.SetReference(reveal, "_settings", settings);
            UpgradeTreeRules rules = settings.UpgradeTree.CreateRules();
            for (int i = 0; i < UpgradeTreeRules.NODE_COUNT; i++)
            {
                UpgradeNode id = (UpgradeNode)i;
                Transform node = panel.Find("Unlock " + id);
                if (node == null) continue;
                Ensure<CanvasGroup>(node.gameObject);
                RectTransform rect = node.GetComponent<RectTransform>();
                rect.anchoredPosition = presentation.TreePosition(id);
                rect.sizeDelta = new Vector2(76, 76);
                RaceSceneBaker.SetReference(node.GetComponent<UpgradeTreeNodeUI>(), "_presentation", presentation);
                UpgradeIconState state = node.GetComponent<UpgradeIconState>();
                if (state != null)
                {
                    RaceSceneBaker.SetReference(state, "_presentation", presentation);
                    RaceSceneBaker.SetReference(state, "_icon", node.Find("Icon").GetComponent<Image>());
                }
                node.GetComponent<Button>().navigation = new Navigation { mode = Navigation.Mode.Automatic };
                Transform border = node.Find("Border");
                if (border != null) border.GetComponent<RectTransform>().sizeDelta = new Vector2(76, 76);
                Transform mark = node.Find("State Mark");
                if (mark != null) mark.GetComponent<RectTransform>().anchoredPosition = new Vector2(27, -27);
                UpgradeNode parent = rules.Rule(id).Prerequisite;
                Transform line = panel.Find("Connection " + id);
                if (line == null || parent == UpgradeNode.None) continue;
                Vector2 start = presentation.TreePosition(parent);
                Vector2 end = presentation.TreePosition(id);
                Vector2 delta = end - start;
                RectTransform lineRect = line.GetComponent<RectTransform>();
                lineRect.anchoredPosition = (start + end) * 0.5f;
                lineRect.sizeDelta = new Vector2(Mathf.Max(0, delta.magnitude - 84), 3);
                lineRect.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
                line.GetComponent<Image>().raycastTarget = false;
            }
        }

        private static void EnsureAssets(UIPresentationSettings presentation)
        {
            Shader shader = Shader.Find("Horsecremental/UI/Upgrade Tree Background");
            if (shader == null) throw new InvalidOperationException("Upgrade Tree background shader must compile first.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_PATH);
            bool firstSetup = presentation.TreeBackground == null;
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, MATERIAL_PATH);
            }
            Sprite shape = EnsureSprite(SHAPE_PATH, false);
            Sprite border = EnsureSprite(BORDER_PATH, true);
            SerializedObject data = new SerializedObject(presentation);
            data.FindProperty("_treeBackground").objectReferenceValue = material;
            data.FindProperty("_treeNodeShape").objectReferenceValue = shape;
            data.FindProperty("_treeNodeBorder").objectReferenceValue = border;
            if (firstSetup)
            {
                SerializedProperty positions = data.FindProperty("_treePositions");
                positions.arraySize = POSITIONS.Length;
                for (int i = 0; i < POSITIONS.Length; i++) positions.GetArrayElementAtIndex(i).vector2Value = POSITIONS[i];
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(material);
        }

        private static Sprite EnsureSprite(string path, bool border)
        {
            if (!File.Exists(path))
            {
                Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                Color[] pixels = new Color[64 * 64];
                for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
                {
                    float dx = Mathf.Abs(x - 31.5f), dy = Mathf.Abs(y - 31.5f);
                    bool outer = Mathf.Max(dx, dy) <= 30 && dx + dy <= 43;
                    bool inner = Mathf.Max(dx, dy) < 27 && dx + dy < 39;
                    pixels[y * 64 + x] = outer && (!border || !inner) ? Color.white : Color.clear;
                }
                texture.SetPixels(pixels); texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static T Ensure<T>(GameObject target) where T : Component
            => target.TryGetComponent(out T component) ? component : target.AddComponent<T>();
    }
}
