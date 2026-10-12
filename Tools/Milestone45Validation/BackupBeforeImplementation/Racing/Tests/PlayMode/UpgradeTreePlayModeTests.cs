using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Horsecremental.Racing.Tests
{
    public sealed class UpgradeTreePlayModeTests
    {
        private bool _background;
        private InputSettings.BackgroundBehavior _inputBackground;
        private InputSettings.EditorInputBehaviorInPlayMode _editorInput;
        private Keyboard _keyboard;
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
            yield return SceneManager.LoadSceneAsync(GameScenes.WORLD); yield return null;
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1;
            if (_keyboard != null) InputSystem.RemoveDevice(_keyboard);
            Application.runInBackground = _background;
            InputSystem.settings.backgroundBehavior = _inputBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = _editorInput;
            yield return null;
        }

        [UnityTest]
        public IEnumerator FreshPlayerEarnsTokensFromRealRacesAndUnlocksFarmInTree()
        {
            GameplayWorld world = Object.FindAnyObjectByType<GameplayWorld>(); PlayerProgression player = world.Player;
            Assert.That(player.RaceTokens, Is.Zero);
            Assert.That(ProgressionLoopTests.FindButton("View Farm").interactable, Is.False);
            world.ShowFarm(); world.ShowBlacksmith();
            Assert.That(world.Area, Is.EqualTo(ManagementArea.Stable));
            Assert.That(ProgressionLoopTests.FindButton("Upgrade Acceleration").interactable, Is.False);
            player.AdvanceFarm(1000, world.Settings); Assert.That(player.StoredCarrots, Is.Zero);
            ProgressionLoopTests.FindButton("View Upgrade Tree").onClick.Invoke();
            yield return new WaitForSeconds(0.3f);
            Capture("tree-fresh", 1920, 1080); Capture("tree-fresh-4x3", 1024, 768);
            Capture("tree-fresh-wide", 2560, 1080);
            for (int cycle = 0; cycle < 5; cycle++)
            {
                world.ShowRaceArea(); world.StartMainRace();
                double training = player.TrainingElapsed;
                int tokens = player.RaceTokens;
                Time.timeScale = 10;
                yield return ProgressionLoopTests.DriveRace(world.Race);
                Assert.That(world.Race.Player.FinishPosition, Is.EqualTo(4));
                Assert.That(player.RaceTokens, Is.EqualTo(tokens + 1));
                Assert.That(player.TrainingElapsed, Is.EqualTo(training));
                Assert.That(player.StoredCarrots, Is.Zero);
                Time.timeScale = 1;
                yield return new WaitForSeconds(0.1f);
                Assert.That(player.RaceTokens, Is.EqualTo(tokens + 1), "Repeated result refresh must not pay twice.");
                Assert.That(GameObject.Find("Race Reward").GetComponent<Text>().text, Does.Contain("+1 Race Tokens"));
                if (cycle == 0) Capture("first-race-tokens", 1920, 1080);
                world.ShowStable(); yield return new WaitForSeconds(0.8f);
            }
            Assert.That(player.RaceTokens, Is.EqualTo(5));
            world.ShowUpgradeTree(); yield return new WaitForSeconds(0.3f);
            Assert.That(ProgressionLoopTests.FindButton("Unlock Farm").interactable, Is.True);
            int coins = player.Coins;
            ProgressionLoopTests.FindButton("Unlock Blacksmith").onClick.Invoke();
            Assert.That(player.IsUnlocked(UpgradeNode.Blacksmith), Is.False);
            ProgressionLoopTests.FindButton("Unlock Farm").onClick.Invoke();
            Assert.That(player.IsUnlocked(UpgradeNode.Farm), Is.True);
            Assert.That(player.Coins, Is.EqualTo(coins)); Assert.That(player.RaceTokens, Is.Zero);
            Assert.That(ProgressionLoopTests.FindButton("View Farm").interactable, Is.True);
            ProgressionLoopTests.FindButton("Unlock Acceleration").onClick.Invoke();
            ProgressionLoopTests.FindButton("Unlock BoostPower").onClick.Invoke();
            Assert.That(player.Coins, Is.EqualTo(coins - 230));
            Assert.That(player.IsUnlocked(HorseUpgrade.BoostPower), Is.True);
            Capture("tree-farm-unlocked", 1920, 1080);
            world.ShowFarm(); yield return new WaitForSeconds(0.8f);
            player.AdvanceFarm(player.CarrotSeconds(world.Settings), world.Settings);
            ProgressionLoopTests.FindButton("Collect Carrots").onClick.Invoke();
            Assert.That(player.Carrots, Is.GreaterThan(0));
            Assert.That(ProgressionLoopTests.FindButton("Upgrade GrowSpeed").interactable, Is.False);
            Assert.That(ProgressionSession.Player, Is.SameAs(player));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator KGrantsResourcesOncePerPressAndTreeStillRequiresParents()
        {
            GameplayWorld world = Object.FindAnyObjectByType<GameplayWorld>(); PlayerProgression player = world.Player;
            _keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.K)); yield return null;
            Assert.That(player.Coins, Is.EqualTo(100000)); Assert.That(player.RaceTokens, Is.EqualTo(1000));
            yield return new WaitForSeconds(0.2f);
            Assert.That(player.Coins, Is.EqualTo(100000)); Assert.That(player.RaceTokens, Is.EqualTo(1000));
            Assert.That(player.IsUnlocked(UpgradeNode.Farm), Is.False);
            Assert.That(GameObject.Find("Race Tokens").GetComponent<Text>().text, Does.Contain("1,000").Or.Contain("1.000"));
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState()); yield return null;
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.K)); yield return null;
            Assert.That(player.Coins, Is.EqualTo(200000)); Assert.That(player.RaceTokens, Is.EqualTo(2000));
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState()); yield return null;
            world.ShowUpgradeTree(); yield return new WaitForSeconds(0.3f);
            ProgressionLoopTests.FindButton("Unlock Blacksmith").onClick.Invoke();
            Assert.That(player.IsUnlocked(UpgradeNode.Blacksmith), Is.False);
            ProgressionLoopTests.FindButton("Unlock Farm").onClick.Invoke();
            ProgressionLoopTests.FindButton("Unlock Blacksmith").onClick.Invoke();
            ProgressionLoopTests.FindButton("Unlock ForgeSpeed").onClick.Invoke();
            ProgressionLoopTests.FindButton("Unlock MetalEfficiency").onClick.Invoke();
            Assert.That(player.RaceTokens, Is.EqualTo(1985)); Assert.That(player.Coins, Is.EqualTo(199650));
            Assert.That(player.IsUnlocked(ProductionUpgrade.MetalEfficiency), Is.True);
            Capture("tree-cheat-and-smith", 1280, 720);
            world.ShowBlacksmith(); yield return new WaitForSeconds(0.3f);
            Assert.That(world.Area, Is.EqualTo(ManagementArea.Blacksmith));
            Assert.That(ProgressionLoopTests.FindButton("Upgrade ForgeSpeed").interactable, Is.True);
            LogAssert.NoUnexpectedReceived();
        }

        private static void Capture(string name, int width, int height)
        {
            ProgressionLoopTests.Capture(name, width, height);
            const string SOURCE = "Tools/Milestone4Validation/Screenshots/";
            const string DESTINATION = "Tools/UpgradeTreeValidation/Screenshots/";
            System.IO.Directory.CreateDirectory(DESTINATION);
            System.IO.File.Copy(SOURCE + name + ".png", DESTINATION + name + ".png", true);
        }
    }
}
