using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Horsecremental.Racing.Tests
{
    public sealed class Milestone4PlayModeTests
    {
        [UnityTest]
        public IEnumerator PhysicalHorsesCropsAndPooledDropsFollowTheSharedModel()
        {
            ProgressionSession.Reset(); Application.runInBackground = true;
            yield return SceneManager.LoadSceneAsync(GameScenes.WORLD); yield return null;
            GameplayWorld world = Object.FindAnyObjectByType<GameplayWorld>(); PlayerProgression player = world.Player;
            FarmWorldView farm = Object.FindAnyObjectByType<FarmWorldView>();
            WorldPickupManager pickups = Object.FindAnyObjectByType<WorldPickupManager>();
            Assert.That(Object.FindAnyObjectByType<TrainingTrackView>(), Is.Null);
            player.GrantDebugResources(5000, 0);
            Assert.That(player.TryUpgradeProduction(ProductionUpgrade.TrainingHorseCount, world.Settings), Is.False);
            player.AdvanceTraining(60f, world.Settings); Assert.That(player.Coins, Is.EqualTo(5000));
            player.AdvanceFarm(30, world.Settings); farm.Click(0);
            Assert.That(player.Crops.Stage(0), Is.EqualTo(CropStage.Sprout));
            Assert.That(player.Carrots, Is.Zero); Assert.That(pickups.PendingValue, Is.EqualTo(2));
            farm.Click(0); Assert.That(player.Crops.Progress(0), Is.Zero, "Same-frame click must be throttled.");
            yield return new WaitForSeconds(0.55f);
            Assert.That(pickups.IsSettled(0), Is.True); Assert.That(player.Carrots, Is.Zero);
            pickups.Spawn(new Vector3(-30, -3, 0), 100, PickupResource.Carrots);
            Assert.That(pickups.ActiveCount, Is.EqualTo(80)); Assert.That(pickups.PendingValue, Is.EqualTo(102));
            yield return new WaitForSeconds(0.6f);
            for (int i = 0; i < 80; i++) if (pickups.IsSettled(i)) pickups.Sweep(pickups.ItemPosition(i), pickups.ItemPosition(i));
            Assert.That(player.Carrots, Is.Zero, "Credit waits until collection animation completes.");
            yield return new WaitForSeconds(0.3f);
            Assert.That(player.Carrots, Is.EqualTo(102)); Assert.That(pickups.PendingValue, Is.Zero); Assert.That(pickups.ActiveCount, Is.Zero);
            ProgressionLoopTests.FindButton("Feed Horse").onClick.Invoke(); Assert.That(player.Carrots, Is.EqualTo(97));
            Assert.That(player.Stats.Speed, Is.EqualTo(10.1f).Within(0.001));
            world.ShowBlacksmith(); Assert.That(world.Area, Is.EqualTo(ManagementArea.Stable));
            Assert.That(player.TryStartCraft(EquipmentType.Horseshoes), Is.False);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
