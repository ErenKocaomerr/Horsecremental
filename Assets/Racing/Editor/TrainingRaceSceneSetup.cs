using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing.Editor
{
    // Idempotent, focused migration: retain legacy objects and their GUIDs, disable their gameplay.
    public static class TrainingRaceSceneSetup
    {
        public const string LADDER_PATH = "Assets/Racing/Config/TrainingLadder.asset";
        private static readonly Color PANEL = new Color(0.07f, 0.09f, 0.11f, 0.97f);
        [MenuItem("Horsecremental/Setup Manual Training Races")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != Milestone3SceneSetup.WORLD_PATH)
                throw new InvalidOperationException("Open Milestone3World before applying Training Races.");
            GameplayWorld world = UnityEngine.Object.FindAnyObjectByType<GameplayWorld>();
            if (world == null || world.PlaygroundSettings == null) throw new InvalidOperationException("The existing Stable Playground is required.");
            Apply(world);
            EditorSceneManager.MarkSceneDirty(world.gameObject.scene);
            EditorSceneManager.SaveScene(world.gameObject.scene);
            AssetDatabase.SaveAssets();
        }
        internal static void Apply(GameplayWorld world)
        {
            TrainingLadderSettings ladder = EnsureLadder();
            RacePrototype prototype = world.GetComponent<RacePrototype>();
            Bind(world, "_trainingLadder", ladder); Bind(prototype, "_trainingLadder", ladder);
            DisablePassiveTraining(world);
            BuildTracks(world, prototype, ladder);
            BuildSelection(world);
            BuildTokenHeader(world);
            BuildHorseCards(world);
            RectTransform position = world.transform.Find("Generated Race/Race UI/Position").GetComponent<RectTransform>();
            position.anchorMin = position.anchorMax = new Vector2(1, 1);
            position.anchoredPosition = new Vector2(-155, -38); position.sizeDelta = new Vector2(290, 48);
        }
        private static TrainingLadderSettings EnsureLadder()
        {
            TrainingLadderSettings existing = AssetDatabase.LoadAssetAtPath<TrainingLadderSettings>(LADDER_PATH);
            if (existing != null) return existing; // Never overwrite tuned settings.
            string[] names = { "Training I", "Training II", "Training III" };
            float[] lengths = { 120f, 150f, 180f };
            float[][] speeds = { new[] { 9.8f, 10.2f, 10.6f }, new[] { 11f, 11.5f, 12f }, new[] { 13f, 13.5f, 14f } };
            int[][] coins = { new[] { 100, 75, 50, 30 }, new[] { 160, 120, 80, 48 }, new[] { 240, 180, 120, 72 } };
            int[][] tokens = { new[] { 6, 4, 3, 1 }, new[] { 10, 7, 4, 2 }, new[] { 16, 10, 6, 3 } };
            RaceSettings template = AssetDatabase.LoadAssetAtPath<RaceSettings>("Assets/Racing/Config/PrototypeRace.asset");
            TrainingLadderSettings ladder = ScriptableObject.CreateInstance<TrainingLadderSettings>();
            AssetDatabase.CreateAsset(ladder, LADDER_PATH);
            SerializedObject data = new SerializedObject(ladder);
            SerializedProperty tiers = data.FindProperty("_tiers"); tiers.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                RaceSettings race = UnityEngine.Object.Instantiate(template);
                AssetDatabase.CreateAsset(race, "Assets/Racing/Config/TrainingRace" + (i + 1) + ".asset");
                SerializedObject raceData = new SerializedObject(race);
                raceData.FindProperty("_trackLength").floatValue = lengths[i];
                SerializedProperty obstacles = raceData.FindProperty("_obstacleDistances");
                obstacles.arraySize = i;
                for (int obstacle = 0; obstacle < i; obstacle++) obstacles.GetArrayElementAtIndex(obstacle).floatValue = 55f + obstacle * 65f;
                for (int rival = 0; rival < 3; rival++)
                {
                    HorseStats horse = UnityEngine.Object.Instantiate(template.OpponentStats[rival]);
                    AssetDatabase.CreateAsset(horse, "Assets/Racing/Config/Training" + (i + 1) + "Rival" + (rival + 1) + ".asset");
                    SerializedObject horseData = new SerializedObject(horse);
                    horseData.FindProperty("_speed").floatValue = speeds[i][rival];
                    horseData.FindProperty("_acceleration").floatValue = 10f + i * 2f;
                    horseData.ApplyModifiedPropertiesWithoutUndo();
                    raceData.FindProperty("_opponentStats").GetArrayElementAtIndex(rival).objectReferenceValue = horse;
                }
                raceData.ApplyModifiedPropertiesWithoutUndo();
                SerializedProperty tier = tiers.GetArrayElementAtIndex(i);
                tier.FindPropertyRelative("_name").stringValue = names[i];
                tier.FindPropertyRelative("_race").objectReferenceValue = race;
                SetInts(tier.FindPropertyRelative("_coins"), coins[i]);
                SetInts(tier.FindPropertyRelative("_tokens"), tokens[i]);
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            return ladder;
        }
        private static void DisablePassiveTraining(GameplayWorld world)
        {
            foreach (TrainingTrackView view in world.GetComponentsInChildren<TrainingTrackView>(true))
            { view.enabled = false; view.gameObject.SetActive(false); }
            Transform root = world.transform;
            Transform controls = root.Find("Playground UI/Training Upgrades");
            if (controls != null) controls.gameObject.SetActive(false);
            Transform tree = root.Find("Upgrade Tree UI/Tree Panel");
            foreach (Transform child in tree)
                if (child.name.Contains("TrainingSpeed") || child.name.Contains("TrainingReward")) child.gameObject.SetActive(false);
        }
        private static void BuildTracks(GameplayWorld world, RacePrototype prototype, TrainingLadderSettings ladder)
        {
            Transform previous = world.transform.Find("Manual Training Tracks");
            if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
            Transform tracks = new GameObject("Manual Training Tracks").transform; tracks.SetParent(world.transform, false);
            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Racing/Generated/SolidSprite.asset");
            GameObject[] courses = new GameObject[ladder.Count];
            for (int i = 0; i < courses.Length; i++)
            {
                courses[i] = new GameObject(ladder.Name(i) + " Track"); courses[i].transform.SetParent(tracks, false);
                RaceSettings race = ladder.Race(i);
                RaceTrackBuilder.Build(courses[i].transform, square, race.TrackLength, race.CreateObstacles(), 4);
                courses[i].SetActive(false);
            }
            RaceSceneBaker.SetArray(prototype, "_trainingTracks", courses);
            Bind(prototype, "_mainTrack", world.transform.Find("Generated Race/Prototype Track").gameObject);
        }
        private static void BuildSelection(GameplayWorld world)
        {
            Transform old = world.transform.Find("Training Selection UI");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            Transform navigation = world.transform.Find("Area Navigation UI");
            old = navigation.Find("View Training");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            Button open = RaceUIBuilder.Button(navigation, "View Training", "TRAINING", Vector2.zero);
            string[] names = { "View Stable", "View Training", "View Farm", "View Race", "View Upgrade Tree" };
            for (int i = 0; i < names.Length; i++)
            {
                RectTransform rect = navigation.Find(names[i]).GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0);
                rect.anchoredPosition = new Vector2((i - 2) * 220, 48); rect.sizeDelta = new Vector2(205, 65);
                Text label = rect.GetComponentInChildren<Text>(); label.fontSize = 22; label.rectTransform.sizeDelta = new Vector2(195, 60);
            }
            GameObject canvas = new GameObject("Training Selection UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.transform.SetParent(world.transform, false);
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay; canvas.GetComponent<Canvas>().sortingOrder = 20;
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            Image panel = RaceUIBuilder.Panel(canvas.transform, "Training Selection", new Vector2(0, 30), new Vector2(1080, 560), PANEL);
            RaceUIBuilder.Label(panel.transform, "Title", new Vector2(0, 235), new Vector2(800, 50), 36).text = "TRAINING RACES";
            RaceUIBuilder.Label(panel.transform, "Hint", new Vector2(0, 190), new Vector2(900, 38), 22).text = "Win to unlock the next race. Every finish earns rewards.";
            Button close = RaceUIBuilder.Button(panel.transform, "Close Training", "BACK TO STABLE", new Vector2(0, -230));
            TrainingTierCardUI[] cards = new TrainingTierCardUI[3];
            for (int i = 0; i < cards.Length; i++)
            {
                Image card = RaceUIBuilder.Panel(panel.transform, "Training Card " + (i + 1), new Vector2((i - 1) * 340, -10), new Vector2(320, 340), PANEL);
                TrainingTierCardUI view = card.gameObject.AddComponent<TrainingTierCardUI>();
                Bind(view, "_background", card);
                Bind(view, "_title", RaceUIBuilder.Label(card.transform, "Name", new Vector2(0, 137), new Vector2(300, 40), 30));
                Bind(view, "_status", RaceUIBuilder.Label(card.transform, "Status", new Vector2(0, 100), new Vector2(300, 30), 21));
                Bind(view, "_best", RaceUIBuilder.Label(card.transform, "Best", new Vector2(0, 70), new Vector2(300, 30), 22));
                Bind(view, "_rewards", RaceUIBuilder.Label(card.transform, "Rewards", new Vector2(0, -10), new Vector2(300, 130), 22));
                Bind(view, "_requirement", RaceUIBuilder.Label(card.transform, "Requirement", new Vector2(0, -99), new Vector2(300, 48), 19));
                Button start = RaceUIBuilder.Button(card.transform, "Start Training " + (i + 1), "START RACE", new Vector2(0, -140));
                start.GetComponent<RectTransform>().sizeDelta = new Vector2(275, 42);
                Text label = start.GetComponentInChildren<Text>(); label.fontSize = 22; label.rectTransform.sizeDelta = new Vector2(260, 38);
                Bind(view, "_start", start); cards[i] = view;
            }
            TrainingSelectionUI ui = world.GetComponent<TrainingSelectionUI>();
            if (ui == null) ui = world.gameObject.AddComponent<TrainingSelectionUI>();
            Bind(ui, "_world", world); Bind(ui, "_open", open); Bind(ui, "_close", close);
            RaceSceneBaker.SetArray(ui, "_cards", cards);
            Bind(world, "_trainingSelectionUI", canvas); canvas.SetActive(false);
        }
        private static void BuildTokenHeader(GameplayWorld world)
        {
            Transform root = world.transform.Find("Currency UI");
            Transform old = root.Find("Training Token Header");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            foreach (Transform item in root)
                if (item.name.Contains("Race Tokens")) item.gameObject.SetActive(false);
            MoveHeader(root.Find("Coins"), new Vector2(115, -48), new Vector2(80, 40));
            MoveHeader(root.Find("Coins Icon"), new Vector2(40, -48), new Vector2(32, 32));
            MoveHeader(root.Find("Coins Popup"), new Vector2(105, -82), new Vector2(120, 30));
            MoveHeader(root.Find("Carrots"), new Vector2(575, -48), new Vector2(100, 40));
            MoveHeader(root.Find("Carrots Icon"), new Vector2(500, -48), new Vector2(32, 32));
            MoveHeader(root.Find("Carrots Popup"), new Vector2(565, -82), new Vector2(120, 30));
            Image header = RaceUIBuilder.Panel(root, "Training Token Header", Vector2.zero, new Vector2(280, 76), PANEL);
            RectTransform rect = header.rectTransform; rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.anchoredPosition = new Vector2(310, -62);
            RaceUIBuilder.Label(header.transform, "Training Token Label", new Vector2(0, 18), new Vector2(310, 24), 18).text = "TRAINING TOKENS";
            Text total = RaceUIBuilder.Label(header.transform, "Training Tokens", new Vector2(0, -12), new Vector2(280, 40), 30);
            total.color = new Color(0.5f, 0.88f, 1f);
            Text popup = RaceUIBuilder.Label(header.transform, "Training Token Popup", new Vector2(120, -16), new Vector2(70, 32), 22);
            popup.color = new Color(0.5f, 0.88f, 1f, 0f);
            CurrencyFeedback feedback = root.GetComponent<CurrencyFeedback>();
            Bind(feedback, "_trainingTokens", total); Bind(feedback, "_trainingTokenPopup", popup);
        }
        private static void BuildHorseCards(GameplayWorld world)
        {
            Transform root = world.transform.Find("Stable Upgrades UI");
            Transform previous = root.Find("Training Horse Cards");
            if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
            root.Find("Upgrades").gameObject.SetActive(false);
            Image panel = RaceUIBuilder.Panel(root, "Training Horse Cards", Vector2.zero, new Vector2(520, 210), PANEL);
            RectTransform rect = panel.rectTransform; rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0);
            rect.anchoredPosition = new Vector2(-260, 220);
            RaceUIBuilder.Label(panel.transform, "Heading", new Vector2(0, 80), new Vector2(500, 32), 22).text = "TRAIN YOUR HORSE";
            string[] names = { "Speed", "Acceleration", "Boost Power" };
            Button[] buttons = new Button[3]; Text[] labels = new Text[3];
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = RaceUIBuilder.Button(panel.transform, "Train " + names[i], string.Empty, new Vector2((i - 1) * 166, 0));
                button.GetComponent<RectTransform>().sizeDelta = new Vector2(158, 110);
                Text label = button.GetComponentInChildren<Text>(); label.fontSize = 17; label.rectTransform.sizeDelta = new Vector2(150, 100);
                ColorBlock colors = button.colors; colors.disabledColor = new Color(0.4f, 0.42f, 0.45f); button.colors = colors;
                buttons[i] = button; labels[i] = label;
            }
            Text purchase = RaceUIBuilder.Label(panel.transform, "Training Upgrade Feedback", new Vector2(0, -79), new Vector2(500, 34), 20);
            StableUI stable = root.GetComponent<StableUI>();
            RaceSceneBaker.SetArray(stable, "_upgrades", buttons); RaceSceneBaker.SetArray(stable, "_upgradeLabels", labels);
            Bind(stable, "_feedback", purchase);
        }
        private static void SetInts(SerializedProperty property, int[] values)
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).intValue = values[i];
        }
        private static void MoveHeader(Transform target, Vector2 position, Vector2 size)
        {
            if (target == null) return;
            RectTransform rect = target.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.anchoredPosition = position; rect.sizeDelta = size;
        }
        private static void Bind(UnityEngine.Object target, string field, UnityEngine.Object value) => RaceSceneBaker.SetReference(target, field, value);
    }
}
