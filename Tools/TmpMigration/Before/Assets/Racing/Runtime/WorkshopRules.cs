using System;

namespace Horsecremental.Racing
{
    public enum EquipmentType { Horseshoes, Saddle, Bridle }
    public enum ProductionUpgrade { ForgeSpeed, MetalEfficiency, GrowSpeed, FarmCapacity, TrainingSpeed, TrainingReward,
        TrainingHorseCount, ClickGrowthPower, HarvestYield }

    public readonly struct EquipmentTier
    {
        public string Name { get; }
        public int Coins { get; }
        public int Metal { get; }
        public float Seconds { get; }
        public float Bonus { get; }
        public EquipmentTier(string name, int coins, int metal, float seconds, float bonus)
        {
            Name = name;
            Coins = Math.Max(0, coins);
            Metal = Math.Max(0, metal);
            Seconds = Math.Max(0.1f, seconds);
            Bonus = Math.Max(0f, bonus);
        }
    }

    public readonly struct ProductionRule
    {
        public string Name { get; }
        public int BaseCost { get; }
        public double Growth { get; }
        public float Effect { get; }
        public int MaxLevel { get; }
        public ProductionRule(string name, int cost, double growth, float effect, int maxLevel)
        {
            Name = name;
            BaseCost = Math.Max(1, cost);
            Growth = Math.Round(Math.Max(1d, growth), 6);
            Effect = Math.Max(0f, effect);
            MaxLevel = Math.Max(0, maxLevel);
        }
        public int Cost(int level) => level >= MaxLevel ? int.MaxValue :
            (int)Math.Min(int.MaxValue, Math.Ceiling(Math.Round(BaseCost * Math.Pow(Growth, level), 6)));
    }

    public interface IWorkshopSettings
    {
        int TierCount(EquipmentType type);
        EquipmentTier Tier(EquipmentType type, int tier);
        ProductionRule Rule(ProductionUpgrade upgrade);
    }

    public readonly struct StatBreakdown
    {
        public float Base { get; }
        public float Upgrades { get; }
        public float Food { get; }
        public float Equipment { get; }
        public float Total => Base + Upgrades + Food + Equipment;
        public StatBreakdown(float basis, float upgrades, float food, float equipment)
        { Base = basis; Upgrades = upgrades; Food = food; Equipment = equipment; }
    }
}
