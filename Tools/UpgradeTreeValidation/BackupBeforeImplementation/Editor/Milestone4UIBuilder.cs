using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing.Editor
{
    internal static class Milestone4UIBuilder
    {
        private static readonly Color PANEL = new Color(0.035f, 0.09f, 0.075f, 0.94f);
        internal static void Build(Transform parent, GameplayWorld world, ProgressionSettings settings)
        {
            ConfigureHorse(parent, world);
            BuildTraining(parent, world, settings);
            BuildFarm(parent, world, settings);
            BuildBlacksmith(parent, world, settings);
            BuildNavigation(parent, world);
            BuildResources(parent, world, settings);
        }
        private static void ConfigureHorse(Transform parent, GameplayWorld world)
        {
            Transform root = parent.Find("Stable Upgrades UI");
            Area(root.gameObject, world, ManagementArea.Stable);
            root.Find("World Title").gameObject.SetActive(false);
            RectTransform panel = (RectTransform)root.Find("Upgrades");
            panel.sizeDelta = new Vector2(450, 820);
            Place(panel.Find("Upgrade Heading"), new Vector2(0, 370), new Vector2(420, 50));
            Place(panel.Find("Current Stats"), new Vector2(0, 195), new Vector2(410, 290));
            panel.Find("Current Stats").GetComponent<Text>().fontSize = 20;
            string[] names = { "Upgrade Speed", "Upgrade Acceleration", "Upgrade Boost Power" };
            for (int i = 0; i < names.Length; i++) Place(panel.Find(names[i]), new Vector2(0, -30 - i * 110), new Vector2(395, 100));
            Place(panel.Find("Upgrade Feedback"), new Vector2(0, -355), new Vector2(410, 65));
        }
        private static void BuildTraining(Transform parent, GameplayWorld world, ProgressionSettings settings)
        {
            GameObject canvas = Canvas(parent, "Management UI");
            Area(canvas, world, ManagementArea.Stable);
            ManagementUI ui = canvas.AddComponent<ManagementUI>();
            Bind(ui, world, settings, "_progression");
            Image panel = RaceUIBuilder.Panel(canvas.transform, "Training Panel", Vector2.zero, new Vector2(470, 520), PANEL);
            Anchor(panel.rectTransform, new Vector2(1, 0.5f), new Vector2(-265, 90));
            Text status = Label(panel.transform, "Training Status", new Vector2(0, 170), new Vector2(430, 140), 25);
            RaceSceneBaker.SetReference(ui, "_training", status);
            Image fill = Bar(panel.transform, "Training Progress", new Vector2(0, 77), 410, new Color(0.95f, 0.74f, 0.22f));
            RaceSceneBaker.SetReference(ui, "_trainingFill", fill);
            Image horse = RaceUIBuilder.Panel(fill.transform.parent, "Training Horse Icon", new Vector2(-205, 14), new Vector2(44, 36), Color.white);
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath("Assets/Racing/Generated/Horse0Frames.png"))
                if (asset is Sprite sprite && sprite.name == "Horse0Idle") horse.sprite = sprite;
            horse.preserveAspect = true; horse.raycastTarget = false;
            RaceSceneBaker.SetReference(ui, "_horseIcon", horse.rectTransform);
            Text feedback = Label(panel.transform, "Training Feedback", new Vector2(0, -215), new Vector2(430, 60), 20);
            Upgrade(panel.transform, world, settings, ProductionUpgrade.TrainingSpeed, ManagementArea.Stable, new Vector2(0, 5), feedback);
            Upgrade(panel.transform, world, settings, ProductionUpgrade.TrainingReward, ManagementArea.Stable, new Vector2(0, -115), feedback);
            Text difficulty = Label(canvas.transform, "Main Race Power", Vector2.zero, new Vector2(460, 110), 22);
            Anchor(difficulty.rectTransform, new Vector2(1, 0.5f), new Vector2(-265, -280));
            RaceSceneBaker.SetReference(ui, "_difficulty", difficulty);
            RaceSceneBaker.SetReference(world, "_managementUI", canvas);
        }
        private static void BuildFarm(Transform parent, GameplayWorld world, ProgressionSettings settings)
        {
            GameObject canvas = Canvas(parent, "Farm UI"); Area(canvas, world, ManagementArea.Farm);
            FarmUI ui = canvas.AddComponent<FarmUI>(); Bind(ui, world, settings);
            Image panel = RaceUIBuilder.Panel(canvas.transform, "Farm Panel", Vector2.zero, new Vector2(470, 570), PANEL);
            Anchor(panel.rectTransform, new Vector2(0, 0.5f), new Vector2(265, 0));
            RaceSceneBaker.SetReference(ui, "_status", Label(panel.transform, "Farm Status", new Vector2(0, 70), new Vector2(430, 325), 25));
            RaceSceneBaker.SetReference(ui, "_fill", Bar(panel.transform, "Farm Progress", new Vector2(0, -110), 410, new Color(0.4f, 0.78f, 0.3f)));
            Button collect = Button(panel.transform, "Collect Carrots", "COLLECT", new Vector2(-110, -180), new Vector2(205, 65));
            Button feed = Button(panel.transform, "Feed Horse", "FEED HORSE", new Vector2(110, -180), new Vector2(205, 65));
            RaceSceneBaker.SetReference(ui, "_collect", collect); RaceSceneBaker.SetReference(ui, "_feed", feed);
            Text feedback = Label(panel.transform, "Farm Feedback", new Vector2(0, -240), new Vector2(430, 55), 20);
            RaceSceneBaker.SetReference(ui, "_feedback", feedback);
            Image upgrades = RaceUIBuilder.Panel(canvas.transform, "Farm Upgrades", Vector2.zero, new Vector2(470, 360), PANEL);
            Anchor(upgrades.rectTransform, new Vector2(1, 0.5f), new Vector2(-265, 0));
            Label(upgrades.transform, "Heading", new Vector2(0, 130), new Vector2(430, 50), 28).text = "FARM PRODUCTION";
            Upgrade(upgrades.transform, world, settings, ProductionUpgrade.GrowSpeed, ManagementArea.Farm, new Vector2(0, 35), feedback);
            Upgrade(upgrades.transform, world, settings, ProductionUpgrade.FarmCapacity, ManagementArea.Farm, new Vector2(0, -85), feedback);
        }
        private static void BuildBlacksmith(Transform parent, GameplayWorld world, ProgressionSettings settings)
        {
            GameObject canvas = Canvas(parent, "Blacksmith UI"); Area(canvas, world, ManagementArea.Blacksmith);
            BlacksmithUI ui = canvas.AddComponent<BlacksmithUI>(); Bind(ui, world, settings);
            Text status = Label(canvas.transform, "Craft Status", new Vector2(0, 120), new Vector2(1300, 65), 26);
            RaceSceneBaker.SetReference(ui, "_status", status);
            RaceSceneBaker.SetReference(ui, "_fill", Bar(canvas.transform, "Craft Progress", new Vector2(0, 73), 780, new Color(1f, 0.55f, 0.15f)));
            Text[] cards = new Text[3]; Button[] crafts = new Button[3];
            string[] names = { "Horseshoes", "Saddle", "Bridle" };
            for (int i = 0; i < 3; i++)
            {
                Image panel = RaceUIBuilder.Panel(canvas.transform, names[i] + " Card", new Vector2((i - 1) * 440, -125), new Vector2(420, 350), PANEL);
                cards[i] = Label(panel.transform, "Equipment Details", new Vector2(0, 30), new Vector2(395, 265), 21);
                crafts[i] = Button(panel.transform, "Craft " + names[i], "CRAFT", new Vector2(0, -135), new Vector2(375, 55));
            }
            RaceSceneBaker.SetArray(ui, "_cards", cards); RaceSceneBaker.SetArray(ui, "_crafts", crafts);
            Text feedback = Label(canvas.transform, "Craft Feedback", new Vector2(0, -328), new Vector2(1250, 36), 21);
            RaceSceneBaker.SetReference(ui, "_feedback", feedback);
            Upgrade(canvas.transform, world, settings, ProductionUpgrade.ForgeSpeed, ManagementArea.Blacksmith, new Vector2(-260, -398), feedback, 95);
            Upgrade(canvas.transform, world, settings, ProductionUpgrade.MetalEfficiency, ManagementArea.Blacksmith, new Vector2(260, -398), feedback, 95);
        }
        private static void BuildNavigation(Transform parent, GameplayWorld world)
        {
            GameObject canvas = Canvas(parent, "Area Navigation UI"); canvas.GetComponent<UnityEngine.Canvas>().sortingOrder = 15;
            AreaNavigationUI ui = canvas.AddComponent<AreaNavigationUI>(); RaceSceneBaker.SetReference(ui, "_world", world);
            Text title = Label(canvas.transform, "Current Area", Vector2.zero, new Vector2(800, 55), 32);
            Anchor(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -103));
            RaceSceneBaker.SetReference(ui, "_title", title);
            Button[] buttons = new Button[4];
            string[] names = { "View Stable", "View Farm", "View Blacksmith", "View Race" };
            string[] labels = { "STABLE [A]", "FARM", "BLACKSMITH", "RACE [D]" };
            for (int i = 0; i < 4; i++)
            {
                buttons[i] = Button(canvas.transform, names[i], labels[i], Vector2.zero, new Vector2(235, 65));
                Anchor(buttons[i].GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2((i - 1.5f) * 255, 48));
            }
            RaceSceneBaker.SetArray(ui, "_buttons", buttons);
        }
        private static void BuildResources(Transform parent, GameplayWorld world, ProgressionSettings settings)
        {
            GameObject canvas = Canvas(parent, "Currency UI"); canvas.GetComponent<UnityEngine.Canvas>().sortingOrder = 20;
            CurrencyFeedback ui = canvas.AddComponent<CurrencyFeedback>();
            RaceSceneBaker.SetReference(ui, "_settings", settings); RaceSceneBaker.SetReference(ui, "_worldSettings", world.Settings);
            Header(ui, canvas.transform, "Coins", "_total", "_popup", -540, 370, new Color(1, 0.85f, 0.25f));
            Header(ui, canvas.transform, "Metal", "_metal", "_metalPopup", -110, 350, new Color(0.65f, 0.85f, 1));
            Header(ui, canvas.transform, "Carrots", "_carrots", "_carrotPopup", 440, 650, new Color(0.7f, 1, 0.45f));
            CraftFeedback feedback = canvas.AddComponent<CraftFeedback>(); RaceSceneBaker.SetReference(feedback, "_settings", settings);
            Image toast = RaceUIBuilder.Panel(canvas.transform, "Craft Complete Toast", Vector2.zero, new Vector2(1320, 90), new Color(0.04f, 0.07f, 0.04f, 0.97f));
            Anchor(toast.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -172)); toast.raycastTarget = false;
            CanvasGroup group = toast.gameObject.AddComponent<CanvasGroup>(); group.alpha = 0; group.blocksRaycasts = group.interactable = false;
            RaceSceneBaker.SetReference(feedback, "_group", group);
            Text banner = Label(toast.transform, "Craft Complete Banner", Vector2.zero, new Vector2(1300, 85), 27);
            banner.color = new Color(1f, 0.8f, 0.3f);
            RaceSceneBaker.SetReference(feedback, "_banner", banner);
        }
        private static void Header(CurrencyFeedback ui, Transform parent, string name, string totalField, string popupField, float x, float width, Color color)
        {
            Text total = Label(parent, name, Vector2.zero, new Vector2(width, 45), 27);
            Anchor(total.rectTransform, new Vector2(0.5f, 1), new Vector2(x, -32)); total.color = color;
            Text popup = Label(parent, name + " Popup", Vector2.zero, new Vector2(width, 30), 20);
            Anchor(popup.rectTransform, new Vector2(0.5f, 1), new Vector2(x, -62)); popup.color = new Color(color.r, color.g, color.b, 0f);
            RaceSceneBaker.SetReference(ui, totalField, total); RaceSceneBaker.SetReference(ui, popupField, popup);
        }
        private static void Upgrade(Transform parent, GameplayWorld world, ProgressionSettings settings, ProductionUpgrade upgrade, ManagementArea area, Vector2 position, Text feedback, float height = 105)
        {
            Button button = Button(parent, "Upgrade " + upgrade, "", position, new Vector2(430, height));
            ProductionUpgradeUI ui = button.gameObject.AddComponent<ProductionUpgradeUI>(); Bind(ui, world, settings);
            SetEnum(ui, "_upgrade", (int)upgrade); SetEnum(ui, "_area", (int)area);
            RaceSceneBaker.SetReference(ui, "_label", button.GetComponentInChildren<Text>()); RaceSceneBaker.SetReference(ui, "_feedback", feedback);
        }
        private static void Bind(Object ui, GameplayWorld world, ProgressionSettings settings, string settingsField = "_settings")
        { RaceSceneBaker.SetReference(ui, "_world", world); RaceSceneBaker.SetReference(ui, settingsField, settings); }
        private static void Area(GameObject root, GameplayWorld world, ManagementArea area)
        {
            if (!root.TryGetComponent(out CanvasGroup group)) group = root.AddComponent<CanvasGroup>();
            if (!root.TryGetComponent(out AreaPanel panel)) panel = root.AddComponent<AreaPanel>();
            RaceSceneBaker.SetReference(panel, "_world", world); SetEnum(panel, "_area", (int)area);
            group.alpha = area == ManagementArea.Stable ? 1f : 0f;
            group.interactable = group.blocksRaycasts = area == ManagementArea.Stable;
        }
        private static GameObject Canvas(Transform parent, string name)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(parent, false); root.GetComponent<UnityEngine.Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            return root;
        }
        private static Text Label(Transform parent, string name, Vector2 position, Vector2 size, int font)
            => RaceUIBuilder.Label(parent, name, position, size, font);
        private static Button Button(Transform parent, string name, string label, Vector2 position, Vector2 size)
        {
            Button button = RaceUIBuilder.Button(parent, name, label, position);
            button.GetComponent<RectTransform>().sizeDelta = size;
            Text text = button.GetComponentInChildren<Text>(); text.rectTransform.sizeDelta = size - new Vector2(14, 10); text.fontSize = 22;
            button.gameObject.AddComponent<ButtonPulse>(); return button;
        }
        private static Image Bar(Transform parent, string name, Vector2 position, float width, Color color)
        {
            Image bar = RaceUIBuilder.Panel(parent, name, position, new Vector2(width, 12), new Color(0.08f, 0.15f, 0.1f));
            Image fill = RaceUIBuilder.Panel(bar.transform, "Fill", Vector2.zero, new Vector2(0, 12), color);
            fill.rectTransform.anchorMin = fill.rectTransform.anchorMax = new Vector2(0, 0.5f);
            fill.rectTransform.pivot = new Vector2(0, 0.5f); return fill;
        }
        private static void Anchor(RectTransform rect, Vector2 anchor, Vector2 position)
        { rect.anchorMin = rect.anchorMax = anchor; rect.anchoredPosition = position; }
        private static void Place(Transform transform, Vector2 position, Vector2 size)
        { RectTransform rect = (RectTransform)transform; rect.anchoredPosition = position; rect.sizeDelta = size; }
        private static void SetEnum(Object target, string field, int value)
        { SerializedObject data = new SerializedObject(target); data.FindProperty(field).enumValueIndex = value; data.ApplyModifiedPropertiesWithoutUndo(); }
    }
}
