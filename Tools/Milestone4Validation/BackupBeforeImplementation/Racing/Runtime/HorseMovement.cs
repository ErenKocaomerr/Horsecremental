using System;
using UnityEngine;

namespace Horsecremental.Racing
{
    public sealed class HorseMovement : MonoBehaviour
    {
        private float _laneY;
        private HorseView _view;
        public HorseSimulation Simulation { get; private set; }
        public HorseStats Stats { get; private set; }
        public HorseParameters Parameters { get; private set; }
        public event Action BoostActivated;
        public event Action ObstacleHit;

        public void Initialize(HorseStats stats, RaceObstacle[] obstacles, float laneY)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            Initialize(stats, stats.Parameters, obstacles, laneY);
        }

        public void Initialize(HorseStats stats, HorseParameters parameters, RaceObstacle[] obstacles, float laneY)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            Stats = stats;
            Parameters = parameters;
            TryGetComponent(out _view);
            Simulation = new HorseSimulation(parameters, obstacles);
            _laneY = laneY;
            ResetHorse();
        }

        public void ResetHorse()
        {
            Simulation.Reset();
            SynchronizePosition();
        }

        public void Step(float deltaTime)
        {
            int previousHits = Simulation.ObstacleHits;
            Simulation.Tick(deltaTime);
            transform.position = new Vector3(Simulation.Distance, _laneY, 0f);
            if (Simulation.ObstacleHits != previousHits) ObstacleHit?.Invoke();
        }

        public bool TryBoost()
        {
            if (Simulation == null || !Simulation.TryBoost()) return false;
            BoostActivated?.Invoke();
            return true;
        }

        public bool TryJump() => Simulation != null && Simulation.TryJump();

        public void SynchronizePosition()
        {
            transform.position = new Vector3(Simulation.Distance, _laneY, 0f);
            if (_view != null) _view.Snap();
        }
    }
}
