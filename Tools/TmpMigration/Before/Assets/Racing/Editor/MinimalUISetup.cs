using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing.Editor
{
    public static class MinimalUISetup
    {
        private static readonly Color PANEL = new Color(0.065f, 0.07f, 0.08f, 0.98f);
        [MenuItem("Horsecremental/Simplify UI")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != Milestone3SceneSetup.WORLD_PATH)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorSceneManager.OpenScene(Milestone3SceneSetup.WORLD_PATH);
            }
            GameplayWorld world = UnityEngine.Object.FindAnyObjectByType<GameplayWorld>();
            if (world == null) throw new InvalidOperationException("Gameplay world is required.");
            if (world.PlaygroundSettings != null) { Milestone45SceneSetup.Apply(); return; }
            ProgressionSettings settings = AssetDatabase.LoadAssetAtPath<ProgressionSettings>(Milestone2SceneSetup.SETTINGS_PATH);
            UpgradeTreeUIBuilder.Build(world.transform, world, settings);
            Apply(world, settings);
            EditorSceneManager.MarkSceneDirty(world.gameObject.scene);
            EditorSceneManager.SaveScene(world.gameObject.scene);
            AssetDatabase.SaveAssets();
        }
        internal static void Apply(GameplayWorld world, ProgressionSettings settings)
        {
            // The legacy world is converted to the workstation layout by Milestone 4 first.
            if (world.transform.Find("Farm UI") == null) return;
            UIPresentationSettings presentation = UIIconAssets.Ensure();
            Transform previous = world.transform.Find("Shared Details UI");
            if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
            UIDetailPanel details = BuildDetails(world, settings);
            Transform root = world.transform;
            ConfigureTree(root, details, presentation, settings);
            ConfigureStable(root, details, presentation, settings);
            ConfigureWorkstations(root, details, presentation, settings);
            ConfigureResources(root, details, presentation);
            ConfigureNavigation(root, details, presentation);
            ConfigureRace(root, details, presentation);
            foreach (CanvasScaler scaler in root.GetComponentsInChildren<CanvasScaler>(true))
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            }
            foreach (Text text in root.GetComponentsInChildren<Text>(true))
                if (text.name.EndsWith("Feedback", StringComparison.Ordinal))
                {
                    text.text = string.Empty;
                    Ensure<TransientUIFeedback>(text.gameObject);
                }
            UIDetailKeyboardTraversal traversal = Ensure<UIDetailKeyboardTraversal>(details.gameObject);
            RaceSceneBaker.SetArray(traversal, "_targets", root.GetComponentsInChildren<UIDetailTarget>(true));
        }
        private static UIDetailPanel BuildDetails(GameplayWorld world, ProgressionSettings settings)
        {
            GameObject root = new GameObject("Shared Details UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            root.transform.SetParent(world.transform, false);
            Canvas canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 30;
            UIDetailPanel ui = root.AddComponent<UIDetailPanel>();
            Image panel = RaceUIBuilder.Panel(root.transform, "Details", Vector2.zero, new Vector2(440, 310), new Color(0.025f, 0.025f, 0.03f, 0.99f));
            Anchor(panel.rectTransform, new Vector2(1,1), new Vector2(-260,-300));
            panel.raycastTarget = false;
            CanvasGroup group = panel.gameObject.AddComponent<CanvasGroup>(); group.alpha = 0; group.blocksRaycasts = group.interactable = false;
            Image icon = SpriteImage(panel.transform, "Icon", null, new Vector2(-178,113), new Vector2(40,40));
            Text title = Label(panel.transform,"Title",new Vector2(28,112),new Vector2(325,50),26);
            Text description = Label(panel.transform,"Description",new Vector2(0,20),new Vector2(394,125),21);
            Text status = Label(panel.transform,"Status",new Vector2(0,-77),new Vector2(394,45),21);
            Text cost = Label(panel.transform,"Cost",new Vector2(0,-126),new Vector2(394,48),20);
            foreach (float y in new[] { 81f, -53f, -102f })
            {
                Image line = RaceUIBuilder.Panel(panel.transform,"Divider " + y,new Vector2(0,y),new Vector2(440,2),new Color(0.48f,0.49f,0.52f));
                line.raycastTarget = false;
            }
            Bind(ui,"_world",world); Bind(ui,"_settings",settings); Bind(ui,"_group",group); Bind(ui,"_icon",icon);
            Bind(ui,"_title",title); Bind(ui,"_description",description); Bind(ui,"_status",status); Bind(ui,"_cost",cost);
            return ui;
        }
        private static void ConfigureTree(Transform root, UIDetailPanel details, UIPresentationSettings presentation, ProgressionSettings settings)
        {
            Transform tree = root.Find("Upgrade Tree UI/Tree Panel");
            tree.GetComponent<Image>().color = new Color(0.075f,0.075f,0.085f,1);
            foreach (string name in new[] { "Heading", "Tree Resources", "Tree Help" }) tree.Find(name).gameObject.SetActive(false);
            Button[] buttons = new Button[UpgradeTreeRules.NODE_COUNT];
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = tree.Find("Unlock " + (UpgradeNode)i).GetComponent<Button>();
                IconButton(buttons[i], details, presentation, settings, DetailKind.Tree, i, i);
            }
            ConnectNavigation(buttons);
            UpgradeTreePresentationSetup.ApplyTree(root, settings, presentation);
        }
        private static void ConfigureStable(Transform root, UIDetailPanel details, UIPresentationSettings presentation, ProgressionSettings settings)
        {
            Transform stable = root.Find("Stable Upgrades UI/Upgrades");
            Place(stable,new Vector2(230,0),new Vector2(360,420)); stable.GetComponent<Image>().color = PANEL;
            Place(stable.Find("Upgrade Heading"),new Vector2(0,155),new Vector2(330,45));
            stable.Find("Upgrade Heading").GetComponent<Text>().text = "HORSE";
            Place(stable.Find("Current Stats"),new Vector2(0,45),new Vector2(320,145));
            string[] names = { "Upgrade Speed", "Upgrade Acceleration", "Upgrade Boost Power" };
            Button[] horse = new Button[3];
            for (int i = 0; i < horse.Length; i++)
            {
                horse[i] = stable.Find(names[i]).GetComponent<Button>();
                Place(horse[i].transform,new Vector2((i-1)*105,-95),new Vector2(80,80));
                IconButton(horse[i],details,presentation,settings,DetailKind.Horse,i,i);
            }
            ConnectNavigation(horse);
            Place(stable.Find("Upgrade Feedback"),new Vector2(0,-175),new Vector2(330,55));
            Transform training = root.Find("Management UI/Training Panel");
            Place(training,new Vector2(-260,-160),new Vector2(440,340)); training.GetComponent<Image>().color = PANEL;
            Place(training.Find("Training Status"),new Vector2(0,100),new Vector2(410,60));
            Place(training.Find("Training Progress"),new Vector2(0,38),new Vector2(410,12));
            Target(training.gameObject,details,DetailKind.Training,0,presentation.Icon(5));
            ProductionButtons(training,details,presentation,settings,new Vector2(-70,-60),new Vector2(70,-60));
            Transform difficulty = root.Find("Management UI/Main Race Power");
            if (difficulty != null) { Place(difficulty,new Vector2(-260,-365),new Vector2(420,40)); Target(difficulty.gameObject,details,DetailKind.RaceHelp,0,presentation.Icon(5)); }
        }
        private static void ConfigureWorkstations(Transform root, UIDetailPanel details, UIPresentationSettings presentation, ProgressionSettings settings)
        {
            Transform farm = root.Find("Farm UI/Farm Panel");
            Place(farm,new Vector2(265,0),new Vector2(470,340)); farm.GetComponent<Image>().color = PANEL;
            Place(farm.Find("Farm Status"),new Vector2(0,82),new Vector2(420,80));
            Place(farm.Find("Farm Progress"),new Vector2(0,12),new Vector2(410,12));
            Place(farm.Find("Collect Carrots"),new Vector2(-110,-60),new Vector2(205,55));
            Place(farm.Find("Feed Horse"),new Vector2(110,-60),new Vector2(205,55));
            Place(farm.Find("Farm Feedback"),new Vector2(0,-130),new Vector2(430,45));
            Target(farm.gameObject,details,DetailKind.Farm,0,presentation.Icon(3));
            Target(farm.Find("Collect Carrots").gameObject,details,DetailKind.Farm,0,presentation.Icon(17));
            Target(farm.Find("Feed Horse").gameObject,details,DetailKind.Feed,0,presentation.Icon(17));
            Transform upgrades = root.Find("Farm UI/Farm Upgrades");
            Place(upgrades,new Vector2(-265,-120),new Vector2(440,230)); upgrades.GetComponent<Image>().color = PANEL;
            Place(upgrades.Find("Heading"),new Vector2(0,72),new Vector2(410,40)); upgrades.Find("Heading").GetComponent<Text>().text = "FARM";
            ProductionButtons(upgrades,details,presentation,settings,new Vector2(-70,-20),new Vector2(70,-20));
            Transform smith = root.Find("Blacksmith UI");
            Place(smith.Find("Craft Status"),new Vector2(-260,230),new Vector2(1000,55));
            Place(smith.Find("Craft Progress"),new Vector2(-260,183),new Vector2(780,12));
            string[] names = { "Horseshoes", "Saddle", "Bridle" };
            for (int i = 0; i < names.Length; i++)
            {
                Transform card = smith.Find(names[i] + " Card");
                Place(card,new Vector2((i-1)*330,-65),new Vector2(290,270)); card.GetComponent<Image>().color = PANEL;
                SpriteImage(card,"Equipment Icon",presentation.Icon(11+i),new Vector2(0,72),new Vector2(56,56));
                Place(card.Find("Equipment Details"),new Vector2(0,0),new Vector2(270,80));
                Button craft = card.Find("Craft " + names[i]).GetComponent<Button>();
                Place(craft.transform,new Vector2(0,-92),new Vector2(245,52));
                craft.GetComponentInChildren<Text>().rectTransform.sizeDelta = new Vector2(235,48);
                Target(card.gameObject,details,DetailKind.Equipment,i,presentation.Icon(11+i));
                Target(craft.gameObject,details,DetailKind.Equipment,i,presentation.Icon(11+i));
            }
            ProductionButtons(smith,details,presentation,settings,new Vector2(-70,-300),new Vector2(70,-300));
            Place(smith.Find("Craft Feedback"),new Vector2(0,-395),new Vector2(1200,45));
        }
        private static void ProductionButtons(Transform parent, UIDetailPanel details, UIPresentationSettings presentation, ProgressionSettings settings, Vector2 first, Vector2 second)
        {
            ProductionUpgradeUI[] upgrades = parent.GetComponentsInChildren<ProductionUpgradeUI>(true);
            Button[] buttons = new Button[upgrades.Length];
            for (int i = 0; i < upgrades.Length; i++)
            {
                ProductionUpgrade upgrade = (ProductionUpgrade)new SerializedObject(upgrades[i]).FindProperty("_upgrade").intValue;
                buttons[i] = upgrades[i].GetComponent<Button>();
                Place(buttons[i].transform,i == 0 ? first : second,new Vector2(80,80));
                IconButton(buttons[i],details,presentation,settings,DetailKind.Production,(int)upgrade,(int)UpgradeTreeRules.NodeFor(upgrade));
            }
            ConnectNavigation(buttons);
        }
        private static void ConfigureResources(Transform root, UIDetailPanel details, UIPresentationSettings presentation)
        {
            Transform resources = root.Find("Currency UI");
            string[] names = { "Coins", "Race Tokens", "Metal", "Carrots" };
            for (int i = 0; i < names.Length; i++)
            {
                Text total = resources.Find(names[i]).GetComponent<Text>();
                float x = (i-1.5f)*300;
                Anchor(total.rectTransform,new Vector2(0.5f,1),new Vector2(x+70,-36)); total.rectTransform.sizeDelta = new Vector2(190,45);
                total.text = "0"; total.alignment = TextAnchor.MiddleLeft; total.fontSize = 24; total.raycastTarget = true;
                Image icon = SpriteImage(resources,names[i] + " Icon",presentation.Icon(14+i),Vector2.zero,new Vector2(32,32));
                Anchor(icon.rectTransform,new Vector2(0.5f,1),new Vector2(x-55,-36)); icon.raycastTarget = true;
                Target(icon.gameObject,details,DetailKind.Resource,i,presentation.Icon(14+i));
                Target(total.gameObject,details,DetailKind.Resource,i,presentation.Icon(14+i));
                Transform popup = resources.Find(names[i] + " Popup");
                Anchor((RectTransform)popup,new Vector2(0.5f,1),new Vector2(x+25,-68)); ((RectTransform)popup).sizeDelta = new Vector2(250,30);
            }
            Transform toast = resources.Find("Craft Complete Toast");
            if (toast != null) { Place(toast,new Vector2(0,-165),new Vector2(680,60)); Place(toast.Find("Craft Complete Banner"),Vector2.zero,new Vector2(650,55)); }
        }
        private static void ConfigureNavigation(Transform root, UIDetailPanel details, UIPresentationSettings presentation)
        {
            Transform navigation = root.Find("Area Navigation UI");
            string[] names = { "View Stable", "View Farm", "View Blacksmith", "View Race", "View Upgrade Tree" };
            int[] icons = { 0,3,4,5,1 };
            for (int i = 0; i < names.Length; i++)
            {
                Button button = navigation.Find(names[i]).GetComponent<Button>();
                button.GetComponentInChildren<Text>().text = i == 0 ? "STABLE" : i == 1 ? "FARM" : i == 2 ? "BLACKSMITH" : i == 3 ? "MAIN RACE" : "UPGRADE TREE";
                button.GetComponent<Image>().color = PANEL;
                Target(button.gameObject,details,DetailKind.Navigation,i,presentation.Icon(icons[i]));
            }
        }
        private static void ConfigureRace(Transform root, UIDetailPanel details, UIPresentationSettings presentation)
        {
            RaceUI race = root.GetComponentInChildren<RaceUI>(true);
            race.transform.Find("Controls").GetComponent<Text>().text = "SPACE   /   E";
            race.transform.Find("Boost").GetComponent<Text>().text = "SPACE";
            race.transform.Find("Race entry/Description").GetComponent<Text>().text = string.Empty;
            race.transform.Find("Race result/Restart Race").GetComponentInChildren<Text>().text = "RACE AGAIN";
            Target(race.transform.Find("Race entry/Start Race").gameObject,details,DetailKind.RaceHelp,0,presentation.Icon(5));
            Text controls = race.transform.Find("Controls").GetComponent<Text>(); controls.raycastTarget = true;
            Anchor(controls.rectTransform,new Vector2(0.5f,0),new Vector2(0,130));
            controls.rectTransform.sizeDelta = new Vector2(300,45); controls.fontSize = 22;
            Target(controls.gameObject,details,DetailKind.RaceHelp,0,presentation.Icon(5));
            Text boost = race.transform.Find("Boost").GetComponent<Text>(); boost.raycastTarget = true;
            Target(boost.gameObject,details,DetailKind.RaceHelp,0,presentation.Icon(2));
        }
        private static void IconButton(Button button, UIDetailPanel details, UIPresentationSettings presentation, ProgressionSettings settings, DetailKind kind, int id, int iconId)
        {
            foreach (string name in new[] { "Icon", "Border", "State Mark", "Focus" })
            { Transform old = button.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            button.GetComponentInChildren<Text>().text = string.Empty;
            button.GetComponentInChildren<Text>().rectTransform.sizeDelta = Vector2.zero;
            button.GetComponent<Image>().color = PANEL;
            ColorBlock colors = button.colors; colors.disabledColor = Color.white; colors.highlightedColor = new Color(1.4f,1.4f,1.4f); button.colors = colors;
            SpriteImage(button.transform,"Icon",presentation.Icon(iconId),Vector2.zero,new Vector2(40,40));
            Image border = SpriteImage(button.transform,"Border",presentation.Icon(20),Vector2.zero,new Vector2(80,80));
            Image mark = SpriteImage(button.transform,"State Mark",presentation.Icon(18),new Vector2(28,-28),new Vector2(18,18));
            Image focus = SpriteImage(button.transform,"Focus",presentation.Icon(20),Vector2.zero,new Vector2(90,90)); focus.enabled = false;
            UpgradeIconState state = Ensure<UpgradeIconState>(button.gameObject);
            Bind(state,"_settings",settings); SetInt(state,"_kind",(int)kind); SetInt(state,"_id",id); Bind(state,"_border",border); Bind(state,"_mark",mark);
            Bind(state,"_lock",presentation.Icon(18)); Bind(state,"_check",presentation.Icon(19));
            Target(button.gameObject,details,kind,id,presentation.Icon(iconId));
            Bind(button.GetComponent<UIDetailTarget>(),"_focus",focus);
        }
        private static void ConnectNavigation(Button[] buttons)
        {
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].navigation = new Navigation { mode = Navigation.Mode.Explicit,
                    selectOnLeft = buttons[(i+buttons.Length-1)%buttons.Length], selectOnRight = buttons[(i+1)%buttons.Length],
                    selectOnUp = buttons[(i+buttons.Length-1)%buttons.Length], selectOnDown = buttons[(i+1)%buttons.Length] };
        }
        private static void Target(GameObject target, UIDetailPanel panel, DetailKind kind, int id, Sprite icon)
        {
            UIDetailTarget hover = Ensure<UIDetailTarget>(target); Bind(hover,"_panel",panel); Bind(hover,"_icon",icon); SetInt(hover,"_kind",(int)kind); SetInt(hover,"_id",id);
        }
        private static Image SpriteImage(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size)
        {
            Transform old = parent.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            Image image = RaceUIBuilder.Panel(parent,name,position,size,Color.white); image.sprite = sprite; image.preserveAspect = true; image.raycastTarget = false; return image;
        }
        private static Text Label(Transform parent, string name, Vector2 position, Vector2 size, int font)
            => RaceUIBuilder.Label(parent,name,position,size,font);
        private static T Ensure<T>(GameObject target) where T : Component => target.TryGetComponent(out T component) ? component : target.AddComponent<T>();
        private static void Bind(UnityEngine.Object target, string field, UnityEngine.Object value) => RaceSceneBaker.SetReference(target,field,value);
        private static void SetInt(UnityEngine.Object target, string field, int value)
        { SerializedObject data = new SerializedObject(target); data.FindProperty(field).intValue = value; data.ApplyModifiedPropertiesWithoutUndo(); }
        private static void Anchor(RectTransform rect, Vector2 anchor, Vector2 position) { rect.anchorMin = rect.anchorMax = anchor; rect.anchoredPosition = position; }
        private static void Place(Transform target, Vector2 position, Vector2 size) { RectTransform rect = (RectTransform)target; rect.anchoredPosition = position; rect.sizeDelta = size; }
    }
}
