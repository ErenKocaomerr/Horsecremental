using System;

namespace Horsecremental.Racing
{
    public sealed partial class PlayerProgression
    {
        private IPlaygroundSettings _playground;
        private IIdleSettings _idle;
        public bool PlaygroundEnabled => _playground != null;
        public TrainingProduction Training { get; private set; }
        public CropGrowth Crops { get; private set; }
        public int TrainingHorseCount => Training == null ? 1 : Training.Count;
        public float ClickGrowth => _playground == null ? 0f : _playground.ClickGrowth + ProductionEffect(ProductionUpgrade.ClickGrowthPower);
        public int HarvestYield => _playground == null ? 0 : _playground.BaseYield + (int)ProductionEffect(ProductionUpgrade.HarvestYield);
        public event Action<int, int> TrainingLapCompleted;
        public event Action<int, int> CropHarvested;
        public void EnablePlayground(IPlaygroundSettings settings, IIdleSettings idle)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (idle == null) throw new ArgumentNullException(nameof(idle));
            if (PlaygroundEnabled) return;
            _playground = settings;
            _idle = idle;
            Training = new TrainingProduction(Math.Min(settings.MaxHorseCount, settings.BaseHorseCount +
                ProductionLevel(ProductionUpgrade.TrainingHorseCount)), settings.MaxHorseCount,
                TrainingElapsed / TrainingSeconds(idle));
            Crops = new CropGrowth(settings);
            if (FarmElapsed > 0d) Crops.Grow(0, FarmElapsed / CarrotSeconds(idle));
            if (!ManualTrainingEnabled) Training.LapCompleted += CompleteLap;
        }
        private void CompleteLap(int horse)
        {
            TrainingRaceNumber++;
            int before = Coins;
            CreditResources(TrainingCoins(_idle), 0);
            TrainingCompleted?.Invoke();
            TrainingLapCompleted?.Invoke(horse, Coins - before);
        }
        public bool GrowCrop(int plot)
        {
            if (!PlaygroundEnabled || !IsUnlocked(UpgradeNode.Farm) || Crops.Stage(plot) == CropStage.Ready) return false;
            Crops.Grow(plot, ClickGrowth);
            return true;
        }
        public bool HarvestCrop(int plot)
        {
            if (!PlaygroundEnabled || !IsUnlocked(UpgradeNode.Farm) || !Crops.Harvest(plot)) return false;
            CropHarvested?.Invoke(plot, HarvestYield);
            return true;
        }
        public int TakeLegacyStoredCarrots()
        {
            if (!PlaygroundEnabled) return 0;
            int amount = StoredCarrots;
            StoredCarrots = 0;
            return amount;
        }
        // Return the credited amount so a full wallet cannot destroy pending pickup value.
        public int CreditPickup(PickupResource resource, long amount)
        {
            if (resource != PickupResource.Carrots) throw new ArgumentOutOfRangeException(nameof(resource));
            if (amount <= 0) return 0;
            int credited = (int)Math.Min(amount, (long)int.MaxValue - Carrots);
            if (credited == 0) return 0;
            Carrots += credited;
            CarrotsChanged?.Invoke(credited);
            Changed?.Invoke();
            return credited;
        }
        private bool InactiveNode(UpgradeNode node) =>
            ManualTrainingEnabled && (node == UpgradeNode.TrainingSpeed || node == UpgradeNode.TrainingReward) ||
            PlaygroundEnabled && (node == UpgradeNode.Blacksmith || node == UpgradeNode.ForgeSpeed ||
                node == UpgradeNode.MetalEfficiency || node == UpgradeNode.FarmCapacity);
        private bool InactiveProduction(ProductionUpgrade upgrade) =>
            ManualTrainingEnabled && (upgrade == ProductionUpgrade.TrainingHorseCount ||
                upgrade == ProductionUpgrade.TrainingSpeed || upgrade == ProductionUpgrade.TrainingReward) ||
            PlaygroundEnabled && (upgrade == ProductionUpgrade.ForgeSpeed || upgrade == ProductionUpgrade.MetalEfficiency ||
                upgrade == ProductionUpgrade.FarmCapacity);
    }
}

