using System;
using UnityEngine;

namespace Horsecremental.Racing
{
    [CreateAssetMenu(menuName = "Horsecremental/Workshop Settings")]
    public sealed class WorkshopSettings : ScriptableObject, IWorkshopSettings
    {
        [Serializable]
        private sealed class TierDefinition
        {
            [SerializeField] private string _name;
            [SerializeField, Min(0)] private int _coins;
            [SerializeField, Min(0)] private int _metal;
            [SerializeField, Min(0.1f)] private float _seconds;
            [SerializeField, Min(0f)] private float _bonus;
            public TierDefinition(string name, int coins, int metal, float seconds, float bonus)
            { _name = name; _coins = coins; _metal = metal; _seconds = seconds; _bonus = bonus; }
            public EquipmentTier Value => new EquipmentTier(_name, _coins, _metal, _seconds, _bonus);
        }
        [Serializable]
        private sealed class ProductionDefinition
        {
            [SerializeField] private string _name;
            [SerializeField, Min(1)] private int _baseCost;
            [SerializeField, Min(1f)] private float _growth = 1.6f;
            [SerializeField, Min(0f)] private float _effect;
            [SerializeField, Range(0, 5)] private int _maxLevel = 5;
            public ProductionDefinition(string name, int cost, float effect)
            { _name = name; _baseCost = cost; _effect = effect; }
            public ProductionRule Value => new ProductionRule(_name, _baseCost, _growth, _effect, _maxLevel);
        }
        [SerializeField] private TierDefinition[] _horseshoes = {
            new TierDefinition("Old Horseshoes", 0, 0, 1, 0),
            new TierDefinition("Iron Horseshoes", 100, 5, 5, 1.5f),
            new TierDefinition("Reinforced Horseshoes", 300, 12, 8, 3),
            new TierDefinition("Steel Racing Horseshoes", 700, 25, 10, 5),
            new TierDefinition("Champion Horseshoes", 1500, 50, 12, 7) };
        [SerializeField] private TierDefinition[] _saddles = {
            new TierDefinition("Old Saddle", 0, 0, 1, 0),
            new TierDefinition("Leather Saddle", 100, 5, 5, 3),
            new TierDefinition("Racing Saddle", 300, 12, 8, 6),
            new TierDefinition("Professional Saddle", 700, 25, 10, 10),
            new TierDefinition("Champion Saddle", 1500, 50, 12, 14) };
        [SerializeField] private TierDefinition[] _bridles = {
            new TierDefinition("Basic Bridle", 0, 0, 1, 0),
            new TierDefinition("Reinforced Bridle", 100, 5, 5, 1.5f),
            new TierDefinition("Racing Bridle", 300, 12, 8, 3),
            new TierDefinition("Professional Bridle", 700, 25, 10, 5),
            new TierDefinition("Champion Bridle", 1500, 50, 12, 7) };
        [SerializeField] private ProductionDefinition[] _production = {
            new ProductionDefinition("Forge Speed", 150, 0.1f),
            new ProductionDefinition("Metal Efficiency", 200, 0.05f),
            new ProductionDefinition("Grow Speed", 120, 0.1f),
            new ProductionDefinition("Farm Capacity", 100, 5),
            new ProductionDefinition("Training Speed", 150, 0.1f),
            new ProductionDefinition("Training Reward", 180, 0.15f) };

        public int TierCount(EquipmentType type) => Definitions(type).Length;
        public EquipmentTier Tier(EquipmentType type, int tier)
        {
            TierDefinition[] definitions = Definitions(type);
            if (tier < 0 || tier >= definitions.Length) throw new ArgumentOutOfRangeException(nameof(tier));
            return definitions[tier].Value;
        }
        public ProductionRule Rule(ProductionUpgrade upgrade)
        {
            int index = (int)upgrade;
            // Separate serialized rules preserve the six legacy array entries and IDs.
            if (upgrade == ProductionUpgrade.TrainingHorseCount) return _horseCount.Value;
            if (upgrade == ProductionUpgrade.ClickGrowthPower) return _clickPower.Value;
            if (upgrade == ProductionUpgrade.HarvestYield) return _yield.Value;
            if (index < 0 || index >= _production.Length) throw new ArgumentOutOfRangeException(nameof(upgrade));
            return _production[index].Value;
        }
        [SerializeField] private ProductionDefinition _horseCount = new ProductionDefinition("Training Horses", 500, 1);
        [SerializeField] private ProductionDefinition _clickPower = new ProductionDefinition("Click Growth Power", 120, 0.02f);
        [SerializeField] private ProductionDefinition _yield = new ProductionDefinition("Harvest Yield", 160, 1);
        private TierDefinition[] Definitions(EquipmentType type)
        {
            switch (type)
            {
                case EquipmentType.Horseshoes: return _horseshoes;
                case EquipmentType.Saddle: return _saddles;
                case EquipmentType.Bridle: return _bridles;
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}
