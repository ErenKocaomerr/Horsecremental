using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing.Tests
{
    public sealed class PresentationSceneTests
    {
        [Test]
        public void AllProjectScenesUseM6x11TextMeshPro()
        {
            string original = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Racing/Resources/Fonts/m6x11 SDF.asset");
            Assert.That(font, Is.Not.Null);
            Assert.That(TMP_Settings.defaultFontAsset, Is.EqualTo(font));
            try
            {
                foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
                {
                    var scene = EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guid));
                    foreach (GameObject root in scene.GetRootGameObjects())
                    {
                        Assert.That(root.GetComponentsInChildren<Text>(true), Is.Empty, scene.path);
                        Assert.That(root.GetComponentsInChildren<TextMesh>(true), Is.Empty, scene.path);
                        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                        {
                            Assert.That(text.font, Is.EqualTo(font), scene.path + "/" + text.name);
                            string missing = new string(text.text.Where(c => !char.IsControl(c) && !font.HasCharacter(c)).Distinct().ToArray());
                            Assert.That(missing, Is.Empty, text.name + " has unsupported glyphs");
                        }
                    }
                }
            }
            finally
            {
                EditorSceneManager.OpenScene(string.IsNullOrEmpty(original) ? "Assets/Racing/Scenes/Milestone3World.unity" : original);
            }
        }

        [Test]
        public void EveryRaceKeepsItsAuthoredArtAndCorrectFinishAndHurdles()
        {
            string original = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            try
            {
                EditorSceneManager.OpenScene("Assets/Racing/Scenes/Milestone3World.unity");
                GameplayWorld world = Object.FindAnyObjectByType<GameplayWorld>();
                SerializedObject prototype = new SerializedObject(world.GetComponent<RacePrototype>());
                ValidateTrack(world.transform.Find("Generated Race/Prototype Track"),
                    AssetDatabase.LoadAssetAtPath<RaceSettings>("Assets/Racing/Config/PrototypeRace.asset"));
                SerializedProperty tracks = prototype.FindProperty("_trainingTracks");
                for (int tier = 0; tier < tracks.arraySize; tier++)
                    ValidateTrack(((GameObject)tracks.GetArrayElementAtIndex(tier).objectReferenceValue).transform,
                        world.TrainingLadder.Race(tier));
            }
            finally
            {
                EditorSceneManager.OpenScene(string.IsNullOrEmpty(original) ? "Assets/Racing/Scenes/Milestone3World.unity" : original);
            }
        }

        private static void ValidateTrack(Transform track, RaceSettings settings)
        {
            Transform art = track.Find("Main Race Environment Art");
            Assert.That(art, Is.Not.Null, track.name);
            Assert.That(art.Find("Finish Flag").localPosition.x, Is.EqualTo(settings.TrackLength));
            SpriteRenderer[] renderers = art.GetComponentsInChildren<SpriteRenderer>(true);
            Assert.That(renderers.All(r => r.enabled && r.sprite != null), Is.True, track.name);
            Assert.That(renderers.Count(r => r.name.StartsWith("Timber Hurdle")), Is.EqualTo(settings.CreateObstacles().Length * 4));
            foreach (SpriteRenderer primitive in track.GetComponentsInChildren<SpriteRenderer>(true))
                if (!primitive.transform.IsChildOf(art)) Assert.That(primitive.enabled, Is.False);
        }
    }
}
