using System;

namespace Horsecremental.Racing
{
    // Pure C#: forward progress and jump height are independent of the horse's visual lane.
    public sealed class HorseSimulation
    {
        private const float HORSE_HALF_WIDTH = 0.45f;
        private readonly HorseParameters _parameters;
        private readonly RaceObstacle[] _obstacles;
        private float _verticalSpeed;
        private float _penaltyRemaining;
        private int _nextObstacle;
        private int _penalizedObstacle = -1;

        public float Distance { get; private set; }
        public float PreviousDistance { get; private set; }
        public float Speed { get; private set; }
        public float Height { get; private set; }
        public float PreviousHeight { get; private set; }
        public float BoostEnergy { get; private set; }
        public float BoostAmount => BoostEnergy * _parameters.BoostPower;
        public float BoostDrain => _parameters.BoostDecayPerSecond + _parameters.BoostHighDrain * BoostEnergy * BoostEnergy * BoostEnergy;
        public int ObstacleHits { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsGrounded => Height <= 0f && _verticalSpeed <= 0f;
        public bool IsBoosting => BoostAmount > 0f;
        public bool IsPenalized => _penaltyRemaining > 0f;
        public float NextObstacleDistance => _nextObstacle < _obstacles.Length
            ? _obstacles[_nextObstacle].Distance : float.PositiveInfinity;

        public HorseSimulation(HorseParameters parameters, RaceObstacle[] obstacles)
        {
            _parameters = parameters;
            _obstacles = obstacles ?? throw new ArgumentNullException(nameof(obstacles));
        }

        public void Reset()
        {
            Distance = PreviousDistance = Speed = Height = PreviousHeight = BoostEnergy = 0f;
            _verticalSpeed = _penaltyRemaining = 0f;
            _nextObstacle = ObstacleHits = 0;
            _penalizedObstacle = -1;
            IsRunning = false;
        }

        public void Start() => IsRunning = true;

        public void Finish(float finishDistance)
        {
            IsRunning = false;
            Distance = PreviousDistance = finishDistance;
            Speed = Height = PreviousHeight = _verticalSpeed = BoostEnergy = 0f;
        }

        public bool TryBoost()
        {
            if (!IsRunning || _parameters.BoostPower <= 0f) return false;
            BoostEnergy = Math.Min(1f, BoostEnergy + _parameters.BoostPerTap);
            return true;
        }

        public bool TryJump()
        {
            if (!IsRunning || !IsGrounded) return false;
            _verticalSpeed = _parameters.JumpPower;
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (!IsRunning || deltaTime <= 0f) return;
            PreviousDistance = Distance;
            PreviousHeight = Height;
            float previousHeight = Height;
            float targetSpeed = _parameters.Speed + BoostAmount;
            if (IsPenalized) targetSpeed *= _parameters.PenaltyMultiplier;
            float change = _parameters.Acceleration * deltaTime;
            Speed += Math.Max(-change, Math.Min(change, targetSpeed - Speed));
            Distance += Speed * deltaTime;

            if (!IsGrounded)
            {
                Height = Math.Max(0f, Height + _verticalSpeed * deltaTime - 0.5f * _parameters.Gravity * deltaTime * deltaTime);
                _verticalSpeed -= _parameters.Gravity * deltaTime;
                if (Height <= 0f) _verticalSpeed = 0f;
            }

            BoostEnergy = Math.Max(0f, BoostEnergy - BoostDrain * deltaTime);
            _penaltyRemaining = Math.Max(0f, _penaltyRemaining - deltaTime);
            CheckObstacles(previousHeight);
        }

        private void CheckObstacles(float previousHeight)
        {
            while (_nextObstacle < _obstacles.Length)
            {
                RaceObstacle obstacle = _obstacles[_nextObstacle];
                float entry = obstacle.Distance - obstacle.Width * 0.5f - HORSE_HALF_WIDTH;
                float exit = obstacle.Distance + obstacle.Width * 0.5f + HORSE_HALF_WIDTH;
                if (Distance < entry) return;

                if (PreviousDistance <= exit && _penalizedObstacle != _nextObstacle)
                {
                    float travel = Distance - PreviousDistance;
                    float first = travel > 0f ? Math.Max(0f, (entry - PreviousDistance) / travel) : 0f;
                    float last = travel > 0f ? Math.Min(1f, (exit - PreviousDistance) / travel) : 1f;
                    float firstHeight = previousHeight + (Height - previousHeight) * first;
                    float lastHeight = previousHeight + (Height - previousHeight) * last;
                    if (Math.Min(firstHeight, lastHeight) < obstacle.Clearance)
                    {
                        _penalizedObstacle = _nextObstacle;
                        _penaltyRemaining = _parameters.PenaltyDuration;
                        Speed *= _parameters.PenaltyMultiplier;
                        ObstacleHits++;
                    }
                }

                if (Distance <= exit) return;
                _nextObstacle++;
            }
        }
    }
}
