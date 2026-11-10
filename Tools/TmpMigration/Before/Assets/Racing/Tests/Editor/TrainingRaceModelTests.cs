using System;
using NUnit.Framework;
using UnityEditor;

namespace Horsecremental.Racing.Tests
{
    public sealed class TrainingRaceModelTests
    {
        private static TrainingLadderSettings Ladder => AssetDatabase.LoadAssetAtPath<TrainingLadderSettings>("Assets/Racing/Config/TrainingLadder.asset");
        private static PlayerProgression CreatePlayer()
        {
            PlayerProgression player = AssetDatabase.LoadAssetAtPath<ProgressionSettings>("Assets/Racing/Config/Progression.asset").CreatePlayer();
            Ladder.Enable(player);
            return player;
        }
        [TestCase(2, 75, 4)]
        [TestCase(3, 50, 3)]
        [TestCase(4, 30, 1)]
        public void NonWinsPayBothCurrenciesWithoutUnlocking(int place, int coins, int tokens)
        {
            PlayerProgression player = CreatePlayer();
            Assert.That(player.IsTrainingUnlocked(0), Is.True);
            Assert.That(player.IsTrainingUnlocked(1), Is.False);
            Assert.That(player.IsTrainingUnlocked(2), Is.False);
            Assert.Throws<InvalidOperationException>(() => player.BeginTrainingRace(1));
            long race = player.BeginTrainingRace(0);
            Assert.That(player.TryAwardTrainingRace(race, 0, place), Is.True);
            Assert.That(player.Coins, Is.EqualTo(coins)); Assert.That(player.TrainingTokens, Is.EqualTo(tokens));
            Assert.That(player.TrainingBestPlace(0), Is.EqualTo(place)); Assert.That(player.IsTrainingUnlocked(1), Is.False);
            Assert.That(player.TryAwardTrainingRace(race, 0, place), Is.False);
            Assert.That(player.RaceTokens, Is.Zero); Assert.That(player.Metal, Is.Zero);
        }
        [Test]
        public void WinsUnlockOnlyTheNextTierBestPlaceNeverRegressesAndReplaysPay()
        {
            PlayerProgression player = CreatePlayer();
            Assert.That(player.TryAwardTrainingRace(player.BeginTrainingRace(0), 0, 1), Is.True);
            Assert.That(player.IsTrainingUnlocked(1), Is.True); Assert.That(player.IsTrainingUnlocked(2), Is.False);
            player.TryAwardTrainingRace(player.BeginTrainingRace(0), 0, 4);
            Assert.That(player.TrainingBestPlace(0), Is.EqualTo(1)); Assert.That(player.IsTrainingUnlocked(1), Is.True);
            player.TryAwardTrainingRace(player.BeginTrainingRace(1), 1, 1);
            Assert.That(player.IsTrainingUnlocked(2), Is.True);
            player.TryAwardTrainingRace(player.BeginTrainingRace(2), 2, 1);
            Assert.That(player.Coins, Is.EqualTo(530)); Assert.That(player.TrainingTokens, Is.EqualTo(33));
            Assert.That(player.TrainingBestPlace(2), Is.EqualTo(1));
        }
        [Test]
        public void ClaimsCannotCrossRaceKindsTiersOrAbandonedAttempts()
        {
            PlayerProgression player = CreatePlayer();
            long abandoned = player.BeginTrainingRace(0);
            long current = player.BeginTrainingRace(0);
            Assert.That(player.TryAwardTrainingRace(abandoned, 0, 1), Is.False);
            Assert.That(player.TryAwardTrainingRace(current, 1, 1), Is.False);
            Assert.That(player.TryAwardRace(current, 500, 0, 5), Is.False);
            Assert.That(player.TryAwardTrainingRace(current, 0, 1), Is.True);
            long main = player.BeginRace();
            Assert.That(player.TryAwardTrainingRace(main, 0, 1), Is.False);
            Assert.That(player.TryAwardRace(main, 500, 0, 5), Is.True);
            Assert.That(player.TrainingTokens, Is.EqualTo(6));
        }
        [Test]
        public void TokensTrainAllThreeRealStatsWithoutSpendingCoins()
        {
            PlayerProgression player = CreatePlayer();
            HorseParameters before = player.Stats;
            player.GrantDebugResources(100000, 1000);
            Assert.That(player.TryUpgrade(HorseUpgrade.Speed), Is.False, "Coins and legacy Race Tokens cannot train the horse.");
            player.TryAwardTrainingRace(player.BeginTrainingRace(0), 0, 1);
            player.TryAwardTrainingRace(player.BeginTrainingRace(0), 0, 1);
            int coins = player.Coins;
            foreach (HorseUpgrade upgrade in new[] { HorseUpgrade.Speed, HorseUpgrade.Acceleration, HorseUpgrade.BoostPower })
                Assert.That(player.TryUpgrade(upgrade), Is.True);
            Assert.That(player.Coins, Is.EqualTo(coins)); Assert.That(player.TrainingTokens, Is.EqualTo(4));
            Assert.That(player.Stats.Speed, Is.EqualTo(before.Speed + 0.75f));
            Assert.That(player.Stats.Acceleration, Is.EqualTo(before.Acceleration + 3f));
            Assert.That(player.Stats.BoostPower, Is.EqualTo(before.BoostPower + 1f));
            HorseSimulation baseline = new HorseSimulation(before, Array.Empty<RaceObstacle>());
            HorseSimulation trained = new HorseSimulation(player.Stats, Array.Empty<RaceObstacle>());
            baseline.Start(); trained.Start(); baseline.Tick(0.1f); trained.Tick(0.1f);
            Assert.That(trained.Speed, Is.GreaterThan(baseline.Speed));
            for (int i = 0; i < 200; i++) { baseline.Tick(0.02f); trained.Tick(0.02f); }
            Assert.That(trained.Distance, Is.GreaterThan(baseline.Distance));
            baseline.TryBoost(); trained.TryBoost(); Assert.That(trained.BoostAmount, Is.GreaterThan(baseline.BoostAmount));
        }
        [Test]
        public void PassiveIncomeAndPurchasesAreDisabledButFarmStillGrows()
        {
            PlayerProgression player = CreatePlayer();
            WorldSettings world = AssetDatabase.LoadAssetAtPath<WorldSettings>("Assets/Racing/Config/World.asset");
            player.EnablePlayground(AssetDatabase.LoadAssetAtPath<StablePlaygroundSettings>("Assets/Racing/Config/StablePlayground.asset"), world);
            player.GrantDebugResources(5000, 0);
            player.AdvanceTraining(300f, world);
            Assert.That(player.Coins, Is.EqualTo(5000)); Assert.That(player.Training.Phase(0), Is.Zero);
            foreach (ProductionUpgrade upgrade in new[] { ProductionUpgrade.TrainingHorseCount, ProductionUpgrade.TrainingSpeed, ProductionUpgrade.TrainingReward })
                Assert.That(player.TryUpgradeProduction(upgrade, world), Is.False);
            Assert.That(player.TryUnlock(UpgradeNode.TrainingSpeed), Is.False);
            player.AdvanceFarm(30f, world); Assert.That(player.Crops.Stage(0), Is.EqualTo(CropStage.Ready));
        }
        [Test]
        public void FixedRivalsAndRewardsIncreaseAcrossExactlyThreeShortTiers()
        {
            PlayerProgression player = CreatePlayer();
            Assert.That(Ladder.Count, Is.EqualTo(3));
            for (int tier = 0; tier < 3; tier++)
            {
                RaceSettings race = Ladder.Race(tier);
                Assert.That(race.TrackLength, Is.LessThan(420f));
                Assert.That(race.OpponentStats.Length, Is.EqualTo(3));
                if (tier == 0) continue;
                for (int rival = 0; rival < 3; rival++)
                    Assert.That(race.OpponentStats[rival].Parameters.Speed, Is.GreaterThan(Ladder.Race(tier - 1).OpponentStats[rival].Parameters.Speed));
                for (int place = 1; place <= 4; place++)
                {
                    Assert.That(player.TrainingRules(tier).CoinsForPlace(place), Is.GreaterThan(player.TrainingRules(tier - 1).CoinsForPlace(place)));
                    Assert.That(player.TrainingRules(tier).TokensForPlace(place), Is.GreaterThan(player.TrainingRules(tier - 1).TokensForPlace(place)));
                }
            }
        }
    }
}
