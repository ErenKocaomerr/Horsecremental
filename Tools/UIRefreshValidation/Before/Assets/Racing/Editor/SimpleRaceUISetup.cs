using TMPro;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing.Editor
{
    public static class SimpleRaceUISetup
    {
        private static readonly Color PANEL = new Color(0.055f, 0.085f, 0.075f, 0.97f);

        [MenuItem("Horsecremental/Simplify Race Navigation")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            GameplayWorld world = UnityEngine.Object.FindAnyObjectByType<GameplayWorld>();
            if (world == null || world.TrainingLadder == null)
                throw new InvalidOperationException("Open the world with the race ladder before simplifying its UI.");
            Apply(world);
            EditorSceneManager.MarkSceneDirty(world.gameObject.scene);
            EditorSceneManager.SaveScene(world.gameObject.scene);
            AssetDatabase.SaveAssets();
        }

        internal static void Apply(GameplayWorld world)
        {
            RenameRaces(world.TrainingLadder);
            ConfigureNavigation(world.transform);
            ConfigureSelection(world);
            ConfigureStable(world);
            ConfigureResources(world.transform);
            Transform race = world.transform.Find("Generated Race/Race UI");
            race.Find("Race entry/Title").GetComponent<TMP_Text>().text = "MAIN RACE";
            RaceSceneBaker.SetReference(race.GetComponent<RaceUI>(), "_entryRequirement",
                race.Find("Race entry/Description").GetComponent<TMP_Text>());
            race.Find("Race entry/Start Race").GetComponentInChildren<TMP_Text>().text = "START RACE";
            race.Find("Race result/Restart Race").GetComponentInChildren<TMP_Text>().text = "CHOOSE RACE";
            race.Find("Race result/Return to Stable").GetComponentInChildren<TMP_Text>().text = "BACK TO STABLE";
            Transform choose = race.Find("Race result/Restart Race");
            choose.gameObject.SetActive(true);
            Place(choose, new Vector2(0.5f, 0.5f), new Vector2(180, -220), new Vector2(320, 58));
            Place(race.Find("Race result/Return to Stable"), new Vector2(0.5f, 0.5f), new Vector2(-180, -220), new Vector2(320, 58));
            foreach (Transform button in new[] { choose, race.Find("Race result/Return to Stable") })
            {
                TMP_Text label = button.GetComponentInChildren<TMP_Text>(); label.fontSize = 23;
                label.rectTransform.sizeDelta = new Vector2(310, 54); Style(button.GetComponent<Button>());
            }
            race.Find("Controls").GetComponent<TMP_Text>().text = "SPACE  Boost     E  Jump";
        }

        private static void RenameRaces(TrainingLadderSettings ladder)
        {
            SerializedObject data = new SerializedObject(ladder);
            SerializedProperty tiers = data.FindProperty("_tiers");
            for (int i = 0; i < tiers.arraySize; i++)
            {
                SerializedProperty name = tiers.GetArrayElementAtIndex(i).FindPropertyRelative("_name");
                if (name.stringValue.StartsWith("Training", StringComparison.Ordinal)) name.stringValue = "Race " + (i + 1);
            }
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureNavigation(Transform root)
        {
            Transform nav = root.Find("Area Navigation UI");
            foreach (string name in new[] { "View Farm", "View Blacksmith", "View Race" }) nav.Find(name).gameObject.SetActive(false);
            string[] names = { "View Stable", "View Training", "View Upgrade Tree" };
            string[] labels = { "STABLE", "RACES", "UPGRADES" };
            for (int i = 0; i < names.Length; i++)
            {
                Transform button = nav.Find(names[i]);
                Place(button, new Vector2(0.5f, 0), new Vector2((i - 1) * 230, 50), new Vector2(214, 64));
                TMP_Text text = button.GetComponentInChildren<TMP_Text>();
                text.text = labels[i]; text.fontSize = 23; text.rectTransform.sizeDelta = new Vector2(204, 58);
                if (button.TryGetComponent(out UIDetailTarget detail)) detail.enabled = false;
                Style(button.GetComponent<Button>());
            }
            TMP_Text title = nav.Find("Current Area").GetComponent<TMP_Text>();
            title.fontSize = 28;
            Place(title.transform, new Vector2(0.5f, 1), new Vector2(0, -58), new Vector2(400, 45));
        }

        private static void ConfigureSelection(GameplayWorld world)
        {
            Transform panel = world.transform.Find("Training Selection UI/Training Selection");
            Place(panel, new Vector2(0.5f, 0.5f), new Vector2(0, 15), new Vector2(1220, 580));
            panel.GetComponent<Image>().color = PANEL;
            TMP_Text title = panel.Find("Title").GetComponent<TMP_Text>();
            title.text = "CHOOSE A RACE"; title.fontSize = 38;
            title.rectTransform.anchoredPosition = new Vector2(0, 235);
            TMP_Text hint = panel.Find("Hint").GetComponent<TMP_Text>();
            hint.text = "Win to unlock the next race. Upgrade your horse to enter the Main Race.";
            hint.fontSize = 25; hint.rectTransform.sizeDelta = new Vector2(1160, 40);
            hint.rectTransform.anchoredPosition = new Vector2(0, 187);
            TrainingTierCardUI[] cards = new TrainingTierCardUI[world.TrainingLadder.Count + 1];
            for (int i = 0; i < cards.Length; i++)
            {
                bool main = i == cards.Length - 1;
                Transform card = main ? EnsureMainCard(panel) : panel.Find("Training Card " + (i + 1));
                Place(card, new Vector2(0.5f, 0.5f), new Vector2((i - 1.5f) * 294, -14), new Vector2(278, 330));
                TextAt(card, "Name", 123, 36, 32);
                TextAt(card, "Status", 84, 28, 22);
                TextAt(card, "Best", 47, 32, 24);
                TextAt(card, "Rewards", -14, 78, 26);
                TextAt(card, "Requirement", -83, 52, 22);
                Button button = card.GetComponentInChildren<Button>(true);
                Place(button.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -132), new Vector2(246, 46));
                TMP_Text label = button.GetComponentInChildren<TMP_Text>(); label.fontSize = 25;
                label.rectTransform.sizeDelta = new Vector2(236, 42); Style(button);
                cards[i] = card.GetComponent<TrainingTierCardUI>();
            }
            RaceSceneBaker.SetArray(world.GetComponent<TrainingSelectionUI>(), "_cards", cards);
            Transform close = panel.Find("Close Training");
            close.GetComponentInChildren<TMP_Text>().text = "BACK TO STABLE";
            Place(close, new Vector2(0.5f, 0.5f), new Vector2(0, -239), new Vector2(300, 52));
            close.GetComponentInChildren<TMP_Text>().fontSize = 26; Style(close.GetComponent<Button>());
        }

        private static Transform EnsureMainCard(Transform parent)
        {
            Transform existing = parent.Find("Main Race Card");
            if (existing != null) return existing;
            Image card = RaceUIBuilder.Panel(parent, "Main Race Card", Vector2.zero, new Vector2(278, 330), PANEL);
            TrainingTierCardUI view = card.gameObject.AddComponent<TrainingTierCardUI>();
            RaceSceneBaker.SetReference(view, "_background", card);
            string[] names = { "Name", "Status", "Best", "Rewards", "Requirement" };
            string[] fields = { "_title", "_status", "_best", "_rewards", "_requirement" };
            ProgressionSettings rewards = AssetDatabase.LoadAssetAtPath<ProgressionSettings>(Milestone2SceneSetup.SETTINGS_PATH);
            for (int i = 0; i < names.Length; i++)
            {
                TMP_Text text = RaceUIBuilder.Label(card.transform, names[i], Vector2.zero, new Vector2(258, 40), 22);
                if (names[i] == "Rewards") text.text = rewards.RewardForPlace(4) + "-" + rewards.RewardForPlace(1) +
                    " Coins\n" + rewards.TokensForPlace(4) + "-" + rewards.TokensForPlace(1) + " Race Tokens";
                RaceSceneBaker.SetReference(view, fields[i], text);
            }
            RaceSceneBaker.SetReference(view, "_start", RaceUIBuilder.Button(card.transform, "Start Main Race", "START RACE", Vector2.zero));
            return card.transform;
        }

        private static void ConfigureStable(GameplayWorld world)
        {
            Transform horse = world.transform.Find("Stable Upgrades UI/Training Horse Cards");
            Place(horse, new Vector2(0.5f, 0), new Vector2(-310, 234), new Vector2(570, 238));
            horse.GetComponent<Image>().color = PANEL;
            TMP_Text heading = horse.Find("Heading").GetComponent<TMP_Text>(); heading.text = "HORSE UPGRADES";
            heading.fontSize = 28;
            heading.rectTransform.anchoredPosition = new Vector2(0, 91);
            TMP_Text power = horse.Find("Horse Power")?.GetComponent<TMP_Text>();
            if (power == null) power = RaceUIBuilder.Label(horse, "Horse Power", Vector2.zero, new Vector2(540, 32), 20);
            power.fontSize = 24;
            power.rectTransform.anchoredPosition = new Vector2(0, 57); power.color = new Color(0.89f, 0.80f, 0.53f);
            RaceSceneBaker.SetReference(world.transform.Find("Stable Upgrades UI").GetComponent<StableUI>(), "_power", power);
            string[] upgrades = { "Train Speed", "Train Acceleration", "Train Boost Power" };
            for (int i = 0; i < upgrades.Length; i++)
            {
                Transform button = horse.Find(upgrades[i]);
                Place(button, new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 180, -14), new Vector2(170, 94));
                TMP_Text label = button.GetComponentInChildren<TMP_Text>(); label.fontSize = 23; label.rectTransform.sizeDelta = new Vector2(162, 90);
                Style(button.GetComponent<Button>());
            }
            Place(horse.Find("Training Upgrade Feedback"), new Vector2(0.5f, 0.5f), new Vector2(0, -92), new Vector2(540, 32));
            Transform farm = world.transform.Find("Playground UI/Farm Upgrades");
            Place(farm, new Vector2(0.5f, 0), new Vector2(310, 234), new Vector2(570, 238));
            farm.GetComponent<Image>().color = PANEL;
            TMP_Text farmHeading = farm.Find("Heading").GetComponent<TMP_Text>(); farmHeading.text = "FARM";
            farmHeading.fontSize = 28; farmHeading.rectTransform.anchoredPosition = new Vector2(0, 91);
            string[] production = { "GrowSpeed", "ClickGrowthPower", "HarvestYield" };
            for (int i = 0; i < production.Length; i++)
            {
                Transform button = farm.Find(production[i]);
                Place(button, new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 180, 14), new Vector2(170, 90));
                TMP_Text label = button.GetComponentInChildren<TMP_Text>(); label.fontSize = 23; label.rectTransform.sizeDelta = new Vector2(162, 86);
                Style(button.GetComponent<Button>());
            }
            Transform feed = farm.Find("Feed Horse");
            Place(feed, new Vector2(0.5f, 0.5f), new Vector2(0, -80), new Vector2(520, 52));
            TMP_Text feedLabel = feed.GetComponentInChildren<TMP_Text>(); feedLabel.fontSize = 24; feedLabel.rectTransform.sizeDelta = new Vector2(510, 50);
            Style(feed.GetComponent<Button>());
        }

        private static void ConfigureResources(Transform root)
        {
            Transform currency = root.Find("Currency UI");
            Transform header = currency.Find("Training Token Header");
            Place(header, new Vector2(0, 1), new Vector2(280, -53), new Vector2(220, 76));
            header.GetComponent<Image>().color = PANEL; header.GetComponent<Image>().raycastTarget = false;
            TMP_Text label = header.Find("Training Token Label").GetComponent<TMP_Text>(); label.text = "UPGRADE TOKENS";
            label.rectTransform.sizeDelta = new Vector2(210, 25);
            Place(currency.Find("Coins"), new Vector2(0, 1), new Vector2(103, -53), new Vector2(100, 40));
            Place(currency.Find("Coins Icon"), new Vector2(0, 1), new Vector2(35, -53), new Vector2(32, 32));
            Place(currency.Find("Carrots"), new Vector2(0, 1), new Vector2(495, -53), new Vector2(100, 40));
            Place(currency.Find("Carrots Icon"), new Vector2(0, 1), new Vector2(426, -53), new Vector2(32, 32));
            Place(currency.Find("Carrots Popup"), new Vector2(0, 1), new Vector2(495, -88), new Vector2(100, 30));
        }

        private static void TextAt(Transform card, string name, float y, float height, int fontSize)
        {
            TMP_Text text = card.Find(name).GetComponent<TMP_Text>(); text.fontSize = fontSize;
            Place(text.transform, new Vector2(0.5f, 0.5f), new Vector2(0, y), new Vector2(258, height));
        }

        private static void Style(Button button)
        {
            button.GetComponent<Image>().color = new Color(0.16f, 0.30f, 0.22f);
            ColorBlock colors = button.colors;
            colors.disabledColor = new Color(0.55f, 0.58f, 0.56f, 1f);
            colors.highlightedColor = new Color(1.25f, 1.3f, 1.2f, 1f);
            button.colors = colors;
        }

        private static void Place(Transform target, Vector2 anchor, Vector2 position, Vector2 size)
        {
            RectTransform rect = (RectTransform)target;
            rect.anchorMin = rect.anchorMax = anchor; rect.anchoredPosition = position; rect.sizeDelta = size;
        }
    }
}
