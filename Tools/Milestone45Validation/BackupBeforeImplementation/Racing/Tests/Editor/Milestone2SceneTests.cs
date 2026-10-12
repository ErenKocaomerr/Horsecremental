using NUnit.Framework;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing.Tests
{
    public sealed class Milestone2SceneTests
    {
        [Test]
        public void StableIsFirstBuildSceneAndAllSavedSceneCamerasAndCanvasesAreConfigured()
        {
            Assert.That(EditorBuildSettings.scenes.Length, Is.EqualTo(1));
            Assert.That(EditorBuildSettings.scenes[0].path, Is.EqualTo("Assets/Racing/Scenes/Milestone3World.unity"));
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
            {
                EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guid));
                foreach (Camera camera in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include))
                    Assert.That(camera.GetComponent<CinemachineBrain>(), Is.Not.Null, camera.name);
                Assert.That(Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include).Length, Is.GreaterThan(0));
                foreach (CanvasScaler scaler in Object.FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include))
                {
                    Assert.That(scaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
                    Assert.That(scaler.referenceResolution, Is.EqualTo(new Vector2(1920, 1080)));
                }
            }
            EditorSceneManager.OpenScene("Assets/Racing/Scenes/Milestone2Stable.unity");
            StableUI stable = Object.FindAnyObjectByType<StableUI>();
            Assert.That(stable, Is.Not.Null);
            Assert.That(new SerializedObject(stable).FindProperty("_settings").objectReferenceValue,
                Is.EqualTo(AssetDatabase.LoadAssetAtPath<ProgressionSettings>("Assets/Racing/Config/Progression.asset")));
            Assert.That(GameObject.Find("Stable Player Horse").GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
            Assert.That(Object.FindObjectsByType<Button>().Length, Is.EqualTo(4));
        }
    }
}
