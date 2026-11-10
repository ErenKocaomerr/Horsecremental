using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static Horsecremental.Racing.Editor.RuralUIWidgets;

namespace Horsecremental.Racing.Editor
{
    public static class RuralUIPresentationSetup
    {
        private static readonly Vector2 CENTRE = new Vector2(.5f, .5f);
        private static readonly Vector2 BOTTOM = new Vector2(.5f, 0);
        private static readonly Vector2 TOP_LEFT = new Vector2(0, 1);

        [MenuItem("Horsecremental/Apply Rural UI Design")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Apply UI design outside Play mode.");
            GameplayWorld world = UnityEngine.Object.FindAnyObjectByType<GameplayWorld>();
            if (world == null || world.TrainingLadder == null) throw new InvalidOperationException("Open the existing gameplay world with its race ladder.");
            Apply(world);
            EditorSceneManager.MarkSceneDirty(world.gameObject.scene);
            EditorSceneManager.SaveScene(world.gameObject.scene); AssetDatabase.SaveAssets();
        }

        internal static void Apply(GameplayWorld world)
        {
            RuralUIAssets.Ensure();
            UIPresentationSettings icons = UIIconAssets.Ensure();
            UIDetailPanel details = world.GetComponentInChildren<UIDetailPanel>(true);
            ConfigureDetails(details);
            ConfigureStable(world, icons, details);
            ConfigureChrome(world, icons);
            RuralUIRacePresentation.Apply(world, icons, details);
            ConfigureTree(world);
            foreach (CanvasScaler scaler in world.GetComponentsInChildren<CanvasScaler>(true))
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            }
            RaceSceneBaker.SetArray(details.GetComponent<UIDetailKeyboardTraversal>(), "_targets", world.GetComponentsInChildren<UIDetailTarget>(true));
        }

        private static void ConfigureStable(GameplayWorld world, UIPresentationSettings icons, UIDetailPanel details)
        {
            Transform horse = world.transform.Find("Stable Upgrades UI/Training Horse Cards");
            Place(horse, BOTTOM, new Vector2(-282, 209), new Vector2(540, 206)); Skin(horse, "Parchment");
            Text(horse, "Heading", "YOUR HORSE", new Vector2(-136, 70), new Vector2(240, 38), 30);
            TMP_Text power = Text(horse, "Horse Power", "", new Vector2(133, 70), new Vector2(240, 38), 25, true);
            StableUI stable = world.transform.Find("Stable Upgrades UI").GetComponent<StableUI>();
            RaceSceneBaker.SetReference(stable, "_theme", RuralUIAssets.Theme);
            RaceSceneBaker.SetReference(stable, "_power", power);
            string[] names = { "Train Speed", "Train Acceleration", "Train Boost Power" };
            for (int i = 0; i < names.Length; i++)
            {
                Transform t = horse.Find(names[i]); Place(t, CENTRE, new Vector2((i - 1) * 166, -8), new Vector2(154, 104));
                Button(t.GetComponent<Button>(), "Inset");
                TMP_Text label = t.GetComponentInChildren<TMP_Text>(); label.color = RuralUIAssets.Theme.Ink;
                Place(label.transform, CENTRE, new Vector2(0, -20), new Vector2(146, 52)); label.fontSize = 23;
                Image(t, "Rural Icon", icons.Icon(i), new Vector2(0, 24), new Vector2(32, 32));
                Inspect(t.gameObject, details, DetailKind.Horse, i, icons.Icon(i));
            }
            Place(horse.Find("Training Upgrade Feedback"), CENTRE, new Vector2(0, -80), new Vector2(490, 26));
            horse.Find("Training Upgrade Feedback").GetComponent<TMP_Text>().color = RuralUIAssets.Theme.Positive;

            Transform farm = world.transform.Find("Playground UI/Farm Upgrades");
            Place(farm, BOTTOM, new Vector2(282, 209), new Vector2(540, 206)); Skin(farm, "Parchment");
            Text(farm, "Heading", "CARROT PATCH", new Vector2(-112, 70), new Vector2(310, 38), 30);
            Text(farm, "Farm Hint", "GROW / HARVEST", new Vector2(155, 70), new Vector2(208, 32), 21, true);
            string[] production = { "GrowSpeed", "ClickGrowthPower", "HarvestYield" };
            int[] iconIds = { 7, 1, 17 };
            for (int i = 0; i < production.Length; i++)
            {
                Transform t = farm.Find(production[i]); Place(t, CENTRE, new Vector2((i - 1) * 166, 0), new Vector2(154, 84));
                Button(t.GetComponent<Button>(), "Inset");
                TMP_Text label = t.GetComponentInChildren<TMP_Text>(); label.color = RuralUIAssets.Theme.Ink;
                Place(label.transform, CENTRE, new Vector2(12, -8), new Vector2(128, 58)); label.fontSize = 22;
                Image(t, "Rural Icon", icons.Icon(iconIds[i]), new Vector2(-56, 20), new Vector2(24, 24));
                Inspect(t.gameObject, details, DetailKind.Production, (int)(ProductionUpgrade)Enum.Parse(typeof(ProductionUpgrade), production[i]), icons.Icon(iconIds[i]));
            }
            Transform feed = farm.Find("Feed Horse"); Place(feed, CENTRE, new Vector2(0, -68), new Vector2(490, 42));
            Button(feed.GetComponent<Button>());
            TMP_Text feedLabel = feed.GetComponentInChildren<TMP_Text>(); feedLabel.fontSize = 25;
            Place(feedLabel.transform, CENTRE, Vector2.zero, new Vector2(472, 38));
            Inspect(feed.gameObject, details, DetailKind.Feed, 0, icons.Icon(17));
        }

        private static void ConfigureChrome(GameplayWorld world, UIPresentationSettings icons)
        {
            Transform nav = world.transform.Find("Area Navigation UI");
            string[] names = { "View Stable", "View Training", "View Upgrade Tree" };
            int[] ids = { 11, 5, 7 };
            for (int i = 0; i < names.Length; i++)
            {
                Transform t = nav.Find(names[i]); Place(t, BOTTOM, new Vector2((i - 1) * 212, 51), new Vector2(202, 62));
                Button(t.GetComponent<Button>(), "Wood");
                TMP_Text label = t.GetComponentInChildren<TMP_Text>(); label.fontSize = 28;
                Place(label.transform, CENTRE, new Vector2(19, 1), new Vector2(150, 50));
                Image(t, "Rural Icon", icons.Icon(ids[i]), new Vector2(-67, 2), new Vector2(28, 28));
            }
            RaceSceneBaker.SetReference(nav.GetComponent<AreaNavigationUI>(), "_theme", RuralUIAssets.Theme);
            RaceSceneBaker.SetReference(world.GetComponent<TrainingSelectionUI>(), "_theme", RuralUIAssets.Theme);
            Transform heading = nav.Find("Current Area");
            Place(heading, new Vector2(.5f, 1), new Vector2(0, -51), new Vector2(284, 52));
            TMP_Text title = heading.GetComponent<TMP_Text>(); title.fontSize = 30; title.color = new Color32(255, 235, 188, 255);
            Image plaque = Image(nav, "Rural Area Plaque", null, Vector2.zero, new Vector2(300, 66));
            Place(plaque.transform, new Vector2(.5f, 1), new Vector2(0, -51), new Vector2(300, 66));
            Skin(plaque.transform, "Wood"); plaque.transform.SetAsFirstSibling();

            Transform currency = world.transform.Find("Currency UI");
            ResourceChip(currency, "Coins", "Coins Icon", 96, icons.Icon(14));
            ResourceChip(currency, "Carrots", "Carrots Icon", 404, icons.Icon(17));
            Transform tokens = currency.Find("Training Token Header");
            Place(tokens, TOP_LEFT, new Vector2(250, -51), new Vector2(142, 62)); Skin(tokens, "Parchment");
            tokens.GetComponent<Image>().raycastTarget = false;
            tokens.Find("Training Token Label").gameObject.SetActive(false);
            TMP_Text count = tokens.Find("Training Tokens").GetComponent<TMP_Text>();
            Place(count.transform, CENTRE, new Vector2(18, 0), new Vector2(82, 44)); count.fontSize = 32; count.color = RuralUIAssets.Theme.Ink;
            Image(tokens, "Rural Icon", icons.Icon(15), new Vector2(-43, 0), new Vector2(30, 30));
            Place(tokens.Find("Training Token Popup"), CENTRE, new Vector2(18, -37), new Vector2(90, 30));
        }

        private static void ResourceChip(Transform currency, string value, string icon, float x, Sprite sprite)
        {
            Image chip = Image(currency, "Rural " + value + " Chip", null, Vector2.zero, new Vector2(142, 62));
            Place(chip.transform, TOP_LEFT, new Vector2(x, -51), new Vector2(142, 62)); Skin(chip.transform, "Parchment"); chip.transform.SetAsFirstSibling();
            TMP_Text text = currency.Find(value).GetComponent<TMP_Text>(); text.color = RuralUIAssets.Theme.Ink; text.fontSize = 32;
            Place(text.transform, TOP_LEFT, new Vector2(x + 18, -51), new Vector2(82, 44));
            Image image = currency.Find(icon).GetComponent<Image>(); image.sprite = sprite;
            Place(image.transform, TOP_LEFT, new Vector2(x - 43, -51), new Vector2(30, 30));
            Place(currency.Find(value + " Popup"), TOP_LEFT, new Vector2(x + 18, -88), new Vector2(100, 32));
        }

        private static void ConfigureDetails(UIDetailPanel details)
        {
            Transform panel = details.transform.Find("Details"); Skin(panel, "Parchment");
            Place(panel, new Vector2(1, 1), new Vector2(-249, -268), new Vector2(446, 282));
            RaceSceneBaker.SetReference(details, "_theme", RuralUIAssets.Theme);
            foreach (TMP_Text text in panel.GetComponentsInChildren<TMP_Text>(true)) text.color = RuralUIAssets.Theme.Ink;
            Place(panel.Find("Icon"), CENTRE, new Vector2(-175, 95), new Vector2(36, 36));
            Place(panel.Find("Title"), CENTRE, new Vector2(26, 95), new Vector2(320, 42));
            Place(panel.Find("Description"), CENTRE, new Vector2(0, 13), new Vector2(392, 104));
            Place(panel.Find("Status"), CENTRE, new Vector2(0, -61), new Vector2(392, 38));
            Place(panel.Find("Cost"), CENTRE, new Vector2(0, -106), new Vector2(392, 44));
            foreach (Transform t in panel) if (t.name.StartsWith("Divider", StringComparison.Ordinal)) t.gameObject.SetActive(false);
        }

        private static void ConfigureTree(GameplayWorld world)
        {
            Transform tree = world.transform.Find("Upgrade Tree UI/Tree Panel"); Skin(tree, "Parchment");
            world.transform.Find("Upgrade Tree UI/Tree Surround").GetComponent<Image>().color = new Color(.12f, .16f, .10f, .95f);
            Text(tree, "Rural Heading", "STABLE UPGRADES", new Vector2(0, 354), new Vector2(700, 48), 38);
            Text(tree, "Rural Hint", "Unlock a branch to add new upgrades.", new Vector2(0, 313), new Vector2(780, 28), 26, true);
            for (int i = 0; i < UpgradeTreeRules.NODE_COUNT; i++)
            {
                UpgradeNode node = (UpgradeNode)i; Transform t = tree.Find("Unlock " + node);
                if (t == null) continue;
                // Labels belong to each node so progressive reveal still hides undiscovered branches.
                TMP_Text label = Text(t, "Rural Caption", UpgradeTreeRules.Name(node), new Vector2(0, -61), new Vector2(210, 30), 24);
                label.raycastTarget = false;
                if (t.TryGetComponent(out UpgradeTreeNodeUI view)) RaceSceneBaker.SetReference(view, "_theme", RuralUIAssets.Theme);
            }
            TMP_Text feedback = tree.Find("Tree Feedback").GetComponent<TMP_Text>(); feedback.color = RuralUIAssets.Theme.Positive;
        }
    }
}
