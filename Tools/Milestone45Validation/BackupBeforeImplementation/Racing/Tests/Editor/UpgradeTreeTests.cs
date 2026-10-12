using System;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Horsecremental.Racing.Tests
{
    public sealed class UpgradeTreeTests
    {
        private static ProgressionSettings Settings => AssetDatabase.LoadAssetAtPath<ProgressionSettings>("Assets/Racing/Config/Progression.asset");
        private static WorldSettings World => AssetDatabase.LoadAssetAtPath<WorldSettings>("Assets/Racing/Config/World.asset");

        [Test]
        public void NewPlayerHasOnlySpeedAndCannotBypassLocksWithLargeBalances()
        {
            PlayerProgression player = Settings.CreatePlayer();
            Assert.That(player.Coins, Is.Zero); Assert.That(player.RaceTokens, Is.Zero);
            for (int i = 0; i < UpgradeTreeRules.NODE_COUNT; i++)
                Assert.That(player.IsUnlocked((UpgradeNode)i), Is.EqualTo(i == 0));
            player.GrantDebugResources(100000, 1000);
            Assert.That(player.TryUpgrade(HorseUpgrade.Speed), Is.True);
            Assert.That(player.TryUpgrade(HorseUpgrade.Acceleration), Is.False);
            Assert.That(player.TryUpgrade(HorseUpgrade.BoostPower), Is.False);
            foreach (ProductionUpgrade upgrade in Enum.GetValues(typeof(ProductionUpgrade)))
                Assert.That(player.TryUpgradeProduction(upgrade, World), Is.False);
            player.TryAwardRace(player.BeginRace(), 0, 1000);
            Assert.That(player.TryStartCraft(EquipmentType.Horseshoes), Is.False);
            player.AdvanceFarm(10000, World);
            Assert.That(player.FarmElapsed, Is.Zero); Assert.That(player.StoredCarrots, Is.Zero);
            Assert.That(player.CollectCarrots(), Is.Zero); Assert.That(player.TryFeed(World), Is.False);
        }

        [Test]
        public void CoinUnlocksRequireParentsAndNeverSpendTokensOrUpgradeStats()
        {
            PlayerProgression player = Settings.CreatePlayer(); player.GrantDebugResources(230, 15);
            int events = 0; player.Changed += () => events++;
            HorseParameters before = player.Stats;
            Assert.That(player.TryUnlock(UpgradeNode.BoostPower), Is.False);
            Assert.That(player.Coins, Is.EqualTo(230)); Assert.That(events, Is.Zero);
            Assert.That(player.TryUnlock(UpgradeNode.Acceleration), Is.True);
            Assert.That(player.TryUnlock(UpgradeNode.BoostPower), Is.True);
            Assert.That(player.Coins, Is.Zero); Assert.That(player.RaceTokens, Is.EqualTo(15));
            Assert.That(events, Is.EqualTo(2));
            Assert.That(player.Stats.Acceleration, Is.EqualTo(before.Acceleration));
            Assert.That(player.TryUnlock(UpgradeNode.Acceleration), Is.False);
            Assert.That(events, Is.EqualTo(2));
            player.GrantDebugResources(player.Cost(HorseUpgrade.Acceleration), 0);
            Assert.That(player.TryUpgrade(HorseUpgrade.Acceleration), Is.True);
            Assert.That(player.Stats.Acceleration, Is.GreaterThan(before.Acceleration));
        }

        [Test]
        public void TokenBranchesChargeExactlyOnceAndActivateTheirSystems()
        {
            PlayerProgression player = Settings.CreatePlayer(); player.GrantDebugResources(1000, 14);
            Assert.That(player.TryUnlock(UpgradeNode.Blacksmith), Is.False);
            Assert.That(player.TryUnlock(UpgradeNode.Farm), Is.True);
            Assert.That(player.Coins, Is.EqualTo(1000)); Assert.That(player.RaceTokens, Is.EqualTo(9));
            Assert.That(player.TryUnlock(UpgradeNode.Blacksmith), Is.False);
            Assert.That(player.TryUnlock(UpgradeNode.Farm), Is.False);
            player.AdvanceFarm(player.CarrotSeconds(World), World);
            Assert.That(player.StoredCarrots, Is.EqualTo(1)); Assert.That(player.CollectCarrots(), Is.EqualTo(1));
            player.GrantDebugResources(0, 1);
            Assert.That(player.TryUnlock(UpgradeNode.Blacksmith), Is.True);
            Assert.That(player.RaceTokens, Is.Zero); Assert.That(player.Coins, Is.EqualTo(1000));
            player.TryAwardRace(player.BeginRace(), 0, 50);
            Assert.That(player.TryStartCraft(EquipmentType.Horseshoes), Is.True);
            player.AdvanceCraft(100);
            Assert.That(player.EquipmentLevel(EquipmentType.Horseshoes), Is.EqualTo(1));
        }

        [Test]
        public void FailedPurchasesLeaveAllResourcesAndLevelsUntouched()
        {
            PlayerProgression player = Settings.CreatePlayer(); player.GrantDebugResources(79, 4);
            int events = 0; player.Changed += () => events++;
            Assert.That(player.TryUnlock(UpgradeNode.Acceleration), Is.False);
            Assert.That(player.TryUnlock(UpgradeNode.Farm), Is.False);
            Assert.That(player.Coins, Is.EqualTo(79)); Assert.That(player.RaceTokens, Is.EqualTo(4));
            Assert.That(player.Level(HorseUpgrade.Acceleration), Is.EqualTo(1)); Assert.That(events, Is.Zero);
        }

        [TestCase(1, 5)] [TestCase(2, 3)] [TestCase(3, 2)] [TestCase(4, 1)] [TestCase(5, 0)]
        public void MainRacePlaceAwardsConfiguredTokensOnce(int place, int expected)
        {
            PlayerProgression player = Settings.CreatePlayer();
            long race = player.BeginRace();
            Assert.That(Settings.TokensForPlace(place), Is.EqualTo(expected));
            Assert.That(player.TryAwardRace(race, Settings.RewardForPlace(place), Settings.MetalForPlace(place), Settings.TokensForPlace(place)), Is.True);
            Assert.That(player.TryAwardRace(race, 999, 999, 999), Is.False);
            Assert.That(player.RaceTokens, Is.EqualTo(expected));
            long stale = player.BeginRace(); player.BeginRace();
            Assert.That(player.TryAwardRace(stale, 0, 0, 100), Is.False);
            Assert.That(player.RaceTokens, Is.EqualTo(expected));
        }

        [Test]
        public void TrainingAndDebugGrantsDoNotConsumeRaceClaimsOrAutomaticallyUnlockNodes()
        {
            PlayerProgression player = Settings.CreatePlayer(); long race = player.BeginRace();
            player.AdvanceTraining(10000, World);
            Assert.That(player.RaceTokens, Is.Zero);
            player.GrantDebugResources(100000, 1000);
            Assert.That(player.IsUnlocked(UpgradeNode.Farm), Is.False);
            Assert.That(player.TryAwardRace(race, 100, 3, 1), Is.True);
            Assert.That(player.RaceTokens, Is.EqualTo(1001));
            player.GrantDebugResources(int.MaxValue, int.MaxValue);
            player.GrantDebugResources(100000, 1000);
            Assert.That(player.Coins, Is.EqualTo(int.MaxValue)); Assert.That(player.RaceTokens, Is.EqualTo(int.MaxValue));
            Assert.Throws<ArgumentOutOfRangeException>(() => player.GrantDebugResources(-1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => player.GrantDebugResources(0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => player.TryAwardRace(race, 0, 0, -1));
        }

        [Test]
        public void RulesRejectCyclesAndDuplicatesAndSnapshotTheirInput()
        {
            UpgradeNodeRule[] nodes = new UpgradeNodeRule[UpgradeTreeRules.NODE_COUNT];
            for (int i = 0; i < nodes.Length; i++) nodes[i] = UpgradeTreeRules.DEFAULT.Rule((UpgradeNode)i);
            UpgradeTreeRules snapshot = new UpgradeTreeRules(nodes);
            nodes[1] = new UpgradeNodeRule(UpgradeNode.Acceleration, UpgradeNode.BoostPower, UpgradeCurrency.Coins, 1);
            Assert.That(snapshot.Rule(UpgradeNode.Acceleration).Prerequisite, Is.EqualTo(UpgradeNode.Speed));
            Assert.Throws<ArgumentException>(() => new UpgradeTreeRules(nodes));
            nodes[1] = nodes[0];
            Assert.Throws<ArgumentException>(() => new UpgradeTreeRules(nodes));
        }

        [Test]
        public void SavedWorldHasConfiguredTreeNavigationCheatAndPreservesSceneGuid()
        {
            const string WORLD_PATH = "Assets/Racing/Scenes/Milestone3World.unity";
            EditorSceneManager.OpenScene(WORLD_PATH);
            Assert.That(Settings.UpgradeTree, Is.Not.Null);
            Assert.That(UnityEngine.Object.FindObjectsByType<UpgradeTreeUI>().Length, Is.EqualTo(1));
            Assert.That(UnityEngine.Object.FindObjectsByType<UpgradeTreeNodeUI>().Length, Is.EqualTo(UpgradeTreeRules.NODE_COUNT));
            DebugResourceCheat cheat = UnityEngine.Object.FindAnyObjectByType<DebugResourceCheat>();
            Assert.That(cheat, Is.Not.Null);
            Assert.That(new SerializedObject(cheat).FindProperty("_settings").objectReferenceValue, Is.SameAs(Settings));
            AreaNavigationUI nav = UnityEngine.Object.FindAnyObjectByType<AreaNavigationUI>();
            Assert.That(new SerializedObject(nav).FindProperty("_buttons").arraySize, Is.EqualTo(5));
            for (int i = 0; i < UpgradeTreeRules.NODE_COUNT; i++)
            {
                UpgradeTreeNodeUI node = GameObject.Find("Unlock " + (UpgradeNode)i).GetComponent<UpgradeTreeNodeUI>();
                Assert.That(new SerializedObject(node).FindProperty("_node").intValue, Is.EqualTo(i));
            }
            Assert.That(AssetDatabase.AssetPathToGUID(WORLD_PATH), Is.EqualTo("8789f1f4a37cec24183629f77a4555ec"));
        }
    }
}
