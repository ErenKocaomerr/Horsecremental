using System;

namespace Horsecremental.Racing
{
    public sealed partial class PlayerProgression
    {
        private readonly int[] _productionLevels = new int[6];
        public int ProductionLevel(ProductionUpgrade upgrade) => _productionLevels[ProductionIndex(upgrade)];
        public ProductionRule ProductionRule(ProductionUpgrade upgrade) => _workshop == null ? default : _workshop.Rule(upgrade);
        public int ProductionCost(ProductionUpgrade upgrade) => _workshop == null ? int.MaxValue : ProductionRule(upgrade).Cost(ProductionLevel(upgrade));
        private float ProductionEffect(ProductionUpgrade upgrade) => ProductionLevel(upgrade) * ProductionRule(upgrade).Effect;
        public float TrainingSeconds(IIdleSettings settings) => Math.Max(1f, settings.TrainingSeconds * Math.Max(0.5f, 1f - ProductionEffect(ProductionUpgrade.TrainingSpeed)));
        public int TrainingCoins(IIdleSettings settings) => (int)Math.Min(int.MaxValue,
            Math.Ceiling(Math.Round(settings.TrainingReward * (1d + Math.Min(0.75d,
                Math.Round(ProductionEffect(ProductionUpgrade.TrainingReward), 6))), 6)));
        public float CarrotSeconds(IIdleSettings settings) => Math.Max(1f, settings.CarrotSeconds * Math.Max(0.5f, 1f - ProductionEffect(ProductionUpgrade.GrowSpeed)));
        public int FarmCapacity(IIdleSettings settings) => (int)Math.Min(int.MaxValue,
            (long)settings.FarmCapacity + (long)ProductionEffect(ProductionUpgrade.FarmCapacity));
        public bool TryUpgradeProduction(ProductionUpgrade upgrade, IIdleSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            int index = ProductionIndex(upgrade);
            if (!IsUnlocked(upgrade)) return false;
            int cost = ProductionCost(upgrade);
            if (cost == int.MaxValue || Coins < cost) return false;
            float trainingBefore = TrainingSeconds(settings);
            float farmBefore = CarrotSeconds(settings);
            Coins -= cost;
            _productionLevels[index]++;
            TrainingElapsed *= TrainingSeconds(settings) / trainingBefore;
            FarmElapsed *= CarrotSeconds(settings) / farmBefore;
            CoinsChanged?.Invoke(-cost);
            Changed?.Invoke();
            return true;
        }
        private static int ProductionIndex(ProductionUpgrade upgrade)
        {
            int index = (int)upgrade;
            if (index < 0 || index > 5) throw new ArgumentOutOfRangeException(nameof(upgrade));
            return index;
        }
    }
}
