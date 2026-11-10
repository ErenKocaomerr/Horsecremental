using System;
using NUnit.Framework;
using UnityEditor;

namespace Horsecremental.Racing.Tests
{
    public sealed class Milestone45ModelTests
    {
        private static WorldSettings World => AssetDatabase.LoadAssetAtPath<WorldSettings>("Assets/Racing/Config/World.asset");
        private static StablePlaygroundSettings Playground => AssetDatabase.LoadAssetAtPath<StablePlaygroundSettings>("Assets/Racing/Config/StablePlayground.asset");
        private static ProgressionSettings Settings => AssetDatabase.LoadAssetAtPath<ProgressionSettings>("Assets/Racing/Config/Progression.asset");
        private static PlayerProgression Create()
        { PlayerProgression player = Settings.CreatePlayer(); player.EnablePlayground(Playground, World); return player; }
        [Test]
        public void FreshWorldEnablesFarmAndPaysOnlyCoinsAtActualLapWraps()
        {
            PlayerProgression player = Create(); long claim = player.BeginRace(); int wraps = 0;
            player.TrainingLapCompleted += (horse, coins) => { wraps++; Assert.That(coins, Is.EqualTo(35)); };
            Assert.That(player.TrainingHorseCount, Is.EqualTo(1)); Assert.That(player.Crops.Count, Is.EqualTo(6));
            Assert.That(player.IsUnlocked(UpgradeNode.Farm), Is.True);
            player.AdvanceTraining(14, World); Assert.That(player.Coins, Is.Zero);
            player.AdvanceTraining(1, World); Assert.That(player.Coins, Is.EqualTo(35)); Assert.That(wraps, Is.EqualTo(1));
            player.AdvanceTraining(32, World); Assert.That(player.Coins, Is.EqualTo(105)); Assert.That(wraps, Is.EqualTo(3));
            Assert.That(player.Training.Phase(0), Is.EqualTo(2d / 15d).Within(0.00001)); Assert.That(player.Metal, Is.Zero);
            Assert.That(player.TryAwardRace(claim, 100, 3, 1), Is.True); Assert.That(player.TryAwardRace(claim, 100), Is.False);
        }
        [Test]
        public void AddingHorsesPreservesPositionsSeparatesPhasesAndCapsAtFour()
        {
            PlayerProgression player = Create(); player.AdvanceTraining(3, World); player.GrantDebugResources(4000, 0);
            int[] costs = { 500, 900, 1620 };
            for (int count = 0; count < costs.Length; count++)
            {
                double phase = player.Training.Phase(0); int coins = player.Coins;
                Assert.That(player.ProductionCost(ProductionUpgrade.TrainingHorseCount), Is.EqualTo(costs[count]));
                Assert.That(player.TryUpgradeProduction(ProductionUpgrade.TrainingHorseCount, World), Is.True);
                Assert.That(player.Coins, Is.EqualTo(coins - costs[count]));
                Assert.That(player.Training.Phase(0), Is.EqualTo(phase));
            }
            Assert.That(player.TrainingHorseCount, Is.EqualTo(4));
            for (int i = 0; i < 4; i++) for (int j = i + 1; j < 4; j++)
                Assert.That(Math.Abs(player.Training.Phase(i) - player.Training.Phase(j)), Is.GreaterThan(0.2));
            Assert.That(player.TryUpgradeProduction(ProductionUpgrade.TrainingHorseCount, World), Is.False);
            int before = player.Coins; player.AdvanceTraining(15, World); Assert.That(player.Coins, Is.EqualTo(before + 140));
        }
        [Test]
        public void SpeedAndRewardChangeRatesWithoutResettingAnyPhase()
        {
            PlayerProgression player = Create(); player.GrantDebugResources(20000, 0);
            player.TryUnlock(UpgradeNode.Acceleration); player.TryUnlock(UpgradeNode.TrainingSpeed); player.TryUnlock(UpgradeNode.TrainingReward);
            player.AdvanceTraining(7.5f, World); double phase = player.Training.Phase(0);
            int[] rewards = { 41, 46, 51, 56, 62 };
            for (int i = 0; i < 5; i++)
            {
                Assert.That(player.TryUpgradeProduction(ProductionUpgrade.TrainingSpeed, World), Is.True);
                Assert.That(player.TryUpgradeProduction(ProductionUpgrade.TrainingReward, World), Is.True);
                Assert.That(player.Training.Phase(0), Is.EqualTo(phase));
                Assert.That(player.TrainingSeconds(World), Is.EqualTo(15f * (1 - 0.1f * (i + 1))).Within(0.001));
                Assert.That(player.TrainingCoins(World), Is.EqualTo(rewards[i]));
            }
            int before = player.Coins; player.AdvanceTraining(3.75f, World); Assert.That(player.Coins, Is.EqualTo(before + 62));
        }
        [Test]
        public void IndependentCropsClampReadyWaitForHarvestAndNeverCreditAutomatically()
        {
            PlayerProgression player = Create();
            Assert.That(player.Crops.Progress(0), Is.Zero); Assert.That(player.Crops.Progress(5), Is.EqualTo(0.45).Within(0.0001));
            player.Crops.Grow(0, Playground.MidThreshold); Assert.That(player.Crops.Stage(0), Is.EqualTo(CropStage.Mid));
            player.Crops.Grow(0, Playground.LargeThreshold - (double)Playground.MidThreshold); Assert.That(player.Crops.Stage(0), Is.EqualTo(CropStage.Large));
            player.Crops.Grow(0, 1d - Playground.LargeThreshold); Assert.That(player.Crops.Stage(0), Is.EqualTo(CropStage.Ready));
            player.AdvanceFarm(10000, World); Assert.That(player.Carrots, Is.Zero); Assert.That(player.StoredCarrots, Is.Zero);
            Assert.That(player.CollectCarrots(), Is.Zero); Assert.That(player.Crops.Progress(0), Is.EqualTo(1));
            int yield = 0; player.CropHarvested += (plot, amount) => yield += amount;
            Assert.That(player.HarvestCrop(0), Is.True); Assert.That(yield, Is.EqualTo(2));
            Assert.That(player.Crops.Stage(0), Is.EqualTo(CropStage.Sprout)); Assert.That(player.Carrots, Is.Zero);
            Assert.That(player.HarvestCrop(0), Is.False); Assert.That(player.Crops.Stage(1), Is.EqualTo(CropStage.Ready));
            player.AdvanceFarm(1, World); Assert.That(player.Crops.Progress(0), Is.EqualTo(0.05).Within(0.00001));
        }
        [Test]
        public void ClickYieldAndAutoGrowthUpgradesAffectActualFarmAndPreserveProgress()
        {
            PlayerProgression player = Create(); player.GrantDebugResources(10000, 0); player.TryUnlock(UpgradeNode.GrowSpeed);
            player.GrowCrop(0); Assert.That(player.Crops.Progress(0), Is.EqualTo(0.1).Within(0.0001));
            player.TryUpgradeProduction(ProductionUpgrade.ClickGrowthPower, World);
            player.TryUpgradeProduction(ProductionUpgrade.HarvestYield, World);
            player.TryUpgradeProduction(ProductionUpgrade.GrowSpeed, World);
            Assert.That(player.Crops.Progress(0), Is.EqualTo(0.1).Within(0.0001));
            Assert.That(player.ClickGrowth, Is.EqualTo(0.12f).Within(0.0001)); Assert.That(player.HarvestYield, Is.EqualTo(3));
            Assert.That(player.CarrotSeconds(World), Is.EqualTo(18));
            player.GrowCrop(0); player.AdvanceFarm(1.8f, World);
            Assert.That(player.Crops.Progress(0), Is.EqualTo(0.32).Within(0.0001));
            Assert.That(player.CreditPickup(PickupResource.Carrots, 5), Is.EqualTo(5)); Assert.That(player.TryFeed(World), Is.True);
            Assert.That(player.Stats.Speed, Is.EqualTo(10.1f).Within(0.0001));
        }
        [Test]
        public void InactiveCraftingCannotSpendButExistingEquipmentAndFarmStorageMigrate()
        {
            PlayerProgression player = UpgradeTreeTestSupport.UnlockAll(Settings.CreatePlayer()); player.TryAwardRace(player.BeginRace(), 10000, 100);
            player.TryStartCraft(EquipmentType.Horseshoes); player.AdvanceCraft(100); player.AdvanceFarm(100, World); player.AdvanceTraining(3, World);
            float speed = player.Stats.Speed; player.EnablePlayground(Playground, World);
            Assert.That(player.Stats.Speed, Is.EqualTo(speed)); Assert.That(player.Training.Phase(0), Is.EqualTo(0.2).Within(0.0001));
            Assert.That(player.TakeLegacyStoredCarrots(), Is.EqualTo(5)); Assert.That(player.TakeLegacyStoredCarrots(), Is.Zero);
            int before = player.Coins; Assert.That(player.TryStartCraft(EquipmentType.Saddle), Is.False);
            Assert.That(player.TryUpgradeProduction(ProductionUpgrade.FarmCapacity, World), Is.False);
            Assert.That(player.TryUpgradeProduction(ProductionUpgrade.ForgeSpeed, World), Is.False);
            Assert.That(player.Coins, Is.EqualTo(before));
        }
        [Test]
        public void FullWalletCreditsOnlyAvailableRoomAndInvalidTimeCannotCorruptState()
        {
            PlayerProgression player = Create(); Assert.That(player.CreditPickup(PickupResource.Carrots, int.MaxValue - 2L), Is.EqualTo(int.MaxValue - 2));
            Assert.That(player.CreditPickup(PickupResource.Carrots, 10), Is.EqualTo(2)); Assert.That(player.CreditPickup(PickupResource.Carrots, 1), Is.Zero);
            player.AdvanceTraining(float.NaN, World); player.AdvanceTraining(float.PositiveInfinity, World);
            player.AdvanceFarm(float.NaN, World); player.AdvanceFarm(-1, World);
            Assert.That(player.Training.Phase(0), Is.Zero); Assert.That(player.Crops.Progress(0), Is.Zero);
        }
        [TestCase(false)] [TestCase(true)]
        public void MeasuredProgressionCanWinWithoutAnyEquipment(bool activeFarm)
        {
            PlayerProgression player = Create(); RaceSettings race = AssetDatabase.LoadAssetAtPath<RaceSettings>("Assets/Racing/Config/PrototypeRace.asset");
            float fresh = FinishTime(player.Stats, race, true); float rival = FinishTime(race.OpponentStats[2].Parameters, race, false);
            int winSeconds = 0;
            for (int second = 1; second <= 1800; second++)
            {
                player.AdvanceTraining(1, World); player.AdvanceFarm(1, World);
                if (activeFarm)
                {
                    for (int plot = 0; plot < player.Crops.Count; plot++)
                    {
                        player.GrowCrop(plot);
                        if (player.HarvestCrop(plot)) player.CreditPickup(PickupResource.Carrots, player.HarvestYield);
                    }
                    while (player.TryFeed(World)) { }
                }
                player.TryUnlock(UpgradeNode.Acceleration); player.TryUnlock(UpgradeNode.BoostPower);
                player.TryUnlock(UpgradeNode.TrainingSpeed); player.TryUnlock(UpgradeNode.TrainingReward);
                if (!activeFarm && player.TrainingHorseCount < 4)
                { player.TryUpgradeProduction(ProductionUpgrade.TrainingHorseCount, World); continue; }
                if (!activeFarm && player.ProductionLevel(ProductionUpgrade.TrainingSpeed) < 3)
                { player.TryUpgradeProduction(ProductionUpgrade.TrainingSpeed, World); continue; }
                if (!activeFarm && player.ProductionLevel(ProductionUpgrade.TrainingReward) < 3)
                { player.TryUpgradeProduction(ProductionUpgrade.TrainingReward, World); continue; }
                player.TryUpgrade(HorseUpgrade.Speed);
                if (second % 3 == 0) player.TryUpgrade(HorseUpgrade.BoostPower);
                if (second % 10 == 0 && player.Level(HorseUpgrade.Acceleration) < 4) player.TryUpgrade(HorseUpgrade.Acceleration);
                if (second % 30 != 0) continue;
                if (FinishTime(player.Stats, race, true) < rival) { winSeconds = second; break; }
            }
            TestContext.WriteLine("ActiveFarm=" + activeFarm + "; fresh=" + fresh + "s; fastest rival=" + rival +
                "s; winning progress seconds=" + winSeconds + "; stats=" + player.Stats.Speed + "/" + player.Stats.Acceleration + "/" + player.Stats.BoostPower);
            Assert.That(fresh, Is.GreaterThan(rival)); Assert.That(winSeconds, Is.InRange(30, 1800));
            Assert.That(player.EquipmentLevel(EquipmentType.Horseshoes), Is.Zero);
        }
        private static float FinishTime(HorseParameters parameters, RaceSettings race, bool boost)
        {
            HorseSimulation horse = new HorseSimulation(parameters, race.CreateObstacles()); horse.Start(); int ticks = 0;
            while (horse.Distance < race.TrackLength && ticks < 6000)
            {
                if (boost && ticks % 25 == 0) horse.TryBoost();
                float distance = horse.NextObstacleDistance - horse.Distance;
                if (distance > 0 && distance <= horse.Speed * 0.28f) horse.TryJump();
                horse.Tick(0.02f); ticks++;
            }
            Assert.That(horse.ObstacleHits, Is.Zero); return ticks * 0.02f;
        }
    }
}
