using System;
using NUnit.Framework;
using UnityEditor;

namespace Horsecremental.Racing.Tests
{
    public sealed class PlayerProgressionTests
    {
        private static PlayerProgression CreatePlayer() => UpgradeTreeTestSupport.UnlockAll(new PlayerProgression(
            new HorseParameters(10f, 12f, 5f, 0.06f, 0.02f, 7f, 20f, 0.45f, 1.2f),
            new UpgradeRule(60, 1.45f, 0.75f), new UpgradeRule(50, 1.45f, 3f), new UpgradeRule(60, 1.45f, 1f)));

        [Test]
        public void PurchasesAreAtomicPricesGrowAndBalanceNeverGoesNegative()
        {
            PlayerProgression player = CreatePlayer();
            int changes = 0;
            player.Changed += () => changes++;
            Assert.That(player.TryUpgrade(HorseUpgrade.Speed), Is.False);
            Assert.That(player.Level(HorseUpgrade.Speed), Is.EqualTo(1));
            Assert.That(changes, Is.Zero);
            long race = player.BeginRace();
            Assert.That(player.TryAwardRace(race, 100), Is.True);
            Assert.That(player.TryUpgrade(HorseUpgrade.Speed), Is.True);
            Assert.That(player.Coins, Is.EqualTo(40));
            Assert.That(player.Cost(HorseUpgrade.Speed), Is.EqualTo(87));
            Assert.That(player.Stats.Speed, Is.EqualTo(10.75f));
            Assert.That(player.TryUpgrade(HorseUpgrade.Acceleration), Is.False);
            Assert.That(player.Coins, Is.EqualTo(40));
            Assert.That(changes, Is.EqualTo(2));
            for (int i = 0; i < 20; i++)
            {
                player.TryAwardRace(player.BeginRace(), 100);
                player.TryUpgrade(HorseUpgrade.Speed);
                player.TryUpgrade(HorseUpgrade.Acceleration);
                player.TryUpgrade(HorseUpgrade.BoostPower);
                Assert.That(player.Coins, Is.GreaterThanOrEqualTo(0));
            }
        }

        [Test]
        public void EachRacePaysOnceAndAbandonedRaceTokensCannotPay()
        {
            PlayerProgression player = CreatePlayer();
            Assert.That(player.TryAwardRace(0, 100), Is.False);
            long first = player.BeginRace();
            Assert.That(player.TryAwardRace(first, 70), Is.True);
            Assert.That(player.TryAwardRace(first, 70), Is.False);
            long second = player.BeginRace();
            Assert.That(player.TryAwardRace(first, 100), Is.False);
            Assert.That(player.TryAwardRace(second, 30), Is.True);
            Assert.That(player.Coins, Is.EqualTo(100));
            Assert.Throws<ArgumentOutOfRangeException>(() => player.TryAwardRace(second, -1));
        }

        [Test]
        public void EconomyCannotOverflowIntoNegativeCurrencyOrPrices()
        {
            PlayerProgression player = CreatePlayer();
            player.TryAwardRace(player.BeginRace(), int.MaxValue);
            player.TryAwardRace(player.BeginRace(), 100);
            Assert.That(player.Coins, Is.EqualTo(int.MaxValue));
            Assert.That(new UpgradeRule(60, 1.45, 0.75f).CostAtLevel(10000), Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void AllUpgradesChangeActualSimulationAndLeaveJumpParametersAlone()
        {
            PlayerProgression player = CreatePlayer();
            HorseParameters before = player.Stats;
            player.TryAwardRace(player.BeginRace(), 1000);
            player.TryUpgrade(HorseUpgrade.Acceleration);
            HorseSimulation baseline = new HorseSimulation(before, Array.Empty<RaceObstacle>());
            HorseSimulation accelerated = new HorseSimulation(player.Stats, Array.Empty<RaceObstacle>());
            baseline.Start();
            accelerated.Start();
            baseline.Tick(0.1f);
            accelerated.Tick(0.1f);
            Assert.That(accelerated.Speed, Is.GreaterThan(baseline.Speed));
            player.TryUpgrade(HorseUpgrade.Speed);
            player.TryUpgrade(HorseUpgrade.BoostPower);
            HorseSimulation upgraded = new HorseSimulation(player.Stats, Array.Empty<RaceObstacle>());
            upgraded.Start();
            for (int i = 0; i < 200; i++) { baseline.Tick(0.02f); upgraded.Tick(0.02f); }
            Assert.That(upgraded.Distance, Is.GreaterThan(baseline.Distance));
            Assert.That(upgraded.Speed, Is.EqualTo(10.75f).Within(0.001f));
            baseline.TryBoost();
            upgraded.TryBoost();
            Assert.That(upgraded.BoostAmount, Is.GreaterThan(baseline.BoostAmount));
            Assert.That(upgraded.BoostEnergy, Is.EqualTo(baseline.BoostEnergy));
            for (int i = 0; i < 20; i++) upgraded.TryBoost();
            Assert.That(upgraded.BoostAmount, Is.EqualTo(6f));
            Assert.That(player.Stats.JumpPower, Is.EqualTo(before.JumpPower));
            Assert.That(player.Stats.PenaltyMultiplier, Is.EqualTo(before.PenaltyMultiplier));
        }

        [TestCase(1, 500)]
        [TestCase(2, 250)]
        [TestCase(3, 150)]
        [TestCase(4, 100)]
        [TestCase(5, 0)]
        public void RewardAssetPaysConfiguredFinishingPositions(int place, int expected)
        {
            ProgressionSettings settings = AssetDatabase.LoadAssetAtPath<ProgressionSettings>("Assets/Racing/Config/Progression.asset");
            Assert.That(settings.RewardForPlace(place), Is.EqualTo(expected));
        }

        [Test]
        public void SeveralSpeedUpgradesMakeTheSameUnassistedRaceNoticeablyFaster()
        {
            ProgressionSettings settings = AssetDatabase.LoadAssetAtPath<ProgressionSettings>("Assets/Racing/Config/Progression.asset");
            RaceSettings race = AssetDatabase.LoadAssetAtPath<RaceSettings>("Assets/Racing/Config/PrototypeRace.asset");
            PlayerProgression player = settings.CreatePlayer();
            float originalTime = FinishTime(player.Stats, race.CreateObstacles(), race.TrackLength);
            for (int i = 0; i < 4; i++)
            {
                player.TryAwardRace(player.BeginRace(), 1000);
                Assert.That(player.TryUpgrade(HorseUpgrade.Speed), Is.True);
            }
            float improvedTime = FinishTime(player.Stats, race.CreateObstacles(), race.TrackLength);
            Assert.That(improvedTime, Is.LessThan(originalTime * 0.85f),
                "Four Speed upgrades should save at least 15% on the same race, with identical missed hurdles.");
        }

        private static float FinishTime(HorseParameters stats, RaceObstacle[] obstacles, float length)
        {
            HorseSimulation horse = new HorseSimulation(stats, obstacles);
            horse.Start();
            int ticks = 0;
            while (horse.Distance < length && ticks < 10000) { horse.Tick(0.02f); ticks++; }
            Assert.That(horse.Distance, Is.GreaterThanOrEqualTo(length));
            return ticks * 0.02f;
        }
    }
}
