using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing.Editor
{
    internal static class Milestone45UIBuilder
    {
        private static readonly Color PANEL = new Color(0.065f, 0.075f, 0.08f, 0.94f);
        internal static void Build(GameplayWorld world, StableAudioHooks audio)
        {
            Transform root = world.transform;
            foreach (string old in new[] { "Management UI", "Farm UI", "Blacksmith UI" }) root.Find(old).gameObject.SetActive(false);
            foreach (CraftFeedback feedback in root.GetComponentsInChildren<CraftFeedback>(true)) feedback.enabled = false;
            foreach (Transform item in root.Find("Currency UI"))
                if (item.name.Contains("Metal") || item.name.Contains("Craft")) item.gameObject.SetActive(false);
            Transform tree = root.Find("Upgrade Tree UI/Tree Panel");
            foreach (UpgradeNode node in new[] { UpgradeNode.Blacksmith, UpgradeNode.FarmCapacity, UpgradeNode.ForgeSpeed, UpgradeNode.MetalEfficiency })
            {
                Transform button = tree.Find("Unlock " + node); if (button != null) button.gameObject.SetActive(false);
                foreach (Transform line in tree) if (line.name.Contains(node.ToString()) && line.name != "Unlock " + node) line.gameObject.SetActive(false);
            }
            Transform nav = root.Find("Area Navigation UI");
            int index = 0;
            foreach (Button button in nav.GetComponentsInChildren<Button>(true))
            {
                if (button.name.Contains("Blacksmith")) { button.gameObject.SetActive(false); continue; }
                button.GetComponent<RectTransform>().anchoredPosition = new Vector2((index++ - 1.5f) * 210f, 48);
            }
            root.Find("Generated Race/Race UI/Race entry/Description").GetComponent<Text>().text = "Fixed rivals: 23 / 24 / 25 Speed\nTrain, upgrade your horse and feed Carrots.";
            Transform stable = root.Find("Stable Upgrades UI/Upgrades");
            Place(stable, new Vector2(0.5f, 0), new Vector2(0, 220), new Vector2(350, 205));
            Place(stable.Find("Upgrade Heading"), new Vector2(0.5f, 0.5f), new Vector2(0, 77), new Vector2(300, 35));
            stable.Find("Upgrade Heading").GetComponent<Text>().fontSize = 23;
            Place(stable.Find("Current Stats"), new Vector2(0.5f, 0.5f), new Vector2(0, 27), new Vector2(310, 72));
            stable.Find("Current Stats").GetComponent<Text>().fontSize = 16;
            string[] horseButtons = { "Upgrade Speed", "Upgrade Acceleration", "Upgrade Boost Power" };
            for (int i = 0; i < horseButtons.Length; i++) Place(stable.Find(horseButtons[i]), new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 90, -53), new Vector2(58, 58));
            stable.Find("Upgrade Feedback").gameObject.SetActive(false);
            GameObject ui = new GameObject("Playground UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            ui.transform.SetParent(root, false); ui.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay; ui.GetComponent<Canvas>().sortingOrder = 10;
            CanvasScaler scaler = ui.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            AreaPanel area = ui.AddComponent<AreaPanel>(); Bind(area, "_world", world);
            Bind(world, "_managementUI", ui);
            Transform training = Panel(ui.transform, "Training Upgrades", new Vector2(-440, 220), "TRAINING • BUY WITH COINS");
            Upgrade(training, world, audio, ProductionUpgrade.TrainingHorseCount, -140);
            Upgrade(training, world, audio, ProductionUpgrade.TrainingSpeed, 0);
            Upgrade(training, world, audio, ProductionUpgrade.TrainingReward, 140);
            Transform farm = Panel(ui.transform, "Farm Upgrades", new Vector2(440, 220), "FARM • CLICK TO GROW / HARVEST");
            Upgrade(farm, world, audio, ProductionUpgrade.GrowSpeed, -140);
            Upgrade(farm, world, audio, ProductionUpgrade.ClickGrowthPower, 0);
            Upgrade(farm, world, audio, ProductionUpgrade.HarvestYield, 140);
            Button feed = RaceUIBuilder.Button(farm, "Feed Horse", string.Empty, new Vector2(0, -65));
            feed.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 46);
            feed.GetComponentInChildren<Text>().rectTransform.sizeDelta = new Vector2(290, 44);
            PlaygroundFarmUI farmUI = farm.gameObject.AddComponent<PlaygroundFarmUI>();
            Bind(farmUI, "_world", world); Bind(farmUI, "_feed", feed); Bind(farmUI, "_label", feed.GetComponentInChildren<Text>());
            Details(feed.gameObject, world, DetailKind.Feed, 0);
            Text hint = RaceUIBuilder.Label(training, "Training Hint", new Vector2(0, -70), new Vector2(410, 40), 18);
            hint.text = "Every horse pays at the finish line"; hint.raycastTarget = false;
            RaceSceneBaker.SetArray(root.GetComponentInChildren<UIDetailKeyboardTraversal>(true), "_targets", root.GetComponentsInChildren<UIDetailTarget>(true));
        }
        private static Transform Panel(Transform parent, string name, Vector2 position, string title)
        {
            Image panel = RaceUIBuilder.Panel(parent, name, Vector2.zero, new Vector2(450, 205), PANEL);
            Place(panel.transform, new Vector2(0.5f, 0), position, new Vector2(450, 205));
            Text heading = RaceUIBuilder.Label(panel.transform, "Heading", new Vector2(0, 77), new Vector2(430, 35), 20);
            heading.text = title; heading.raycastTarget = false; return panel.transform;
        }
        private static void Upgrade(Transform parent, GameplayWorld world, StableAudioHooks audio, ProductionUpgrade upgrade, float x)
        {
            Button button = RaceUIBuilder.Button(parent, upgrade.ToString(), string.Empty, new Vector2(x, 2));
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(130, 78);
            Text label = button.GetComponentInChildren<Text>(); label.fontSize = 17;
            label.rectTransform.sizeDelta = new Vector2(120, 74);
            PlaygroundUpgradeUI presenter = button.gameObject.AddComponent<PlaygroundUpgradeUI>();
            Bind(presenter, "_world", world); Bind(presenter, "_audio", audio); Bind(presenter, "_label", label);
            SerializedObject data = new SerializedObject(presenter); data.FindProperty("_upgrade").enumValueIndex = (int)upgrade; data.ApplyModifiedPropertiesWithoutUndo();
            Details(button.gameObject, world, DetailKind.Production, (int)upgrade);
        }
        private static void Details(GameObject button, GameplayWorld world, DetailKind kind, int id)
        {
            UIDetailTarget target = button.AddComponent<UIDetailTarget>();
            Bind(target, "_panel", world.GetComponentInChildren<UIDetailPanel>(true));
            Bind(target, "_icon", AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Racing/Generated/UI/Icon17.png"));
            SerializedObject data = new SerializedObject(target); data.FindProperty("_kind").intValue = (int)kind;
            data.FindProperty("_id").intValue = id; data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Place(Transform target, Vector2 anchor, Vector2 position, Vector2 size)
        { RectTransform rect = target.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = anchor; rect.anchoredPosition = position; rect.sizeDelta = size; }
        private static void Bind(UnityEngine.Object target, string field, UnityEngine.Object value) => Milestone45SceneSetup.Bind(target, field, value);
    }
}
