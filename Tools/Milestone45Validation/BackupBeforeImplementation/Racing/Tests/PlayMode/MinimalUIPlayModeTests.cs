using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Horsecremental.Racing.Tests
{
    public sealed class MinimalUIPlayModeTests
    {
        private bool _background;
        private InputSettings.BackgroundBehavior _inputBackground;
        private InputSettings.EditorInputBehaviorInPlayMode _editorInput;
        private Keyboard _keyboard;
        private GameplayWorld _world;
        private UIDetailPanel _panel;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _background = Application.runInBackground;
            _inputBackground = InputSystem.settings.backgroundBehavior;
            _editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            Application.runInBackground = true;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            ProgressionSession.Reset();
            yield return SceneManager.LoadSceneAsync(GameScenes.WORLD);
            yield return new WaitForSeconds(0.4f);
            _world = Object.FindAnyObjectByType<GameplayWorld>();
            _panel = Object.FindAnyObjectByType<UIDetailPanel>();
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1f;
            if (_keyboard != null) InputSystem.RemoveDevice(_keyboard);
            Application.runInBackground = _background;
            InputSystem.settings.backgroundBehavior = _inputBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = _editorInput;
            yield return null;
        }
        [UnityTest]
        public IEnumerator HoverFocusAndPurchasesKeepDetailsCurrentAndClearAcrossAreas()
        {
            Assert.That(_panel.IsVisible, Is.False);
            _world.ShowUpgradeTree(); yield return new WaitForSeconds(0.4f);
            Button smith = ProgressionLoopTests.FindButton("Unlock Blacksmith");
            ExecuteEvents.Execute(smith.gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerEnterHandler);
            Assert.That(_panel.Title, Is.EqualTo("Unlock Blacksmith"));
            Assert.That(_panel.Status, Does.Contain("Requires"));
            int coins = _world.Player.Coins;
            int tokens = _world.Player.RaceTokens;
            smith.onClick.Invoke();
            Assert.That(_world.Player.Coins, Is.EqualTo(coins));
            Assert.That(_world.Player.RaceTokens, Is.EqualTo(tokens));
            _world.Player.GrantDebugResources(50000,100);
            Button farm = ProgressionLoopTests.FindButton("Unlock Farm");
            EventSystem.current.SetSelectedGameObject(farm.gameObject);
            Assert.That(_panel.Title, Is.EqualTo("Unlock Farm"));
            int before = _world.Player.RaceTokens;
            ExecuteEvents.Execute(farm.gameObject,new BaseEventData(EventSystem.current),ExecuteEvents.submitHandler);
            Assert.That(_world.Player.RaceTokens, Is.EqualTo(before-5));
            Assert.That(_panel.Status, Does.Contain("UNLOCKED"));
            ExecuteEvents.Execute(farm.gameObject,new BaseEventData(EventSystem.current),ExecuteEvents.submitHandler);
            Assert.That(_world.Player.RaceTokens, Is.EqualTo(before-5));
            _world.ShowFarm();
            Assert.That(_panel.IsVisible, Is.False);
            yield return new WaitForSeconds(0.4f);
            GameObject.Find("Upgrade GrowSpeed").GetComponent<UIDetailTarget>().Inspect();
            Assert.That(_panel.Status, Does.Contain("Unlock in Upgrade Tree"));
            _world.ShowUpgradeTree(); yield return new WaitForSeconds(0.4f);
            ProgressionLoopTests.FindButton("Unlock GrowSpeed").onClick.Invoke();
            _world.ShowFarm(); yield return new WaitForSeconds(0.4f);
            Button grow = ProgressionLoopTests.FindButton("Upgrade GrowSpeed");
            grow.GetComponent<UIDetailTarget>().Inspect();
            int level = _world.Player.ProductionLevel(ProductionUpgrade.GrowSpeed);
            int price = _world.Player.ProductionCost(ProductionUpgrade.GrowSpeed);
            coins = _world.Player.Coins;
            grow.onClick.Invoke();
            Assert.That(_world.Player.ProductionLevel(ProductionUpgrade.GrowSpeed), Is.EqualTo(level+1));
            Assert.That(_world.Player.Coins, Is.EqualTo(coins-price));
            Assert.That(_panel.Status, Does.Contain("Lv. 1"));
            while (_world.Player.TryUpgradeProduction(ProductionUpgrade.GrowSpeed,_world.Settings)) { }
            UIDetailContent max = UIDetailFormatter.Create(DetailKind.Production,(int)ProductionUpgrade.GrowSpeed,_world.Player,_world);
            Assert.That(max.Cost, Is.EqualTo("MAX LEVEL"));
            Assert.That(grow.interactable, Is.False);
            grow.GetComponent<UIDetailTarget>().Inspect();
            Assert.That(_panel.IsVisible, Is.True);
            _world.ShowStable(); yield return new WaitForSeconds(0.4f);
            Assert.That(_panel.IsVisible, Is.False);
            _world.Player.TryUnlock(UpgradeNode.Acceleration);
            Button speed = ProgressionLoopTests.FindButton("Upgrade Speed");
            speed.GetComponent<UIDetailTarget>().Inspect();
            coins = _world.Player.Coins; price = _world.Player.Cost(HorseUpgrade.Speed);
            level = _world.Player.Level(HorseUpgrade.Speed);
            speed.onClick.Invoke();
            Assert.That(_world.Player.Level(HorseUpgrade.Speed), Is.EqualTo(level+1));
            Assert.That(_world.Player.Coins, Is.EqualTo(coins-price));
            Assert.That(_panel.Status, Does.Contain("Lv. " + (level+1)));
            yield return new WaitForSecondsRealtime(2.8f);
            Assert.That(GameObject.Find("Upgrade Feedback").GetComponent<Text>().text, Is.Empty);
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest]
        public IEnumerator TabAndArrowFocusCanInspectDisabledNodesWithoutPurchasing()
        {
            _world.ShowUpgradeTree(); yield return new WaitForSeconds(0.4f);
            _keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(_keyboard,new KeyboardState(Key.Tab)); yield return null;
            Assert.That(_panel.IsVisible, Is.True);
            Button smith = ProgressionLoopTests.FindButton("Unlock Blacksmith");
            Assert.That(smith.interactable, Is.False);
            EventSystem.current.SetSelectedGameObject(smith.gameObject);
            Assert.That(_panel.Status, Does.Contain("Requires"));
            ExecuteEvents.Execute(smith.gameObject,new AxisEventData(EventSystem.current) { moveDir = MoveDirection.Right },ExecuteEvents.moveHandler);
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.Not.EqualTo(smith.gameObject));
            Assert.That(_panel.IsVisible, Is.True);
            Assert.That(_world.Player.IsUnlocked(UpgradeNode.Blacksmith), Is.False);
        }
        [UnityTest]
        public IEnumerator AllScreensFitSupportedAspectRatiosAndProduceVisualEvidence()
        {
            _world.Player.GrantDebugResources(10000,100);
            ProgressionLoopTests.UnlockAll(_world.Player);
            int[] widths = {1920,1280,1024,2560}; int[] heights = {1080,720,768,1080};
            string[] names = {"stable","farm","blacksmith","tree","race"};
            string[] targets = {"Upgrade Speed","Upgrade GrowSpeed","Craft Horseshoes","Unlock Farm","Start Race"};
            for (int area = 0; area < names.Length; area++)
            {
                switch (area)
                {
                    case 0: _world.ShowStable(); break;
                    case 1: _world.ShowFarm(); break;
                    case 2: _world.ShowBlacksmith(); break;
                    case 3: _world.ShowUpgradeTree(); break;
                    default: _world.ShowRaceArea(); break;
                }
                yield return new WaitForSeconds(0.8f);
                GameObject.Find(targets[area]).GetComponent<UIDetailTarget>().Inspect();
                for (int ratio = 0; ratio < widths.Length; ratio++)
                {
                    string name = "minimal-"+names[area]+"-"+widths[ratio]+"x"+heights[ratio];
                    Capture(name,widths[ratio],heights[ratio]);
                }
            }
            _world.StartMainRace(); yield return new WaitForSeconds(0.8f);
            Assert.That(_panel.IsVisible, Is.False);
            LogAssert.NoUnexpectedReceived();
        }
        private static void Capture(string name, int width, int height)
        {
            ProgressionLoopTests.Capture(name,width,height, () => ValidateLayout(width,height));
            const string DESTINATION = "Tools/MinimalUIValidation/Screenshots/";
            Directory.CreateDirectory(DESTINATION);
            File.Copy("Tools/Milestone4Validation/Screenshots/"+name+".png",DESTINATION+name+".png",true);
        }
        private static void ValidateLayout(int width, int height)
        {
            RectTransform details = (RectTransform)GameObject.Find("Shared Details UI").transform.Find("Details");
            Rect panel = ScreenRect(details);
            AssertInside(panel,width,height,"Details");
            Rect controls = default;
            GameObject controlsObject = GameObject.Find("Controls");
            bool showControls = controlsObject != null && IsVisible(controlsObject.transform);
            if (showControls) { controls = ScreenRect(controlsObject.GetComponent<RectTransform>()); AssertInside(controls,width,height,"Controls"); }
            foreach (Button button in Object.FindObjectsByType<Button>())
            {
                if (!IsVisible(button.transform)) continue;
                Rect bounds = ScreenRect(button.GetComponent<RectTransform>());
                AssertInside(bounds,width,height,button.name);
                Assert.That(panel.Overlaps(bounds), Is.False, "Details cover " + button.name);
                if (showControls) Assert.That(controls.Overlaps(bounds), Is.False,"Controls cover " + button.name);
            }
            UpgradeTreeNodeUI[] nodes = Object.FindObjectsByType<UpgradeTreeNodeUI>();
            if (nodes.Length == 0 || !IsVisible(nodes[0].transform)) return;
            for (int i = 0; i < nodes.Length; i++)
                for (int j = i+1; j < nodes.Length; j++)
                    Assert.That(ScreenRect(nodes[i].GetComponent<RectTransform>()).Overlaps(ScreenRect(nodes[j].GetComponent<RectTransform>())), Is.False);
        }
        private static bool IsVisible(Transform target)
        {
            foreach (CanvasGroup group in target.GetComponentsInParent<CanvasGroup>()) if (group.alpha < 0.9f) return false;
            return target.gameObject.activeInHierarchy;
        }
        private static Rect ScreenRect(RectTransform transform)
        {
            Vector3[] corners = new Vector3[4]; transform.GetWorldCorners(corners);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(Camera.main,corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(Camera.main,corners[2]);
            return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
        }
        private static void AssertInside(Rect bounds, int width, int height, string name)
        {
            Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(-1),name);
            Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(-1),name);
            Assert.That(bounds.xMax, Is.LessThanOrEqualTo(width+1),name);
            Assert.That(bounds.yMax, Is.LessThanOrEqualTo(height+1),name);
        }
    }
}
