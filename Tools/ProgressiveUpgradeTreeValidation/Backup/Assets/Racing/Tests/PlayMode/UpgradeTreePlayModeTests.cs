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
        public IEnumerator ImmediateFarmAndTreeUnlocksDriveCompactProductionButtons()
        {
            GameplayWorld world = Object.FindAnyObjectByType<GameplayWorld>(); PlayerProgression player = world.Player;
            Assert.That(ProgressionLoopTests.FindButton("View Farm").interactable, Is.True);
            Assert.That(player.IsUnlocked(UpgradeNode.Farm), Is.True); Assert.That(GameObject.Find("View Blacksmith"), Is.Null);
            Assert.That(ProgressionLoopTests.FindButton("GrowSpeed").interactable, Is.False);
            Assert.That(ProgressionLoopTests.FindButton("TrainingSpeed").interactable, Is.False);
            player.GrantDebugResources(10000, 0); world.ShowUpgradeTree(); yield return new WaitForSeconds(0.3f);
            Assert.That(GameObject.Find("Unlock Blacksmith"), Is.Null);
            Assert.That(GameObject.Find("Unlock ForgeSpeed"), Is.Null);
            ProgressionLoopTests.FindButton("Unlock Acceleration").onClick.Invoke();
            ProgressionLoopTests.FindButton("Unlock TrainingSpeed").onClick.Invoke();
            ProgressionLoopTests.FindButton("Unlock TrainingReward").onClick.Invoke();
            ProgressionLoopTests.FindButton("Unlock GrowSpeed").onClick.Invoke();
            world.ShowFarm(); yield return new WaitForSeconds(0.3f);
            ProgressionLoopTests.FindButton("GrowSpeed").onClick.Invoke();
            ProgressionLoopTests.FindButton("ClickGrowthPower").onClick.Invoke();
            ProgressionLoopTests.FindButton("HarvestYield").onClick.Invoke();
            ProgressionLoopTests.FindButton("TrainingSpeed").onClick.Invoke();
            ProgressionLoopTests.FindButton("TrainingReward").onClick.Invoke();
            Assert.That(player.CarrotSeconds(world.Settings), Is.EqualTo(18));
            Assert.That(player.TrainingSeconds(world.Settings), Is.EqualTo(13.5f));
            Assert.That(player.TrainingCoins(world.Settings), Is.EqualTo(41));
            Assert.That(player.ClickGrowth, Is.EqualTo(0.12f).Within(0.0001)); Assert.That(player.HarvestYield, Is.EqualTo(3));
            Assert.That(ProgressionSession.Player, Is.SameAs(player)); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest]
        public IEnumerator KGrantsOncePerPressAndCannotEnableInactiveCrafting()
        {
            GameplayWorld world = Object.FindAnyObjectByType<GameplayWorld>(); PlayerProgression player = world.Player;
            _keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.K)); yield return null;
            Assert.That(player.Coins, Is.EqualTo(100000)); Assert.That(player.RaceTokens, Is.EqualTo(1000));
            yield return new WaitForSeconds(0.2f); Assert.That(player.Coins, Is.EqualTo(100000));
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState()); yield return null;
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.K)); yield return null;
            Assert.That(player.Coins, Is.EqualTo(200000)); Assert.That(player.RaceTokens, Is.EqualTo(2000));
            Assert.That(player.TryUnlock(UpgradeNode.Blacksmith), Is.False);
            Assert.That(player.TryUnlock(UpgradeNode.ForgeSpeed), Is.False);
            Assert.That(player.TryUpgradeProduction(ProductionUpgrade.FarmCapacity, world.Settings), Is.False);
            Assert.That(player.TryStartCraft(EquipmentType.Saddle), Is.False);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
