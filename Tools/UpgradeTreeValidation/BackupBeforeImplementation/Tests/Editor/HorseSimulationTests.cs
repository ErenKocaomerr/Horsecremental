using System;
using NUnit.Framework;

namespace Horsecremental.Racing.Tests
{
    public sealed class HorseSimulationTests
    {
        private const float STEP = 0.02f;
        private static readonly HorseParameters DEFAULT_PARAMETERS = new HorseParameters(10f, 12f, 5f, 0.06f, 0.02f, 7f, 20f, 0.45f, 1.2f);

        [Test]
        public void CountdownAndFinishRejectMovementAndInput()
        {
            HorseSimulation horse = Create();
            Assert.That(horse.TryBoost(), Is.False);
            Assert.That(horse.TryJump(), Is.False);
            horse.Tick(1f);
            Assert.That(horse.Distance, Is.Zero);
            horse.Start();
            Advance(horse, 1f);
            horse.Finish(420f);
            horse.Tick(1f);
            Assert.That(horse.Distance, Is.EqualTo(420f));
            Assert.That(horse.TryBoost(), Is.False);
            Assert.That(horse.TryJump(), Is.False);
        }

        [Test]
        public void BoostStacksPerTapIsCappedAndDecays()
        {
            HorseSimulation horse = Create();
            horse.Start();
            Advance(horse, 1f);
            Assert.That(horse.TryBoost(), Is.True);
            Assert.That(horse.BoostEnergy, Is.EqualTo(0.06f));
            Assert.That(horse.TryBoost(), Is.True);
            Assert.That(horse.BoostEnergy, Is.EqualTo(0.12f));
            for (int i = 0; i < 20; i++) horse.TryBoost();
            Assert.That(horse.BoostAmount, Is.EqualTo(5f));
            Advance(horse, 0.2f);
            Assert.That(horse.BoostEnergy, Is.InRange(0.85f, 0.95f));
            Assert.That(horse.Speed, Is.GreaterThan(10f));
            Advance(horse, 30f);
            Assert.That(horse.IsBoosting, Is.False);
            Assert.That(horse.BoostAmount, Is.Zero);
            Advance(horse, 0.5f);
            Assert.That(horse.Speed, Is.EqualTo(10f).Within(0.01f));
            Assert.That(horse.TryBoost(), Is.True);
        }

        [Test]
        public void JumpRejectsAirJumpAndLands()
        {
            HorseSimulation horse = Create();
            horse.Start();
            Assert.That(horse.TryJump(), Is.True);
            Assert.That(horse.TryJump(), Is.False);
            Advance(horse, 0.3f);
            Assert.That(horse.Height, Is.GreaterThan(0.65f));
            Assert.That(horse.TryJump(), Is.False);
            Advance(horse, 0.5f);
            Assert.That(horse.IsGrounded, Is.True);
            Assert.That(horse.TryJump(), Is.True);
        }

        [Test]
        public void MissedObstaclePenalizesOnceAndHorseStillFinishes()
        {
            HorseSimulation horse = Create(new RaceObstacle(20f, 0.65f, 0.65f));
            horse.Start();
            while (horse.ObstacleHits == 0) horse.Tick(STEP);
            Assert.That(horse.Speed, Is.LessThan(5f));
            Assert.That(horse.IsPenalized, Is.True);
            Advance(horse, 50f);
            Assert.That(horse.ObstacleHits, Is.EqualTo(1));
            Assert.That(horse.Distance, Is.GreaterThan(420f));
        }

        [TestCase(9.5f)]
        [TestCase(10.1f)]
        [TestCase(10.4f)]
        public void AutomaticJumpClearsEveryHurdleAndFinishesInTargetTime(float speed)
        {
            RaceObstacle[] obstacles = {
                new RaceObstacle(65f, 0.65f, 0.65f), new RaceObstacle(145f, 0.65f, 0.65f),
                new RaceObstacle(230f, 0.65f, 0.65f), new RaceObstacle(315f, 0.65f, 0.65f),
                new RaceObstacle(385f, 0.65f, 0.65f)
            };
            HorseSimulation horse = new HorseSimulation(new HorseParameters(speed, 12f, 5f, 1f, 3f, 7f, 20f, 0.45f, 1.2f), obstacles);
            horse.Start();
            float elapsed = 0f;
            while (horse.Distance < 420f && elapsed < 60f)
            {
                float distance = horse.NextObstacleDistance - horse.Distance;
                if (distance > 0f && distance <= horse.Speed * 0.28f) horse.TryJump();
                horse.Tick(STEP);
                elapsed += STEP;
            }
            Assert.That(horse.Distance, Is.GreaterThanOrEqualTo(420f));
            Assert.That(elapsed, Is.InRange(30f, 60f));
            Assert.That(horse.ObstacleHits, Is.Zero);
        }

        [Test]
        public void RestartClearsBoostJumpPenaltyAndObstacleHistory()
        {
            HorseSimulation horse = Create(new RaceObstacle(3f, 0.65f, 0.65f));
            horse.Start();
            Advance(horse, 1f);
            horse.TryBoost();
            horse.TryJump();
            horse.Tick(STEP);
            Assert.That(horse.ObstacleHits, Is.EqualTo(1));
            horse.Reset();
            Assert.That(horse.Distance, Is.Zero);
            Assert.That(horse.Speed, Is.Zero);
            Assert.That(horse.Height, Is.Zero);
            Assert.That(horse.BoostAmount, Is.Zero);
            Assert.That(horse.PreviousDistance, Is.Zero);
            Assert.That(horse.PreviousHeight, Is.Zero);
            Assert.That(horse.ObstacleHits, Is.Zero);
            Assert.That(horse.IsRunning, Is.False);
            Assert.That(horse.NextObstacleDistance, Is.EqualTo(3f));
        }

        private static HorseSimulation Create(params RaceObstacle[] obstacles) => new HorseSimulation(DEFAULT_PARAMETERS, obstacles);

        private static void Advance(HorseSimulation horse, float seconds)
        {
            int steps = (int)Math.Round(seconds / STEP);
            for (int i = 0; i < steps; i++) horse.Tick(STEP);
        }
    }
}
