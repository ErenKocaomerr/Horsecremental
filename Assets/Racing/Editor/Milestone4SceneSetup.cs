using System;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing.Editor
{
    // Updates the existing world. It never recreates the race or makes another gameplay scene.
    public static class Milestone4SceneSetup
    {
        public const string WORKSHOP_PATH = "Assets/Racing/Config/Workshop.asset";
        [MenuItem("Horsecremental/Setup Milestone 4 World")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(Milestone3SceneSetup.WORLD_PATH);
            GameplayWorld world = UnityEngine.Object.FindAnyObjectByType<GameplayWorld>();
            if (world == null) throw new InvalidOperationException("The existing Milestone 3 world is required.");
            if (world.PlaygroundSettings != null) { Milestone45SceneSetup.Apply(); return; }
            ProgressionSettings settings = AssetDatabase.LoadAssetAtPath<ProgressionSettings>(Milestone2SceneSetup.SETTINGS_PATH);
            WorkshopSettings workshop = AssetDatabase.LoadAssetAtPath<WorkshopSettings>(WORKSHOP_PATH);
            if (workshop == null)
            { workshop = ScriptableObject.CreateInstance<WorkshopSettings>(); AssetDatabase.CreateAsset(workshop, WORKSHOP_PATH); }
            RaceSceneBaker.SetReference(settings, "_workshop", workshop);
            ConfigureWorld(world.Settings);
            ConfigureRivals();
            Transform parent = world.transform;
            string[] replaced = { "Management UI", "Currency UI", "Farm UI", "Blacksmith UI", "Area Navigation UI", "Blacksmith Workstation" };
            foreach (string name in replaced)
            { Transform old = parent.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            Camera physical = UnityEngine.Object.FindAnyObjectByType<Camera>();
            WorldCameraNavigation navigation = world.GetComponent<WorldCameraNavigation>();
            RaceSceneBaker.SetReference(navigation, "_farm", Camera(parent, "Farm Cinemachine", new Vector3(-43, 0, -10), physical));
            RaceSceneBaker.SetReference(navigation, "_blacksmith", Camera(parent, "Blacksmith Cinemachine", new Vector3(-9, 0, -10), physical));
            physical.GetComponent<CinemachineBrain>().DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, world.Settings.CameraBlend);
            for (int i = 0; i < 4; i++)
            {
                Transform plot = parent.Find("Carrot Plot " + i); Transform leaves = parent.Find("Carrot Leaves " + i);
                if (plot != null) { plot.localPosition = new Vector3(-45 + i * 1.2f, -2, 0); plot.localScale = new Vector3(0.8f, 0.7f, 1); }
                if (leaves != null) leaves.localPosition = new Vector3(-45 + i * 1.2f, -1.5f, 0);
            }
            BuildForge(parent, settings);
            Milestone4UIBuilder.Build(parent, world, settings);
            RaceUI raceUI = world.GetComponentInChildren<RaceUI>(true);
            raceUI.transform.Find("Race entry/Description").GetComponent<Text>().text = "Fixed rivals: 23 / 24 / 25 Speed\nCraft equipment and improve Stable production.";
            MinimalUISetup.Apply(world, settings);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("Milestone 4 saved in the existing world: Metal, equipment, forge and production upgrades.");
        }
        private static CinemachineCamera Camera(Transform parent, string name, Vector3 position, Camera physical)
        {
            Transform existing = parent.Find(name);
            GameObject root = existing == null ? new GameObject(name, typeof(CinemachineCamera)) : existing.gameObject;
            root.transform.SetParent(parent, false); root.transform.localPosition = position;
            CinemachineCamera camera = root.GetComponent<CinemachineCamera>(); camera.Lens = LensSettings.FromCamera(physical); camera.Priority.Value = 0; return camera;
        }
        private static void ConfigureWorld(WorldSettings settings)
        {
            SerializedObject data = new SerializedObject(settings);
            data.FindProperty("_trainingMetal").intValue = 2; data.FindProperty("_cameraBlend").floatValue = 0.6f;
            data.FindProperty("_recommendedPower").floatValue = 245f; data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void ConfigureRivals()
        {
            string[] names = { "RivalSteady", "RivalQuick", "RivalFast" }; float[] speeds = { 23, 24, 25 };
            for (int i = 0; i < 3; i++)
            {
                HorseStats stats = AssetDatabase.LoadAssetAtPath<HorseStats>("Assets/Racing/Config/" + names[i] + ".asset");
                SerializedObject data = new SerializedObject(stats); data.FindProperty("_speed").floatValue = speeds[i];
                data.ApplyModifiedPropertiesWithoutUndo();
            }
        }
        private static void BuildForge(Transform parent, ProgressionSettings settings)
        {
            GameObject root = new GameObject("Blacksmith Workstation"); root.transform.SetParent(parent, false); root.transform.localPosition = new Vector3(-9, 4.2f, 0);
            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Racing/Generated/SolidSprite.asset");
            Rect(root.transform, square, "Workshop floor", new Vector2(0, -2), new Vector2(9, 0.3f), new Color(0.4f, 0.28f, 0.16f));
            Rect(root.transform, square, "Forge masonry", new Vector2(-1.2f, 0), new Vector2(2.4f, 2.4f), new Color(0.23f, 0.23f, 0.27f));
            SpriteRenderer glow = Rect(root.transform, square, "Forge glow", new Vector2(-1.2f, -0.35f), new Vector2(1.5f, 1.1f), new Color(1, 0.4f, 0.1f));
            glow.sortingOrder = 14;
            Rect(root.transform, square, "Workbench", new Vector2(2, -0.9f), new Vector2(3, 0.4f), new Color(0.5f, 0.3f, 0.15f));
            Rect(root.transform, square, "Workbench leg left", new Vector2(0.9f, -1.5f), new Vector2(0.25f, 0.9f), new Color(0.35f, 0.2f, 0.1f));
            Rect(root.transform, square, "Workbench leg right", new Vector2(3.1f, -1.5f), new Vector2(0.25f, 0.9f), new Color(0.35f, 0.2f, 0.1f));
            Rect(root.transform, square, "Anvil base", new Vector2(1.8f, -0.3f), new Vector2(0.5f, 0.7f), new Color(0.3f, 0.34f, 0.4f));
            Rect(root.transform, square, "Anvil top", new Vector2(1.8f, 0.05f), new Vector2(1.5f, 0.25f), new Color(0.65f, 0.7f, 0.75f));
            Rect(root.transform, square, "Metal crate", new Vector2(-3.2f, -1.25f), new Vector2(1.1f, 1.1f), new Color(0.4f, 0.27f, 0.14f));
            Transform hammer = new GameObject("Hammer pivot").transform; hammer.SetParent(root.transform, false); hammer.localPosition = new Vector3(2.1f, 0.8f, 0);
            Rect(hammer, square, "Hammer handle", new Vector2(0, -0.3f), new Vector2(0.12f, 0.8f), new Color(0.55f, 0.35f, 0.16f));
            Rect(hammer, square, "Hammer head", new Vector2(0, 0.15f), new Vector2(0.55f, 0.3f), new Color(0.7f, 0.74f, 0.8f));
            Transform sparkRoot = new GameObject("Anvil sparks").transform; sparkRoot.SetParent(root.transform, false); sparkRoot.localPosition = new Vector3(1.8f, 0, 0);
            Transform[] sparks = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                SpriteRenderer spark = Rect(sparkRoot, square, "Spark " + i, Vector2.zero, new Vector2(0.08f, 0.16f), new Color(1, 0.85f, 0.3f));
                spark.sortingOrder = 16; sparks[i] = spark.transform;
            }
            ForgeVisual visual = root.AddComponent<ForgeVisual>(); RaceSceneBaker.SetReference(visual, "_settings", settings);
            RaceSceneBaker.SetReference(visual, "_glow", glow); RaceSceneBaker.SetReference(visual, "_hammer", hammer); RaceSceneBaker.SetArray(visual, "_sparks", sparks);
        }
        private static SpriteRenderer Rect(Transform parent, Sprite sprite, string name, Vector2 position, Vector2 size, Color color)
            => RaceTrackBuilder.Rectangle(parent, sprite, name, position, size, color, 12).GetComponent<SpriteRenderer>();
    }
}
