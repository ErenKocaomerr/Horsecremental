using System;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Horsecremental.Racing.Editor
{
    public static class Milestone3SceneSetup
    {
        public const string WORLD_PATH = "Assets/Racing/Scenes/Milestone3World.unity";
        public const string WORLD_SETTINGS_PATH = "Assets/Racing/Config/World.asset";

        [MenuItem("Horsecremental/Open Milestone 3 World")]
        public static void Open()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(WORLD_PATH);
        }

        [MenuItem("Horsecremental/Setup Milestone 3 World")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            ConfigureBalance();
            WorldSettings settings = AssetDatabase.LoadAssetAtPath<WorldSettings>(WORLD_SETTINGS_PATH);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<WorldSettings>();
                AssetDatabase.CreateAsset(settings, WORLD_SETTINGS_PATH);
            }
            AssetDatabase.SaveAssets();
            settings = AssetDatabase.LoadAssetAtPath<WorldSettings>(WORLD_SETTINGS_PATH);
            var scene = EditorSceneManager.OpenScene(RacePrototypeMenu.SCENE_PATH);
            EditorSceneManager.SaveScene(scene, WORLD_PATH);
            RacePrototype prototype = UnityEngine.Object.FindAnyObjectByType<RacePrototype>();
            prototype.name = "Milestone 3 World";
            GameplayWorld world = prototype.gameObject.AddComponent<GameplayWorld>();
            WorldCameraNavigation navigation = prototype.gameObject.AddComponent<WorldCameraNavigation>();
            RaceSceneBaker.SetReference(prototype, "_world", world);
            RaceSceneBaker.SetReference(world, "_settings", settings);
            RaceSceneBaker.SetReference(world, "_cameras", navigation);

            Camera camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            camera.name = "World Camera";
            // Pixel-perfect quantization cancels small lens changes and produces stepped blends.
            if (camera.TryGetComponent(out PixelPerfectCamera pixel)) UnityEngine.Object.DestroyImmediate(pixel);
            CinemachineCamera follow = UnityEngine.Object.FindAnyObjectByType<CinemachineCamera>();
            if (follow.TryGetComponent(out CinemachinePixelPerfect extension)) UnityEngine.Object.DestroyImmediate(extension);
            follow.name = "Main Race Cinemachine";
            follow.Priority.Value = 0;
            CinemachineBrain brain = camera.GetComponent<CinemachineBrain>();
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, settings.CameraBlend);
            CinemachineCamera stable = StaticCamera(prototype.transform, "Stable Cinemachine", new Vector3(-26f, 0f, -10f), camera);
            stable.Priority.Value = 20;
            CinemachineCamera entry = StaticCamera(prototype.transform, "Race Entry Cinemachine", new Vector3(7f, 0.5f, -10f), camera);
            RaceSceneBaker.SetReference(navigation, "_stable", stable);
            RaceSceneBaker.SetReference(navigation, "_entry", entry);
            RaceSceneBaker.SetReference(navigation, "_race", follow);
            RaceSceneBaker.SetReference(navigation, "_brain", brain);
            camera.transform.position = stable.transform.position;

            var stableScene = EditorSceneManager.OpenScene(Milestone2SceneSetup.STABLE_PATH, OpenSceneMode.Additive);
            GameObject stableRoot = stableScene.GetRootGameObjects()[0];
            GameObject environment = UnityEngine.Object.Instantiate(stableRoot);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(environment, scene);
            environment.name = "Stable and Farm Environment";
            environment.transform.SetParent(prototype.transform, false);
            environment.transform.position = new Vector3(-20f, 0f, 0f);
            foreach (Canvas canvas in environment.GetComponentsInChildren<Canvas>(true)) UnityEngine.Object.DestroyImmediate(canvas.gameObject);
            foreach (Camera duplicate in environment.GetComponentsInChildren<Camera>(true)) UnityEngine.Object.DestroyImmediate(duplicate.gameObject);
            foreach (CinemachineCamera duplicate in environment.GetComponentsInChildren<CinemachineCamera>(true)) UnityEngine.Object.DestroyImmediate(duplicate.gameObject);
            foreach (EventSystem duplicate in environment.GetComponentsInChildren<EventSystem>(true)) UnityEngine.Object.DestroyImmediate(duplicate.gameObject);
            EditorSceneManager.CloseScene(stableScene, true);
            EditorSceneManager.SetActiveScene(scene);
            settings = AssetDatabase.LoadAssetAtPath<WorldSettings>(WORLD_SETTINGS_PATH);
            RaceSceneBaker.SetReference(world, "_settings", settings);
            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Racing/Generated/SolidSprite.asset");
            RaceTrackBuilder.Rectangle(prototype.transform, square, "Stable Ground", new Vector2(-26f, 0f),
                new Vector2(90f, 30f), new Color(0.16f, 0.32f, 0.23f), -30);
            RaceTrackBuilder.Rectangle(prototype.transform, square, "Stable Track Connection", new Vector2(-15f, -3.8f),
                new Vector2(30f, 2f), new Color(0.47f, 0.36f, 0.22f), -19);
            for (int i = 0; i < 4; i++)
            {
                RaceTrackBuilder.Rectangle(prototype.transform, square, "Carrot Plot " + i, new Vector2(-25f + i * 1.2f, -2f),
                    new Vector2(0.8f, 0.7f), new Color(0.38f, 0.2f, 0.09f), 5);
                RaceTrackBuilder.Rectangle(prototype.transform, square, "Carrot Leaves " + i, new Vector2(-25f + i * 1.2f, -1.5f),
                    new Vector2(0.25f, 0.5f), new Color(0.37f, 0.67f, 0.22f), 6);
            }
            GameObject light = new GameObject("World Directional Light", typeof(Light));
            light.transform.SetParent(prototype.transform, false);
            light.GetComponent<Light>().type = LightType.Directional;
            light.GetComponent<Light>().intensity = 0.6f;

            ProgressionSettings progression = AssetDatabase.LoadAssetAtPath<ProgressionSettings>(Milestone2SceneSetup.SETTINGS_PATH);
            RaceUI raceUI = UnityEngine.Object.FindAnyObjectByType<RaceUI>();
            RaceSceneBaker.SetReference(raceUI, "_world", world);
            raceUI.transform.Find("Race result/Restart Race").gameObject.SetActive(false);
            raceUI.transform.Find("Race entry/Start Race").GetComponentInChildren<Text>(true).text = "START MAIN RACE";
            raceUI.transform.Find("Race entry/Title").GetComponent<Text>().text = "MAIN RACE";
            raceUI.transform.Find("Race entry/Description").GetComponent<Text>().text = "Fixed rivals: 14 / 14.5 / 15 speed\nTrain and upgrade to beat them.";
            raceUI.transform.Find("Controls").GetComponent<Text>().text = "SPACE: rhythmic boost taps   |   E: jump\nOrange hurdles slow you down. Jump near the yellow marker.";
            raceUI.transform.Find("Race result/Return to Stable").GetComponentInChildren<Text>(true).text = "RETURN TO STABLE";
            raceUI.transform.Find("Race result/Column place").GetComponent<Text>().text = "Place";
            raceUI.transform.Find("Race result/Column name").GetComponent<Text>().text = "Horse";
            raceUI.transform.Find("Race result/Column time").GetComponent<Text>().text = "Finish time";
            foreach (RaceParticipant participant in prototype.GetComponentsInChildren<RaceParticipant>())
            {
                SerializedObject participantData = new SerializedObject(participant);
                participantData.FindProperty("_displayName").stringValue = participant.IsPlayer ? "Your horse" : participant.name.Replace("AI Horse", "Rival");
                participantData.ApplyModifiedPropertiesWithoutUndo();
            }
            CanvasGroup stableGroup = Milestone3UIBuilder.BuildStable(prototype.transform, progression);
            ManagementUI management = Milestone3UIBuilder.BuildManagement(prototype.transform, world, progression);
            Milestone3UIBuilder.BuildCurrency(prototype.transform, progression);
            RaceSceneBaker.SetReference(world, "_stableUI", stableGroup.gameObject);
            RaceSceneBaker.SetReference(world, "_stableGroup", stableGroup);
            RaceSceneBaker.SetReference(world, "_managementUI", management.gameObject);
            RaceSceneBaker.SetReference(world, "_raceUI", raceUI.gameObject);
            foreach (Button button in prototype.GetComponentsInChildren<Button>(true)) button.gameObject.AddComponent<ButtonPulse>();
            raceUI.gameObject.SetActive(false);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(WORLD_PATH, true) };
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Milestone 3 World saved. Stable, Farm, and Main Race share one scene.");
        }

        private static CinemachineCamera StaticCamera(Transform parent, string name, Vector3 position, Camera camera)
        {
            GameObject root = new GameObject(name, typeof(CinemachineCamera));
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            CinemachineCamera result = root.GetComponent<CinemachineCamera>();
            result.Lens = LensSettings.FromCamera(camera);
            result.Priority.Value = 0;
            return result;
        }

        private static void ConfigureBalance()
        {
            string[] names = { "PlayerHorse", "RivalSteady", "RivalQuick", "RivalFast" };
            float[] speeds = { 10f, 14f, 14.5f, 15f };
            for (int i = 0; i < names.Length; i++)
            {
                HorseStats stats = AssetDatabase.LoadAssetAtPath<HorseStats>("Assets/Racing/Config/" + names[i] + ".asset");
                SerializedObject data = new SerializedObject(stats);
                data.FindProperty("_speed").floatValue = speeds[i];
                data.FindProperty("_boostPerTap").floatValue = 0.06f;
                data.FindProperty("_boostDecayPerSecond").floatValue = 0.02f;
                data.FindProperty("_boostHighDrain").floatValue = 0.58f;
                data.ApplyModifiedPropertiesWithoutUndo();
            }
            ProgressionSettings progression = AssetDatabase.LoadAssetAtPath<ProgressionSettings>(Milestone2SceneSetup.SETTINGS_PATH);
            SerializedObject settings = new SerializedObject(progression);
            int[] rewards = { 500, 250, 150, 100 };
            SerializedProperty array = settings.FindProperty("_placeRewards");
            array.arraySize = rewards.Length;
            for (int i = 0; i < rewards.Length; i++) array.GetArrayElementAtIndex(i).intValue = rewards[i];
            settings.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }
    }
}
