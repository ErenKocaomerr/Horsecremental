using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Horsecremental.Racing.Tests
{
    public sealed class Milestone4PlayModeTests
    {
        [UnityTest]
        public IEnumerator CraftContinuesAcrossAreasAndRaceWhileTrainingPausesAndFarmProduces()
        {
            bool background = Application.runInBackground; Application.runInBackground = true;
            ProgressionSession.Reset();
            yield return SceneManager.LoadSceneAsync(GameScenes.WORLD); yield return null;
            GameplayWorld world = Object.FindAnyObjectByType<GameplayWorld>(); PlayerProgression player = world.Player;
            ProgressionLoopTests.UnlockAll(player);
            try
            {
                player.TryAwardRace(player.BeginRace(), 1000, 50);
                world.ShowBlacksmith();
                yield return new WaitForSeconds(0.8f);
                ProgressionLoopTests.FindButton("Craft Saddle").onClick.Invoke();
                Assert.That(player.IsCrafting, Is.True);
                int coins = player.Coins; int metal = player.Metal;
                ProgressionLoopTests.FindButton("Craft Bridle").onClick.Invoke();
                Assert.That(GameObject.Find("Craft Feedback").GetComponent<Text>().text, Does.Contain("BUSY"));
                Assert.That(player.Coins, Is.EqualTo(coins)); Assert.That(player.Metal, Is.EqualTo(metal));
                Time.timeScale = 1f;
                yield return new WaitForSeconds(0.2f);
                ProgressionLoopTests.Capture("crafting", 1920, 1080);
                world.ShowFarm(); yield return new WaitForSeconds(0.8f);
                Assert.That(player.CraftElapsed, Is.GreaterThan(0.5d));
                Assert.That(GameObject.Find("Blacksmith UI").GetComponent<CanvasGroup>().blocksRaycasts, Is.False);
                player.AdvanceFarm(19f, world.Settings);
                world.ShowRaceArea(); world.StartMainRace();
                float raceAcceleration = world.Race.Player.Movement.Parameters.Acceleration;
                double training = player.TrainingElapsed; long number = player.TrainingRaceNumber;
                int storage = player.StoredCarrots;
                Time.timeScale = 10f;
                yield return ProgressionLoopTests.DriveRace(world.Race);
                Assert.That(player.IsCrafting, Is.False);
                Assert.That(player.EquipmentLevel(EquipmentType.Saddle), Is.EqualTo(1));
                Assert.That(player.Stats.Acceleration, Is.GreaterThan(raceAcceleration));
                Assert.That(world.Race.Player.Movement.Parameters.Acceleration, Is.EqualTo(raceAcceleration));
                Assert.That(player.StoredCarrots, Is.GreaterThan(storage));
                Assert.That(player.TrainingElapsed, Is.EqualTo(training)); Assert.That(player.TrainingRaceNumber, Is.EqualTo(number));
                Assert.That(GameObject.Find("Craft Complete Banner").GetComponent<Text>().text, Does.Contain("equipped!"));
                ProgressionLoopTests.Capture("craft-complete-during-race", 1920, 1080);
                world.ShowStable(); Time.timeScale = 1f; yield return new WaitForSeconds(0.8f);
                Assert.That(world.IsManagement, Is.True);
                Assert.That(player.TrainingElapsed, Is.GreaterThan(training));
                world.ShowRaceArea(); world.StartMainRace();
                Assert.That(world.Race.Player.Movement.Parameters.Acceleration, Is.EqualTo(player.Stats.Acceleration));
                LogAssert.NoUnexpectedReceived();
            }
            finally { Time.timeScale = 1f; Application.runInBackground = background; }
        }
    }
}
