using System;

namespace Horsecremental.Racing
{
    // Equipment remains state in the central model; this partial keeps crafting responsibility readable.
    public sealed partial class PlayerProgression
    {
        private readonly IWorkshopSettings _workshop;
        private readonly int[] _equipmentTiers = new int[3];
        public bool IsCrafting { get; private set; }
        public EquipmentType CraftType { get; private set; }
        public int CraftTier { get; private set; }
        public double CraftElapsed { get; private set; }
        public float CraftDuration { get; private set; }
        public float CraftRemaining => IsCrafting ? (float)Math.Max(0d, CraftDuration - CraftElapsed) : 0f;
        public float CraftProgress => IsCrafting ? (float)(CraftElapsed / CraftDuration) : 0f;
        public event Action CraftStarted;
        public event Action<EquipmentType> CraftCompleted;

        public int EquipmentLevel(EquipmentType type) => _equipmentTiers[EquipmentIndex(type)];
        public bool HasNextTier(EquipmentType type) => _workshop != null && EquipmentLevel(type) + 1 < _workshop.TierCount(type);
        public EquipmentTier Equipment(EquipmentType type) => _workshop == null ? default : _workshop.Tier(type, EquipmentLevel(type));
        public EquipmentTier NextEquipment(EquipmentType type) => HasNextTier(type) ? _workshop.Tier(type, EquipmentLevel(type) + 1) : default;
        public EquipmentTier ActiveCraft => IsCrafting ? _workshop.Tier(CraftType, CraftTier) : default;
        public int CraftMetalCost(EquipmentType type) => HasNextTier(type) ?
            Math.Max(1, (int)Math.Ceiling(NextEquipment(type).Metal * Math.Max(0.75d, 1d - ProductionEffect(ProductionUpgrade.MetalEfficiency)))) : 0;
        public float CraftSeconds(EquipmentType type) => HasNextTier(type) ?
            Math.Max(0.1f, NextEquipment(type).Seconds * Math.Max(0.5f, 1f - ProductionEffect(ProductionUpgrade.ForgeSpeed))) : 0f;
        public bool TryStartCraft(EquipmentType type)
        {
            if (!IsUnlocked(UpgradeNode.Blacksmith) || IsCrafting || !HasNextTier(type)) return false;
            EquipmentTier next = NextEquipment(type);
            int metal = CraftMetalCost(type);
            if (Coins < next.Coins || Metal < metal) return false;
            Coins -= next.Coins;
            Metal -= metal;
            CraftType = type;
            CraftTier = EquipmentLevel(type) + 1;
            CraftDuration = CraftSeconds(type);
            CraftElapsed = 0d;
            IsCrafting = true;
            CoinsChanged?.Invoke(-next.Coins);
            MetalChanged?.Invoke(-metal);
            Changed?.Invoke();
            CraftStarted?.Invoke();
            return true;
        }
        public void AdvanceCraft(float seconds)
        {
            if (!IsCrafting || seconds <= 0f || float.IsNaN(seconds) || float.IsInfinity(seconds)) return;
            CraftElapsed = Math.Min(CraftDuration, CraftElapsed + seconds);
            if (CraftElapsed < CraftDuration) return;
            _equipmentTiers[EquipmentIndex(CraftType)] = CraftTier;
            IsCrafting = false;
            Changed?.Invoke();
            CraftCompleted?.Invoke(CraftType);
        }
        public StatBreakdown Breakdown(HorseUpgrade stat)
        {
            int index = Index(stat);
            float basis = stat == HorseUpgrade.Speed ? _baseStats.Speed :
                stat == HorseUpgrade.Acceleration ? _baseStats.Acceleration : _baseStats.BoostPower;
            return new StatBreakdown(basis, (_levels[index] - 1) * _rules[index].Increase,
                stat == HorseUpgrade.Speed ? FeedingSpeed : 0f, Equipment((EquipmentType)index).Bonus);
        }
        public float HorsePower => Stats.Speed * 10f + Stats.Acceleration * 0.25f + Stats.BoostPower * 5f;
        private static int EquipmentIndex(EquipmentType type)
        {
            int index = (int)type;
            if (index < 0 || index > 2) throw new ArgumentOutOfRangeException(nameof(type));
            return index;
        }
    }
}
