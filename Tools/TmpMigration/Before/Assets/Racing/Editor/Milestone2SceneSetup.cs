using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Horsecremental.Racing.Editor
{
    public static class Milestone2SceneSetup
    {
        public const string STABLE_PATH = "Assets/Racing/Scenes/Milestone2Stable.unity";
        public const string SETTINGS_PATH = "Assets/Racing/Config/Progression.asset";

        [MenuItem("Horsecremental/Legacy/Open Milestone 2 Stable")]
        public static void OpenStable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(STABLE_PATH);
        }

        [MenuItem("Horsecremental/Legacy/Setup Milestone 2 Loop")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Bake();
        }

        public static void Bake()
        {
            ProgressionSettings settings = EnsureSettings();
            UpgradeRace(settings);
            BuildStable(settings);
            // Include existing sample/template cameras and every scene Canvas in the requested conversion.
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = EditorSceneManager.OpenScene(path);
                foreach (Camera camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include))
                    if (!camera.TryGetComponent(out Unity.Cinemachine.CinemachineBrain unused)) CinemachineSetup.Convert(camera);
                foreach (Canvas canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
                {
                    if (!canvas.TryGetComponent(out CanvasScaler scaler)) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.matchWidthOrHeight = 0.5f;
                }
                EditorSceneManager.SaveScene(scene);
            }
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(STABLE_PATH, true),
                new EditorBuildSettingsScene(RacePrototypeMenu.SCENE_PATH, true)
            };
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(STABLE_PATH);
            Debug.Log("Milestone 2 Stable and Race saved; Cinemachine and 1920x1080 Canvas scaling configured.");
        }

        internal static ProgressionSettings EnsureSettings()
        {
            ProgressionSettings settings = AssetDatabase.LoadAssetAtPath<ProgressionSettings>(SETTINGS_PATH);
            if (settings != null) return settings;
            settings = ScriptableObject.CreateInstance<ProgressionSettings>();
            AssetDatabase.CreateAsset(settings, SETTINGS_PATH);
            RaceSceneBaker.SetReference(settings, "_baseHorse", AssetDatabase.LoadAssetAtPath<HorseStats>("Assets/Racing/Config/PlayerHorse.asset"));
            SerializedObject serialized = new SerializedObject(settings);
            Configure(serialized, "_speed", 60, 0.75f);
            Configure(serialized, "_acceleration", 50, 3f);
            Configure(serialized, "_boost", 60, 1f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(settings);
            return AssetDatabase.LoadAssetAtPath<ProgressionSettings>(SETTINGS_PATH);
        }

        private static void Configure(SerializedObject serialized, string name, int cost, float increase)
        {
            SerializedProperty property = serialized.FindProperty(name);
            property.FindPropertyRelative("_baseCost").intValue = cost;
            property.FindPropertyRelative("_growthRate").floatValue = 1.45f;
            property.FindPropertyRelative("_increase").floatValue = increase;
        }

        private static void UpgradeRace(ProgressionSettings settings)
        {
            var scene = EditorSceneManager.OpenScene(RacePrototypeMenu.SCENE_PATH);
            RacePrototype prototype = UnityEngine.Object.FindAnyObjectByType<RacePrototype>();
            if (prototype == null) throw new InvalidOperationException("Milestone 1 RacePrototype is missing.");
            RaceCamera adapter = UnityEngine.Object.FindAnyObjectByType<RaceCamera>();
            Camera camera = adapter.GetComponent<Camera>();
            if (!camera.TryGetComponent(out Unity.Cinemachine.CinemachineBrain unused))
            {
                HorseController player = UnityEngine.Object.FindAnyObjectByType<HorseController>();
                CinemachineSetup.ConfigureRace(camera, adapter, player.GetComponent<HorseView>().RenderRoot);
            }
            RaceUI oldUI = UnityEngine.Object.FindAnyObjectByType<RaceUI>();
            Transform parent = oldUI.transform.parent;
            UnityEngine.Object.DestroyImmediate(oldUI.gameObject);
            RaceSettings raceSettings = AssetDatabase.LoadAssetAtPath<RaceSettings>("Assets/Racing/Config/PrototypeRace.asset");
            RaceUI ui = RaceUIBuilder.Build(parent, raceSettings.OpponentStats.Length + 1);
            RaceSceneBaker.SetReference(prototype, "_ui", ui);
            RaceSceneBaker.SetReference(prototype, "_progressionSettings", settings);
            EditorSceneManager.SaveScene(scene);
        }

        private static void BuildStable(ProgressionSettings settings)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("Milestone 2 Stable");
            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Racing/Generated/SolidSprite.asset");
            if (square == null) throw new InvalidOperationException("Milestone 1 generated sprites are missing.");
            BuildEnvironment(root.transform, square);
            Sprite horseSprite = null;
            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath("Assets/Racing/Generated/Horse0Frames.png"))
                if (asset is Sprite sprite && sprite.name == "Horse0Idle") horseSprite = sprite;
            if (horseSprite == null) throw new InvalidOperationException("Player horse idle frame is missing.");
            GameObject horse = new GameObject("Stable Player Horse", typeof(SpriteRenderer));
            horse.transform.SetParent(root.transform, false);
            horse.transform.position = new Vector3(-6f, -1f, 0f);
            horse.transform.localScale = Vector3.one * 2.6f;
            horse.GetComponent<SpriteRenderer>().sprite = horseSprite;
            horse.GetComponent<SpriteRenderer>().sortingOrder = 10;

            GameObject cameraObject = new GameObject("Stable Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.16f, 0.32f, 0.23f);
            CinemachineSetup.Convert(camera);
            GameObject events = new GameObject("Stable Event System", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(root.transform, false);
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            BuildUI(root.transform, settings);
            EditorSceneManager.SaveScene(scene, STABLE_PATH);
        }

        private static void BuildEnvironment(Transform parent, Sprite square)
        {
            const string BACKGROUND = "Assets/Sprites/Jungle Asset Pack/Jungle Asset Pack/parallax background/plx-1.png";
            Sprite background = AssetDatabase.LoadAssetAtPath<Sprite>(BACKGROUND);
            if (background != null)
            {
                GameObject trees = new GameObject("Existing Environment Background", typeof(SpriteRenderer));
                trees.transform.SetParent(parent, false);
                trees.GetComponent<SpriteRenderer>().sprite = background;
                trees.GetComponent<SpriteRenderer>().sortingOrder = -50;
                trees.transform.localScale = new Vector3(30f / background.bounds.size.x, 16f / background.bounds.size.y, 1f);
            }
            RaceTrackBuilder.Rectangle(parent, square, "Paddock", new Vector2(0f, -4.5f), new Vector2(35f, 6f), new Color(0.27f, 0.33f, 0.17f), -20);
            RaceTrackBuilder.Rectangle(parent, square, "Stable Wall", new Vector2(-6f, 1.8f), new Vector2(10f, 5.8f), new Color(0.35f, 0.23f, 0.12f), -10);
            RaceTrackBuilder.Rectangle(parent, square, "Stall", new Vector2(-6f, 1f), new Vector2(6.7f, 4f), new Color(0.12f, 0.15f, 0.1f), -9);
            for (int i = 0; i < 6; i++)
                RaceTrackBuilder.Rectangle(parent, square, "Fence Post", new Vector2(-11f + i * 2f, -2.3f), new Vector2(0.18f, 1.5f), new Color(0.6f, 0.43f, 0.22f), 15);
            RaceTrackBuilder.Rectangle(parent, square, "Fence Rail", new Vector2(-6f, -2.2f), new Vector2(10.2f, 0.15f), new Color(0.65f, 0.48f, 0.26f), 16);
        }

        private static void BuildUI(Transform parent, ProgressionSettings settings)
        {
            // Scene changes/imports can replace the native instance of a newly created asset.
            settings = AssetDatabase.LoadAssetAtPath<ProgressionSettings>(SETTINGS_PATH);
            if (settings == null) throw new InvalidOperationException("Progression settings must be saved before building the Stable.");
            GameObject canvas = new GameObject("Stable UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(StableUI));
            canvas.transform.SetParent(parent, false);
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            StableUI ui = canvas.GetComponent<StableUI>();
            RaceSceneBaker.SetReference(ui, "_settings", settings);
            RaceUIBuilder.Label(canvas.transform, "Stable Title", new Vector2(-460, 440), new Vector2(800, 90), 52).text = "AHIR — MILESTONE 2";
            RaceUIBuilder.Label(canvas.transform, "Horse Title", new Vector2(-460, 160), new Vector2(650, 70), 40).text = "SENİN ATIN";
            var panel = RaceUIBuilder.Panel(canvas.transform, "Horse Upgrades", new Vector2(460, 40), new Vector2(740, 840), new Color(0.05f, 0.1f, 0.08f, 0.96f));
            RaceUIBuilder.Label(panel.transform, "Title", new Vector2(0, 350), new Vector2(700, 60), 38).text = "AT YÜKSELTMELERİ";
            Text coins = RaceUIBuilder.Label(panel.transform, "Coins", new Vector2(0, 285), new Vector2(700, 60), 36);
            coins.color = new Color(1f, 0.83f, 0.2f);
            RaceSceneBaker.SetReference(ui, "_coins", coins);
            Text stats = RaceUIBuilder.Label(panel.transform, "Current Horse Stats", new Vector2(0, 170), new Vector2(650, 145), 31);
            RaceSceneBaker.SetReference(ui, "_stats", stats);
            Button[] buttons = new Button[3];
            Text[] labels = new Text[3];
            string[] names = { "Upgrade Speed", "Upgrade Acceleration", "Upgrade Boost Power" };
            for (int i = 0; i < 3; i++)
            {
                buttons[i] = RaceUIBuilder.Button(panel.transform, names[i], string.Empty, new Vector2(0, 10 - i * 138));
                buttons[i].GetComponent<RectTransform>().sizeDelta = new Vector2(640, 120);
                labels[i] = buttons[i].GetComponentInChildren<Text>();
                labels[i].rectTransform.sizeDelta = new Vector2(610, 114);
                labels[i].fontSize = 28;
            }
            RaceSceneBaker.SetArray(ui, "_upgrades", buttons);
            RaceSceneBaker.SetArray(ui, "_upgradeLabels", labels);
            Text feedback = RaceUIBuilder.Label(canvas.transform, "Upgrade Feedback", new Vector2(0, -430), new Vector2(1750, 65), 32);
            feedback.text = "Yarış → Coins → Yükseltme → Daha güçlü at. İlk yükseltme 1–2 yarışta!";
            feedback.color = new Color(1f, 0.85f, 0.3f);
            RaceSceneBaker.SetReference(ui, "_feedback", feedback);
            Button start = RaceUIBuilder.Button(canvas.transform, "Start Race", "YARIŞI BAŞLAT", new Vector2(-460, -270));
            start.GetComponent<RectTransform>().sizeDelta = new Vector2(520, 100);
            start.GetComponentInChildren<Text>().rectTransform.sizeDelta = new Vector2(500, 95);
            start.GetComponentInChildren<Text>().fontSize = 36;
            RaceSceneBaker.SetReference(ui, "_startRace", start);
        }
    }
}
