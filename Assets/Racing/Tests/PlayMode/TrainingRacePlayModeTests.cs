using System.Collections;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Horsecremental.Racing.Tests
{
    public sealed class TrainingRacePlayModeTests
    {
        private GameplayWorld _world;
        private bool _background;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _background = Application.runInBackground; Application.runInBackground = true;
            ProgressionSession.Reset();
            yield return SceneManager.LoadSceneAsync(GameScenes.WORLD);
            yield return new WaitForSeconds(0.4f);
            _world = Object.FindAnyObjectByType<GameplayWorld>();
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1f; Application.runInBackground = _background; yield return null;
        }
        [UnityTest]
        public IEnumerator SelectionWinRewardsUnlockUpgradeAndTierTwoUseTheRealRace()
        {
            PlayerProgression player = _world.Player;
            var handle = SceneManager.GetActiveScene().handle;
            RaceManager originalRace = _world.Race;
            int objects = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Length;
            ProgressionLoopTests.FindButton("View Training").onClick.Invoke(); yield return new WaitForSeconds(0.4f);
            Assert.That(_world.Area, Is.EqualTo(ManagementArea.Training));
            Assert.That(ProgressionLoopTests.FindButton("Start Training 1").interactable, Is.True);
            Assert.That(ProgressionLoopTests.FindButton("Start Training 2").interactable, Is.False);
            Assert.That(_world.StartTrainingRace(1), Is.False);
            Capture("selection", 1920, 1080); Capture("selection-4x3", 1024, 768);
            ProgressionLoopTests.FindButton("Start Training 1").onClick.Invoke();
            Assert.That(_world.IsTrainingRace, Is.True);
            Assert.That(_world.Race.Participants[3].Movement.Parameters.Speed, Is.EqualTo(10.6f));
            Time.timeScale = 4f;
            yield return ProgressionLoopTests.DriveRace(_world.Race);
            Time.timeScale = 1f;
            Assert.That(_world.Race.Player.FinishPosition, Is.EqualTo(1));
            Assert.That(player.Coins, Is.EqualTo(100)); Assert.That(player.TrainingTokens, Is.EqualTo(6));
            Assert.That(player.IsTrainingUnlocked(1), Is.True); Assert.That(player.IsTrainingUnlocked(2), Is.False);
            Assert.That(player.TrainingBestPlace(0), Is.EqualTo(1)); Assert.That(player.RaceTokens, Is.Zero);
            Assert.That(Object.FindAnyObjectByType<StableWorldInteraction>().CanInteract, Is.False);
            Capture("training-one-result", 1920, 1080);
            yield return new WaitForSeconds(0.2f); Assert.That(player.TrainingTokens, Is.EqualTo(6));
            ProgressionLoopTests.FindButton("Return to Stable").onClick.Invoke();
            yield return new WaitForSeconds(0.4f);
            float speed = player.Stats.Speed;
            ProgressionLoopTests.FindButton("Train Speed").onClick.Invoke();
            Assert.That(player.TrainingTokens, Is.EqualTo(3)); Assert.That(player.Coins, Is.EqualTo(100));
            Assert.That(player.Stats.Speed, Is.EqualTo(speed + 0.75f));
            Assert.That(GameObject.Find("Training Tokens").GetComponent<Text>().text, Is.EqualTo("3"));
            Capture("horse-upgraded", 1920, 1080);
            _world.ShowTraining(); yield return new WaitForSeconds(0.4f);
            Assert.That(ProgressionLoopTests.FindButton("Start Training 2").interactable, Is.True);
            Capture("training-two-unlocked", 1920, 1080);
            ProgressionLoopTests.FindButton("Start Training 2").onClick.Invoke();
            Assert.That(_world.Race.Player.Movement.Parameters.Speed, Is.EqualTo(player.Stats.Speed));
            Assert.That(_world.Race.Participants[3].Movement.Parameters.Speed, Is.EqualTo(12f));
            Time.timeScale = 4f; yield return ProgressionLoopTests.DriveRace(_world.Race); Time.timeScale = 1f;
            int place = _world.Race.Player.FinishPosition;
            Assert.That(player.Coins, Is.EqualTo(100 + player.TrainingRules(1).CoinsForPlace(place)));
            Assert.That(player.TrainingTokens, Is.EqualTo(3 + player.TrainingRules(1).TokensForPlace(place)));
            Assert.That(player.IsTrainingUnlocked(2), Is.EqualTo(place == 1));
            _world.ShowStable(); yield return new WaitForSeconds(0.4f);
            Assert.That(_world.Race, Is.SameAs(originalRace)); Assert.That(SceneManager.GetActiveScene().handle, Is.EqualTo(handle));
            Assert.That(Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Length, Is.EqualTo(objects));
            Assert.That(_world.Race.Participants[3].Movement.Parameters.Speed, Is.EqualTo(25f), "Main Race rivals must be restored.");
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest]
        public IEnumerator UnassistedTrainingLossPaysAndStableHasNoPassiveIncome()
        {
            PlayerProgression player = _world.Player;
            player.AdvanceTraining(300, _world.Settings); Assert.That(player.Coins, Is.Zero);
            Assert.That(GameObject.Find("TrainingHorseCount"), Is.Null);
            Assert.That(GameObject.Find("Physical Training Track"), Is.Null);
            _world.ShowTraining(); _world.StartTrainingRace(0);
            Time.timeScale = 4f;
            float timeout = Time.realtimeSinceStartup + 15f;
            while (!_world.Race.Player.HasFinished && Time.realtimeSinceStartup < timeout) yield return new WaitForFixedUpdate();
            Time.timeScale = 1f;
            Assert.That(_world.Race.Player.HasFinished, Is.True);
            Assert.That(_world.Race.Player.FinishPosition, Is.GreaterThan(1));
            Assert.That(player.IsTrainingUnlocked(1), Is.False);
            int place = _world.Race.Player.FinishPosition;
            Assert.That(player.TrainingTokens, Is.EqualTo(player.TrainingRules(0).TokensForPlace(place)));
            Assert.That(player.Coins, Is.EqualTo(player.TrainingRules(0).CoinsForPlace(place)));
            _world.ShowStable(); yield return new WaitForSeconds(0.4f);
            int coins = player.Coins; yield return new WaitForSeconds(0.5f); Assert.That(player.Coins, Is.EqualTo(coins));
            LogAssert.NoUnexpectedReceived();
        }
        private static void Capture(string name, int width, int height) =>
            ProgressionLoopTests.Capture(name, width, height, null, "Tools/TrainingRaceValidation/Screenshots");
    }
}
