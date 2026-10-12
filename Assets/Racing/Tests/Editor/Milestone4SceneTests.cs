using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Horsecremental.Racing.Tests
{
    public sealed class Milestone4SceneTests
    {
        [Test]
        public void SavedWorldKeepsOneCentralCoordinatorAndConfiguredWorkstations()
        {
            EditorSceneManager.OpenScene("Assets/Racing/Scenes/Milestone3World.unity");
            Assert.That(Object.FindObjectsByType<GameplayWorld>().Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<BlacksmithUI>().Length, Is.Zero);
            Assert.That(Object.FindObjectsByType<FarmUI>().Length, Is.Zero);
            Assert.That(Object.FindObjectsByType<ProductionUpgradeUI>().Length, Is.Zero);
            Assert.That(Object.FindObjectsByType<PlaygroundUpgradeUI>().Length, Is.EqualTo(3));
            Assert.That(Object.FindObjectsByType<TrainingTrackView>().Length, Is.Zero);
            Assert.That(Object.FindObjectsByType<TrainingSelectionUI>().Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<FarmWorldView>().Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<WorldPickupManager>().Length, Is.EqualTo(1));
            Assert.That(GameObject.Find("Blacksmith Workstation"), Is.Not.Null);
            Assert.That(GameObject.Find("Farm Cinemachine"), Is.Not.Null);
            Assert.That(GameObject.Find("Blacksmith Cinemachine"), Is.Not.Null);
            foreach (AreaPanel panel in Object.FindObjectsByType<AreaPanel>())
                Assert.That(new SerializedObject(panel).FindProperty("_world").objectReferenceValue, Is.Not.Null);
            ProgressionSettings settings = AssetDatabase.LoadAssetAtPath<ProgressionSettings>("Assets/Racing/Config/Progression.asset");
            Assert.That(settings.Workshop, Is.Not.Null);
            foreach (EquipmentType type in System.Enum.GetValues(typeof(EquipmentType))) Assert.That(settings.Workshop.TierCount(type), Is.EqualTo(5));
            Assert.That(AssetDatabase.AssetPathToGUID("Assets/Racing/Scenes/Milestone3World.unity"), Is.EqualTo("8789f1f4a37cec24183629f77a4555ec"));
        }
    }
}
