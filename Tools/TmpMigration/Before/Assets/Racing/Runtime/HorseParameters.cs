using System;

namespace Horsecremental.Racing
{
    public readonly struct HorseParameters
    {
        public float Speed { get; }
        public float Acceleration { get; }
        public float BoostPower { get; }
        public float BoostPerTap { get; }
        public float BoostDecayPerSecond { get; }
        public float BoostHighDrain { get; }
        public float JumpPower { get; }
        public float Gravity { get; }
        public float PenaltyMultiplier { get; }
        public float PenaltyDuration { get; }

        public HorseParameters(float speed, float acceleration, float boostPower,
            float boostPerTap, float boostDecayPerSecond, float jumpPower, float gravity,
            float penaltyMultiplier, float penaltyDuration, float boostHighDrain = 0.58f)
        {
            if (speed <= 0f || acceleration <= 0f || boostPower < 0f ||
                boostPerTap <= 0f || boostDecayPerSecond <= 0f || jumpPower <= 0f ||
                gravity <= 0f || penaltyMultiplier <= 0f || penaltyMultiplier >= 1f || penaltyDuration <= 0f)
                throw new ArgumentOutOfRangeException(nameof(speed), "Horse parameters must describe a valid racer.");

            Speed = speed;
            Acceleration = acceleration;
            BoostPower = boostPower;
            BoostPerTap = boostPerTap;
            BoostDecayPerSecond = boostDecayPerSecond;
            BoostHighDrain = boostHighDrain;
            JumpPower = jumpPower;
            Gravity = gravity;
            PenaltyMultiplier = penaltyMultiplier;
            PenaltyDuration = penaltyDuration;
        }
    }

    public readonly struct RaceObstacle
    {
        public float Distance { get; }
        public float Width { get; }
        public float Clearance { get; }

        public RaceObstacle(float distance, float width, float clearance)
        {
            Distance = distance;
            Width = width;
            Clearance = clearance;
        }
    }
}
