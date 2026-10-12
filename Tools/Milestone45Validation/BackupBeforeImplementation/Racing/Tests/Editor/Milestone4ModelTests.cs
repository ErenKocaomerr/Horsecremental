using System;
using NUnit.Framework;
using UnityEditor;

namespace Horsecremental.Racing.Tests
{
    public sealed class Milestone4ModelTests
    {
        private static ProgressionSettings Progression => AssetDatabase.LoadAssetAtPath<ProgressionSettings>("Assets/Racing/Config/Progression.asset");
        private static WorldSettings World => AssetDatabase.LoadAssetAtPath<WorldSettings>("Assets/Racing/Config/World.asset");
        private static PlayerProgression Create() => UpgradeTreeTestSupport.UnlockAll(Progression.CreatePlayer());
        private static void Fund(PlayerProgression player, int coins = 10000, int metal = 500)
            => player.TryAwardRace(player.BeginRace(), coins, metal);

        [Test]
        public void MetalRewardsShareRaceTokenAndNeverOverflow()
        {
            PlayerProgression player = Create();
            long token = player.BeginRace();
            player.AdvanceTraining(37, World);
            Assert.That(player.Metal, Is.EqualTo(4));
            Assert.That(player.Coins, Is.EqualTo(70));
            Assert.That(player.TryAwardRace(token, 500, 15), Is.True);
            Assert.That(player.TryAwardRace(token, 500, 15), Is.False);
            Assert.That(player.Metal, Is.EqualTo(19));
            long stale = player.BeginRace(); player.BeginRace();
            Assert.That(player.TryAwardRace(stale, 100, 20), Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(() => player.TryAwardRace(token, 1, -1));
            Fund(player, int.MaxValue, int.MaxValue); Fund(player);
            Assert.That(player.Coins, Is.EqualTo(int.MaxValue)); Assert.That(player.Metal, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void CraftPaymentIsAtomicOneSlotIsReservedAndCompletionPaysOnce()
        {
            PlayerProgression player = Create(); Fund(player, 100, 4);
            Assert.That(player.TryStartCraft(EquipmentType.Horseshoes), Is.False);
            Assert.That(player.Coins, Is.EqualTo(100)); Assert.That(player.Metal, Is.EqualTo(4));
            Fund(player, 0, 1);
            int completions = 0; player.CraftCompleted += type => completions++;
            Assert.That(player.TryStartCraft(EquipmentType.Horseshoes), Is.True);
            Assert.That(player.Coins, Is.Zero); Assert.That(player.Metal, Is.Zero);
            Assert.That(player.TryStartCraft(EquipmentType.Saddle), Is.False);
            player.AdvanceCraft(4); Assert.That(player.EquipmentLevel(EquipmentType.Horseshoes), Is.Zero);
            Assert.That(player.CraftRemaining, Is.EqualTo(1));
            player.AdvanceCraft(100); player.AdvanceCraft(100);
            Assert.That(completions, Is.EqualTo(1)); Assert.That(player.IsCrafting, Is.False);
            Assert.That(player.EquipmentLevel(EquipmentType.Horseshoes), Is.EqualTo(1));
        }

        [Test]
        public void TierReplacementCombinesAllStatSourcesAndChangesRealSimulation()
        {
            PlayerProgression player = Create(); Fund(player);
            HorseParameters before = player.Stats;
            player.AdvanceFarm(100, World); player.CollectCarrots(); player.TryFeed(World);
            player.TryUpgrade(HorseUpgrade.Speed);
            for (int i = 0; i < 2; i++) { player.TryStartCraft(EquipmentType.Horseshoes); player.AdvanceCraft(20); }
            player.TryStartCraft(EquipmentType.Saddle); player.AdvanceCraft(20);
            player.TryStartCraft(EquipmentType.Bridle); player.AdvanceCraft(20);
            StatBreakdown speed = player.Breakdown(HorseUpgrade.Speed);
            Assert.That(speed.Base, Is.EqualTo(10)); Assert.That(speed.Upgrades, Is.EqualTo(0.75f));
            Assert.That(speed.Food, Is.EqualTo(0.1f)); Assert.That(speed.Equipment, Is.EqualTo(3));
            Assert.That(player.Stats.Speed, Is.EqualTo(13.85f).Within(0.001f));
            Assert.That(player.Stats.Acceleration, Is.EqualTo(15)); Assert.That(player.Stats.BoostPower, Is.EqualTo(6.5f));
            Assert.That(player.Stats.BoostHighDrain, Is.EqualTo(before.BoostHighDrain));
            Assert.That(player.Stats.JumpPower, Is.EqualTo(before.JumpPower));
            Assert.That(player.HorsePower, Is.GreaterThan(108));
            HorseSimulation baseline = new HorseSimulation(before, Array.Empty<RaceObstacle>());
            HorseSimulation equipped = new HorseSimulation(player.Stats, Array.Empty<RaceObstacle>());
            baseline.Start(); equipped.Start();
            for (int i = 0; i < 250; i++) { baseline.Tick(0.02f); equipped.Tick(0.02f); }
            Assert.That(equipped.Distance, Is.GreaterThan(baseline.Distance));
            baseline.TryBoost(); equipped.TryBoost(); Assert.That(equipped.BoostAmount, Is.GreaterThan(baseline.BoostAmount));
        }

        [Test]
        public void ProductionUpgradesHaveCapsAndChangeActualRatesWithoutFreeCrafts()
        {
            PlayerProgression player = Create(); Fund(player, 100000, 500);
            Assert.That(player.ProductionRule(ProductionUpgrade.ForgeSpeed).Cost(1), Is.EqualTo(240));
            Assert.That(player.ProductionRule(ProductionUpgrade.ForgeSpeed).Cost(2), Is.EqualTo(384));
            int[] rewards = { 41, 46, 51, 56, 62 };
            for (int i = 0; i < rewards.Length; i++)
            {
                player.TryUpgradeProduction(ProductionUpgrade.TrainingReward, World);
                Assert.That(player.TrainingCoins(World), Is.EqualTo(rewards[i]));
            }
            player.AdvanceTraining(7.5f, World); player.AdvanceFarm(10, World);
            Assert.That(player.TryUpgradeProduction(ProductionUpgrade.TrainingSpeed, World), Is.True);
            Assert.That(player.TrainingElapsed / player.TrainingSeconds(World), Is.EqualTo(0.5d).Within(0.00001));
            Assert.That(player.TryUpgradeProduction(ProductionUpgrade.GrowSpeed, World), Is.True);
            Assert.That(player.FarmElapsed / player.CarrotSeconds(World), Is.EqualTo(0.5d).Within(0.00001));
            foreach (ProductionUpgrade upgrade in Enum.GetValues(typeof(ProductionUpgrade)))
                while (player.TryUpgradeProduction(upgrade, World)) { }
            Assert.That(player.TrainingSeconds(World), Is.EqualTo(7.5f));
            Assert.That(player.CarrotSeconds(World), Is.EqualTo(10)); Assert.That(player.FarmCapacity(World), Is.EqualTo(35));
            Assert.That(player.TrainingCoins(World), Is.EqualTo(62));
            Assert.That(player.CraftSeconds(EquipmentType.Horseshoes), Is.EqualTo(2.5f));
            Assert.That(player.CraftMetalCost(EquipmentType.Horseshoes), Is.EqualTo(4));
            int coins = player.Coins; Assert.That(player.TryUpgradeProduction(ProductionUpgrade.ForgeSpeed, World), Is.False);
            Assert.That(player.Coins, Is.EqualTo(coins));
            player.AdvanceFarm(1000, World); Assert.That(player.StoredCarrots, Is.EqualTo(35));
            Assert.That(player.FarmElapsed, Is.Zero); player.CollectCarrots(); player.AdvanceFarm(10, World);
            Assert.That(player.StoredCarrots, Is.EqualTo(1));
        }

        [Test]
        public void ActiveCraftKeepsItsOriginalDurationAndEverySlotStopsAtTierFour()
        {
            PlayerProgression player = Create(); Fund(player, 100000, 1000);
            Assert.That(player.TryStartCraft(EquipmentType.Horseshoes), Is.True); player.AdvanceCraft(1);
            player.TryUpgradeProduction(ProductionUpgrade.ForgeSpeed, World);
            player.TryUpgradeProduction(ProductionUpgrade.MetalEfficiency, World);
            Assert.That(player.CraftDuration, Is.EqualTo(5)); Assert.That(player.CraftElapsed, Is.EqualTo(1));
            player.AdvanceCraft(4);
            foreach (EquipmentType type in Enum.GetValues(typeof(EquipmentType)))
            {
                while (player.HasNextTier(type)) { Assert.That(player.TryStartCraft(type), Is.True); player.AdvanceCraft(20); }
                Assert.That(player.EquipmentLevel(type), Is.EqualTo(4));
                Assert.That(player.TryStartCraft(type), Is.False);
            }
            int coins = player.Coins; int metal = player.Metal;
            player.AdvanceCraft(1000); Assert.That(player.Coins, Is.EqualTo(coins)); Assert.That(player.Metal, Is.EqualTo(metal));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DiverseTrainingFundedPathsLoseFreshAndWinWithoutMaxEquipment(bool productionFirst)
        {
            RaceSettings race = AssetDatabase.LoadAssetAtPath<RaceSettings>("Assets/Racing/Config/PrototypeRace.asset");
            PlayerProgression player = Create();
            float rival = FinishTime(race.OpponentStats[2].Parameters, race, false);
            float fresh = FinishTime(player.Stats, race, true);
            Assert.That(fresh, Is.GreaterThan(FinishTime(race.OpponentStats[0].Parameters, race, false) * 1.3f));
            int winningMinute = 0;
            for (int minute = 1; minute <= 30; minute++)
            {
                player.AdvanceTraining(60, World); player.AdvanceFarm(60, World); player.AdvanceCraft(60);
                player.CollectCarrots(); while (player.TryFeed(World)) { }
                if (productionFirst)
                {
                    BuyUpTo(player, ProductionUpgrade.TrainingSpeed, 2);
                    BuyUpTo(player, ProductionUpgrade.TrainingReward, 2);
                }
                BuyUpTo(player, ProductionUpgrade.GrowSpeed, 1); BuyUpTo(player, ProductionUpgrade.FarmCapacity, 1);
                BuyUpTo(player, ProductionUpgrade.ForgeSpeed, 1); BuyUpTo(player, ProductionUpgrade.MetalEfficiency, 1);
                foreach (EquipmentType type in Enum.GetValues(typeof(EquipmentType)))
                {
                    if (player.EquipmentLevel(type) >= (productionFirst ? 2 : 3)) continue;
                    if (player.TryStartCraft(type)) player.AdvanceCraft(60);
                }
                player.TryUpgrade(HorseUpgrade.Speed); player.TryUpgrade(HorseUpgrade.BoostPower);
                if (minute % 3 == 0) player.TryUpgrade(HorseUpgrade.Acceleration);
                if (!productionFirst)
                { BuyUpTo(player, ProductionUpgrade.TrainingSpeed, 1); BuyUpTo(player, ProductionUpgrade.TrainingReward, 1); }
                float time = FinishTime(player.Stats, race, true);
                if (time < rival) { winningMinute = minute; break; }
            }
            TestContext.WriteLine("Path productionFirst=" + productionFirst + ": fresh " + fresh + "s; rival " + rival +
                "s; win minute " + winningMinute + "; stats " + player.Stats.Speed + "/" + player.Stats.Acceleration + "/" + player.Stats.BoostPower +
                "; tiers " + player.EquipmentLevel(EquipmentType.Horseshoes) + "/" + player.EquipmentLevel(EquipmentType.Saddle) + "/" + player.EquipmentLevel(EquipmentType.Bridle));
            Assert.That(winningMinute, Is.InRange(15, 30));
            Assert.That(player.EquipmentLevel(EquipmentType.Horseshoes), Is.LessThan(4));
        }
        private static void BuyUpTo(PlayerProgression player, ProductionUpgrade upgrade, int level)
        { if (player.ProductionLevel(upgrade) < level) player.TryUpgradeProduction(upgrade, World); }
        private static float FinishTime(HorseParameters parameters, RaceSettings settings, bool boost)
        {
            HorseSimulation horse = new HorseSimulation(parameters, settings.CreateObstacles()); horse.Start();
            int ticks = 0;
            while (horse.Distance < settings.TrackLength && ticks < 6000)
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
