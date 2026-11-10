using System;

namespace Horsecremental.Racing
{
    public enum HorseUpgrade { Speed, Acceleration, BoostPower }

    public readonly struct UpgradeRule
    {
        public int BaseCost { get; }
        public double GrowthRate { get; }
        public float Increase { get; }

        public UpgradeRule(int baseCost, double growthRate, float increase)
        {
            if (baseCost <= 0 || double.IsNaN(growthRate) || double.IsInfinity(growthRate) ||
                growthRate <= 1d || float.IsNaN(increase) || float.IsInfinity(increase) || increase <= 0f)
                throw new ArgumentOutOfRangeException(nameof(baseCost));
            BaseCost = baseCost;
            GrowthRate = Math.Round(growthRate, 6);
            Increase = increase;
        }

        public int CostAtLevel(int level)
        {
            if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));
            double rawPrice = BaseCost * Math.Pow(GrowthRate, level - 1);
            if (rawPrice >= int.MaxValue) return int.MaxValue;
            double price = Math.Ceiling(Math.Round(rawPrice, 6));
            // int.MaxValue is an unreachable price sentinel; never wrap a price negative.
            return price >= int.MaxValue ? int.MaxValue : (int)price;
        }
    }
}
