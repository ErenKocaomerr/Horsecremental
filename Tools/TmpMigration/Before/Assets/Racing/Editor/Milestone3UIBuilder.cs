using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing.Editor
{
    internal static class Milestone3UIBuilder
    {
        private static readonly Color PANEL_COLOR = new Color(0.035f, 0.09f, 0.075f, 0.92f);
        internal static CanvasGroup BuildStable(Transform parent, ProgressionSettings settings)
        {
            GameObject canvas = Canvas(parent, "Stable Upgrades UI");
            StableUI ui = canvas.AddComponent<StableUI>();
            CanvasGroup group = canvas.AddComponent<CanvasGroup>();
            RaceSceneBaker.SetReference(ui, "_settings", settings);
            Text title = RaceUIBuilder.Label(canvas.transform, "World Title", Vector2.zero, new Vector2(600, 65), 34);
            Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -50f));
            title.text = "HORSECREMENTAL • STABLE";
            Image panel = RaceUIBuilder.Panel(canvas.transform, "Upgrades", Vector2.zero, new Vector2(450, 700), PANEL_COLOR);
            Anchor(panel.rectTransform, new Vector2(0f, 0.5f), new Vector2(250f, 0f));
            RaceUIBuilder.Label(panel.transform, "Upgrade Heading", new Vector2(0f, 305f), new Vector2(420, 50), 30).text = "HORSE UPGRADES";
            Text coins = RaceUIBuilder.Label(panel.transform, "Legacy Coins", Vector2.zero, Vector2.zero, 1);
            coins.gameObject.SetActive(false);
            RaceSceneBaker.SetReference(ui, "_coins", coins);
            RaceSceneBaker.SetReference(ui, "_stats", RaceUIBuilder.Label(panel.transform, "Current Stats", new Vector2(0f, 205f), new Vector2(410, 135), 26));
            Button[] buttons = new Button[3];
            Text[] labels = new Text[3];
            string[] names = { "Upgrade Speed", "Upgrade Acceleration", "Upgrade Boost Power" };
            for (int i = 0; i < 3; i++)
            {
                buttons[i] = RaceUIBuilder.Button(panel.transform, names[i], string.Empty, new Vector2(0f, 70f - 115f * i));
                buttons[i].GetComponent<RectTransform>().sizeDelta = new Vector2(395, 100);
                labels[i] = buttons[i].GetComponentInChildren<Text>();
                labels[i].rectTransform.sizeDelta = new Vector2(380, 95);
                labels[i].fontSize = 23;
            }
            RaceSceneBaker.SetArray(ui, "_upgrades", buttons);
            RaceSceneBaker.SetArray(ui, "_upgradeLabels", labels);
            Text feedback = RaceUIBuilder.Label(panel.transform, "Upgrade Feedback", new Vector2(0f, -280f), new Vector2(410, 65), 22);
            feedback.color = new Color(1f, 0.85f, 0.3f);
            feedback.text = "Training earns coins automatically.";
            RaceSceneBaker.SetReference(ui, "_feedback", feedback);
            Button race = RaceUIBuilder.Button(canvas.transform, "Stable Race Navigation", "RACE >  [D]", Vector2.zero);
            race.gameObject.SetActive(false); // Shared visible navigation lives in ManagementUI.
            RaceSceneBaker.SetReference(ui, "_startRace", race);
            return group;
        }

        internal static ManagementUI BuildManagement(Transform parent, GameplayWorld world, ProgressionSettings progression)
        {
            GameObject canvas = Canvas(parent, "Management UI");
            ManagementUI ui = canvas.AddComponent<ManagementUI>();
            RaceSceneBaker.SetReference(ui, "_world", world);
            RaceSceneBaker.SetReference(ui, "_progression", progression);
            GameObject panels = new GameObject("Training Farm Panels", typeof(RectTransform), typeof(CanvasGroup));
            panels.transform.SetParent(canvas.transform, false);
            RectTransform panelsRect = panels.GetComponent<RectTransform>();
            panelsRect.anchorMin = Vector2.zero; panelsRect.anchorMax = Vector2.one; panelsRect.offsetMin = panelsRect.offsetMax = Vector2.zero;
            RaceSceneBaker.SetReference(ui, "_panels", panels.GetComponent<CanvasGroup>());
            Image training = RaceUIBuilder.Panel(panels.transform, "Training Panel", Vector2.zero, new Vector2(460, 210), PANEL_COLOR);
            Anchor(training.rectTransform, new Vector2(1f, 0.5f), new Vector2(-255f, 220f));
            RaceSceneBaker.SetReference(ui, "_training", RaceUIBuilder.Label(training.transform, "Training Status", new Vector2(0, 35), new Vector2(430, 100), 24));
            Image trainingBar = Bar(training.transform, "Training Progress", new Vector2(0, -65), new Color(0.95f, 0.74f, 0.22f));
            RaceSceneBaker.SetReference(ui, "_trainingFill", trainingBar);
            Image horse = RaceUIBuilder.Panel(trainingBar.transform.parent, "Training Horse Icon", new Vector2(-205, 14), new Vector2(44, 36), Color.white);
            UnityEngine.Object[] frames = AssetDatabase.LoadAllAssetsAtPath("Assets/Racing/Generated/Horse0Frames.png");
            foreach (UnityEngine.Object asset in frames) if (asset is Sprite sprite && sprite.name == "Horse0Idle") horse.sprite = sprite;
            horse.preserveAspect = true; horse.raycastTarget = false;
            RaceSceneBaker.SetReference(ui, "_horseIcon", horse.rectTransform);
            Image farm = RaceUIBuilder.Panel(panels.transform, "Farm Panel", Vector2.zero, new Vector2(460, 350), PANEL_COLOR);
            Anchor(farm.rectTransform, new Vector2(1f, 0.5f), new Vector2(-255f, -90f));
            RaceSceneBaker.SetReference(ui, "_farm", RaceUIBuilder.Label(farm.transform, "Farm Status", new Vector2(0, 55), new Vector2(430, 185), 23));
            RaceSceneBaker.SetReference(ui, "_farmFill", Bar(farm.transform, "Farm Progress", new Vector2(0, -58), new Color(0.4f, 0.78f, 0.3f)));
            Button collect = RaceUIBuilder.Button(farm.transform, "Collect Carrots", "COLLECT", new Vector2(-110, -120));
            Button feed = RaceUIBuilder.Button(farm.transform, "Feed Horse", "FEED HORSE", new Vector2(110, -120));
            ResizeButton(collect, 205); ResizeButton(feed, 205);
            RaceSceneBaker.SetReference(ui, "_collect", collect);
            RaceSceneBaker.SetReference(ui, "_feed", feed);
            Text difficulty = RaceUIBuilder.Label(panels.transform, "Main Race Power", Vector2.zero, new Vector2(460, 100), 23);
            Anchor(difficulty.rectTransform, new Vector2(1, 0.5f), new Vector2(-255, -320));
            difficulty.color = new Color(1f, 0.82f, 0.4f);
            RaceSceneBaker.SetReference(ui, "_difficulty", difficulty);
            Text feedback = RaceUIBuilder.Label(panels.transform, "Farm Feedback", Vector2.zero, new Vector2(1050, 60), 26);
            Anchor(feedback.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 115));
            feedback.color = new Color(0.8f, 1f, 0.55f);
            RaceSceneBaker.SetReference(ui, "_feedback", feedback);
            Button left = RaceUIBuilder.Button(canvas.transform, "View Stable", "< STABLE  [A]", Vector2.zero);
            Anchor(left.GetComponent<RectTransform>(), Vector2.zero, new Vector2(180, 55));
            Button right = RaceUIBuilder.Button(canvas.transform, "View Race", "RACE >  [D]", Vector2.zero);
            Anchor(right.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(-180, 55));
            RaceSceneBaker.SetReference(ui, "_left", left);
            RaceSceneBaker.SetReference(ui, "_right", right);
            return ui;
        }

        internal static void BuildCurrency(Transform parent, ProgressionSettings progression)
        {
            GameObject canvas = Canvas(parent, "Currency UI");
            canvas.GetComponent<UnityEngine.Canvas>().sortingOrder = 20;
            CurrencyFeedback feedback = canvas.AddComponent<CurrencyFeedback>();
            Text total = RaceUIBuilder.Label(canvas.transform, "Coins", Vector2.zero, new Vector2(350, 55), 30);
            Anchor(total.rectTransform, Vector2.one, new Vector2(-220, -50));
            total.color = new Color(1f, 0.85f, 0.25f);
            Text popup = RaceUIBuilder.Label(canvas.transform, "Coin Popup", Vector2.zero, new Vector2(350, 45), 26);
            Anchor(popup.rectTransform, Vector2.one, new Vector2(-220, -95));
            RaceSceneBaker.SetReference(feedback, "_settings", progression);
            RaceSceneBaker.SetReference(feedback, "_total", total);
            RaceSceneBaker.SetReference(feedback, "_popup", popup);
        }

        private static GameObject Canvas(Transform parent, string name)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(parent, false);
            root.GetComponent<UnityEngine.Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            return root;
        }
        private static Image Bar(Transform parent, string name, Vector2 position, Color color)
        {
            Image bar = RaceUIBuilder.Panel(parent, name, position, new Vector2(410, 12), new Color(0.08f, 0.15f, 0.1f));
            Image fill = RaceUIBuilder.Panel(bar.transform, "Fill", Vector2.zero, new Vector2(0, 12), color);
            fill.rectTransform.anchorMin = fill.rectTransform.anchorMax = new Vector2(0, 0.5f);
            fill.rectTransform.pivot = new Vector2(0, 0.5f);
            return fill;
        }
        private static void ResizeButton(Button button, float width)
        {
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(width, 60);
            button.GetComponentInChildren<Text>().rectTransform.sizeDelta = new Vector2(width - 10, 55);
            button.GetComponentInChildren<Text>().fontSize = 23;
        }
        private static void Anchor(RectTransform rect, Vector2 anchor, Vector2 position)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position;
        }
    }
}
