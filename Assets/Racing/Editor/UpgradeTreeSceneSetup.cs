using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Horsecremental.Racing.Editor
{
    public static class UpgradeTreeSceneSetup
    {
        public const string TREE_PATH = "Assets/Racing/Config/UpgradeTree.asset";
        [MenuItem("Horsecremental/Setup Upgrade Tree")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (SceneManager.GetActiveScene().path != Milestone3SceneSetup.WORLD_PATH)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorSceneManager.OpenScene(Milestone3SceneSetup.WORLD_PATH);
            }
            GameplayWorld world = UnityEngine.Object.FindAnyObjectByType<GameplayWorld>();
            if (world == null) throw new InvalidOperationException("The existing gameplay world is required.");
            ProgressionSettings settings = AssetDatabase.LoadAssetAtPath<ProgressionSettings>(Milestone2SceneSetup.SETTINGS_PATH);
            Apply(world, settings);
            EditorSceneManager.MarkSceneDirty(world.gameObject.scene);
            EditorSceneManager.SaveScene(world.gameObject.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Upgrade Tree saved. Farm and Blacksmith require Race Tokens. K grants debug Coins and Race Tokens.");
        }

        internal static void Apply(GameplayWorld world, ProgressionSettings settings)
        {
            UpgradeTreeSettings tree = AssetDatabase.LoadAssetAtPath<UpgradeTreeSettings>(TREE_PATH);
            if (tree == null)
            {
                tree = ScriptableObject.CreateInstance<UpgradeTreeSettings>();
                AssetDatabase.CreateAsset(tree, TREE_PATH);
            }
            tree.CreateRules();
            RaceSceneBaker.SetReference(settings, "_upgradeTree", tree);
            if (!world.TryGetComponent(out DebugResourceCheat cheat)) cheat = world.gameObject.AddComponent<DebugResourceCheat>();
            RaceSceneBaker.SetReference(cheat, "_settings", settings);
            ConfigureNavigation(world, settings);
            ConfigureResources(world);
            UpgradeTreeUIBuilder.Build(world.transform, world, settings);
            RaceUI raceUI = world.GetComponentInChildren<RaceUI>(true);
            Text reward = raceUI.transform.Find("Race result/Race Reward").GetComponent<Text>();
            reward.fontSize = 23;
            Text description = raceUI.transform.Find("Race entry/Description").GetComponent<Text>();
            description.text = "Main Race rewards Race Tokens by place\n1st: 5   2nd: 3   3rd: 2   4th: 1";
            world.transform.Find("Stable Upgrades UI/Upgrades/Upgrade Feedback").GetComponent<Text>().text =
                "Unlock new upgrades in the Upgrade Tree.\nRace to earn tokens for Farm and Blacksmith.";
            MinimalUISetup.Apply(world, settings);
            if (world.TrainingLadder != null) TrainingRaceSceneSetup.Apply(world);
        }

        private static void ConfigureNavigation(GameplayWorld world, ProgressionSettings settings)
        {
            Transform root = world.transform.Find("Area Navigation UI");
            AreaNavigationUI ui = root.GetComponent<AreaNavigationUI>();
            RaceSceneBaker.SetReference(ui, "_settings", settings);
            Button[] buttons = new Button[5];
            string[] names = { "View Stable", "View Farm", "View Blacksmith", "View Race", "View Upgrade Tree" };
            for (int i = 0; i < buttons.Length; i++)
            {
                Transform existing = root.Find(names[i]);
                buttons[i] = existing != null ? existing.GetComponent<Button>() :
                    RaceUIBuilder.Button(root, names[i], "UPGRADE TREE", Vector2.zero);
                RectTransform rect = buttons[i].GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0);
                rect.anchoredPosition = new Vector2((i - 2) * 255, 48);
                rect.sizeDelta = new Vector2(235, 65);
                Text label = buttons[i].GetComponentInChildren<Text>();
                label.fontSize = 22; label.rectTransform.sizeDelta = new Vector2(221, 60);
                label.text = i == 1 || i == 2 ? (i == 1 ? "FARM" : "BLACKSMITH") + "\nLOCKED - TREE" :
                    i == 4 ? "UPGRADE TREE" : i == 0 ? "STABLE [A]" : "MAIN RACE [D]";
            }
            RaceSceneBaker.SetArray(ui, "_buttons", buttons);
        }

        private static void ConfigureResources(GameplayWorld world)
        {
            Transform root = world.transform.Find("Currency UI");
            CurrencyFeedback ui = root.GetComponent<CurrencyFeedback>();
            MoveHeader(root, "Coins", -650, 300);
            MoveHeader(root, "Metal", 180, 260);
            MoveHeader(root, "Carrots", 600, 550);
            Text total = TokenLabel(root, "Race Tokens", -240, -32, 25);
            Text popup = TokenLabel(root, "Race Tokens Popup", -240, -62, 20);
            total.color = new Color(0.8f, 0.65f, 1); total.text = "RACE TOKENS  0";
            popup.color = new Color(0.8f, 0.65f, 1, 0);
            RaceSceneBaker.SetReference(ui, "_tokens", total);
            RaceSceneBaker.SetReference(ui, "_tokenPopup", popup);
        }

        private static Text TokenLabel(Transform parent, string name, float x, float y, int size)
        {
            Transform existing = parent.Find(name);
            Text text = existing != null ? existing.GetComponent<Text>() :
                RaceUIBuilder.Label(parent, name, Vector2.zero, new Vector2(430, 45), size);
            text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(0.5f, 1);
            text.rectTransform.anchoredPosition = new Vector2(x, y);
            return text;
        }
        private static void MoveHeader(Transform root, string name, float x, float width)
        {
            foreach (string suffix in new[] { "", " Popup" })
            {
                Text text = root.Find(name + suffix).GetComponent<Text>();
                text.rectTransform.anchoredPosition = new Vector2(x, suffix.Length == 0 ? -32 : -62);
                text.rectTransform.sizeDelta = new Vector2(width, suffix.Length == 0 ? 45 : 30);
                text.fontSize = suffix.Length == 0 ? 24 : 20;
            }
        }
    }
}
