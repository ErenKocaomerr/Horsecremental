using System;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Horsecremental.Racing.Tests
{
    public sealed class Milestone3ModelTests
    {
        private static WorldSettings Settings => AssetDatabase.LoadAssetAtPath<WorldSettings>("Assets/Racing/Config/World.asset");
        private static PlayerProgression Create() => AssetDatabase.LoadAssetAtPath<ProgressionSettings>("Assets/Racing/Config/Progression.asset").CreatePlayer();

        [Test]
        public void TrainingCarriesRemaindersAndDoesNotConsumeMainRaceReward()
        {
            PlayerProgression player = Create();
            long token = player.BeginRace();
            player.AdvanceTraining(37f, Settings);
            Assert.That(player.Coins, Is.EqualTo(70));
            Assert.That(player.TrainingRaceNumber, Is.EqualTo(3));
            Assert.That(player.TrainingElapsed, Is.EqualTo(7d));
            Assert.That(player.TryAwardRace(token, 500), Is.True);
            Assert.That(player.TryAwardRace(token, 500), Is.False);
            Assert.That(player.Coins, Is.EqualTo(570));
        }

        [Test]
        public void FarmCapsCollectsAndFeedChangesTheSameStatsModel()
        {
            PlayerProgression player = Create();
            player.AdvanceFarm(1000f, Settings);
            Assert.That(player.StoredCarrots, Is.EqualTo(10));
            Assert.That(player.FarmElapsed, Is.Zero);
            player.AdvanceFarm(1000f, Settings);
            Assert.That(player.StoredCarrots, Is.EqualTo(10));
            Assert.That(player.TryFeed(Settings), Is.False);
            Assert.That(player.CollectCarrots(), Is.EqualTo(10));
            Assert.That(player.CollectCarrots(), Is.Zero);
            Assert.That(player.TryFeed(Settings), Is.True);
            Assert.That(player.Stats.Speed, Is.EqualTo(10.1f).Within(0.001f));
            Assert.That(player.Carrots, Is.EqualTo(5));
            player.AdvanceFarm(25f, Settings);
            Assert.That(player.StoredCarrots, Is.EqualTo(1));
            Assert.That(player.FarmElapsed, Is.EqualTo(5d));
            Assert.That(player.Coins, Is.Zero);
        }

        [Test]
        public void HighBoostDrainsFasterAndRhythmicTapsSettleBelowMaximum()
        {
            PlayerProgression player = Create();
            HorseSimulation horse = new HorseSimulation(player.Stats, Array.Empty<RaceObstacle>());
            horse.Start();
            horse.TryBoost();
            float lowDrain = horse.BoostDrain;
            for (int i = 0; i < 20; i++) horse.TryBoost();
            Assert.That(horse.BoostDrain, Is.GreaterThan(lowDrain * 10f));
            for (int i = 0; i < 6000; i++)
            {
                if (i % 25 == 0) horse.TryBoost();
                horse.Tick(0.02f);
            }
            Assert.That(horse.BoostEnergy, Is.InRange(0.5f, 0.65f));
            float energy = horse.BoostEnergy;
            for (int i = 0; i < 25; i++) horse.Tick(0.02f);
            Assert.That(horse.BoostEnergy, Is.LessThan(energy));
        }

        [Test]
        public void FixedOpponentsBeatFreshRhythmicPlayerAndLoseAfterProgression()
        {
            RaceSettings race = AssetDatabase.LoadAssetAtPath<RaceSettings>("Assets/Racing/Config/PrototypeRace.asset");
            PlayerProgression player = Create();
            float fastestRival = FinishTime(race.OpponentStats[2].Parameters, race, false);
            float slowestRival = FinishTime(race.OpponentStats[0].Parameters, race, false);
            float fresh = FinishTime(player.Stats, race, true);
            Assert.That(fresh, Is.GreaterThan(slowestRival));
            // Nine minutes of training, with early Speed and Boost purchases and feeding.
            for (int minute = 0; minute < 10; minute++)
            {
                player.AdvanceTraining(60f, Settings);
                player.AdvanceFarm(60f, Settings);
                player.CollectCarrots();
                while (player.TryFeed(Settings)) { }
                player.TryUpgrade(HorseUpgrade.Speed);
                player.TryUpgrade(HorseUpgrade.BoostPower);
                player.TryUpgrade(HorseUpgrade.Acceleration);
            }
            float upgraded = FinishTime(player.Stats, race, true);
            Assert.That(upgraded, Is.LessThan(fastestRival));
            Assert.That(race.OpponentStats[2].Parameters.Speed, Is.EqualTo(15f));
            TestContext.WriteLine("Fresh: " + fresh + "s, slowest rival: " + slowestRival + "s, fastest rival: " + fastestRival + "s, upgraded after 10 training minutes: " + upgraded + "s. Speed=" + player.Stats.Speed + ", boost=" + player.Stats.BoostPower);
        }

        [Test]
        public void WorldHasSingleCameraListenerAndEventSystemAndSmoothCinemachineBlend()
        {
            EditorSceneManager.OpenScene("Assets/Racing/Scenes/Milestone3World.unity");
            Assert.That(UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include).Length, Is.EqualTo(1));
            Assert.That(UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include).Length, Is.EqualTo(1));
            Assert.That(UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include).Length, Is.EqualTo(1));
            Assert.That(UnityEngine.Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include).Length, Is.EqualTo(3));
            CinemachineBrain brain = UnityEngine.Object.FindAnyObjectByType<CinemachineBrain>();
            Assert.That(brain.DefaultBlend.Time, Is.InRange(0.5f, 1.5f));
            Assert.That(brain.DefaultBlend.Style, Is.EqualTo(CinemachineBlendDefinition.Styles.EaseInOut));
            Assert.That(GameObject.Find("Stable and Farm Environment"), Is.Not.Null);
            Assert.That(GameObject.Find("Prototype Track"), Is.Not.Null);
            GameplayWorld world = UnityEngine.Object.FindAnyObjectByType<GameplayWorld>();
            Assert.That(world.Settings, Is.SameAs(Settings), "World settings must survive scene save and reload.");
            foreach (Canvas canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
            {
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                Assert.That(scaler, Is.Not.Null);
                Assert.That(scaler.referenceResolution, Is.EqualTo(new Vector2(1920, 1080)));
                Assert.That(scaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
            }
        }

        private static float FinishTime(HorseParameters parameters, RaceSettings race, bool boost)
        {
            HorseSimulation horse = new HorseSimulation(parameters, race.CreateObstacles());
            horse.Start();
            int ticks = 0;
            while (horse.Distance < race.TrackLength && ticks < 6000)
            {
                if (boost && ticks % 25 == 0) horse.TryBoost();
                float distance = horse.NextObstacleDistance - horse.Distance;
                if (distance > 0f && distance <= horse.Speed * 0.28f) horse.TryJump();
                horse.Tick(0.02f);
                ticks++;
            }
            Assert.That(horse.ObstacleHits, Is.Zero);
            return ticks * 0.02f;
        }
    }
}
