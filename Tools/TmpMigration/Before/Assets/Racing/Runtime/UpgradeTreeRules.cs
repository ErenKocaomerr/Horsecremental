using System;

namespace Horsecremental.Racing
{
    public enum UpgradeNode
    {
        None = -1, Speed, Acceleration, BoostPower, Farm, Blacksmith,
        TrainingSpeed, TrainingReward, GrowSpeed, FarmCapacity, ForgeSpeed, MetalEfficiency
    }

    public enum UpgradeCurrency { Coins, RaceTokens }

    public readonly struct UpgradeNodeRule
    {
        public UpgradeNode Id { get; }
        public UpgradeNode Prerequisite { get; }
        public UpgradeCurrency Currency { get; }
        public int Cost { get; }
        public bool StartsUnlocked { get; }

        public UpgradeNodeRule(UpgradeNode id, UpgradeNode prerequisite, UpgradeCurrency currency,
            int cost, bool startsUnlocked = false)
        {
            UpgradeTreeRules.Index(id);
            if (prerequisite != UpgradeNode.None) UpgradeTreeRules.Index(prerequisite);
            if (currency != UpgradeCurrency.Coins && currency != UpgradeCurrency.RaceTokens)
                throw new ArgumentOutOfRangeException(nameof(currency));
            if (cost < 0) throw new ArgumentOutOfRangeException(nameof(cost));
            Id = id; Prerequisite = prerequisite; Currency = currency;
            Cost = cost; StartsUnlocked = startsUnlocked;
        }
    }

    // Immutable rules, independent of Unity; each player owns separate unlock state.
    public sealed class UpgradeTreeRules
    {
        public const int NODE_COUNT = 11;
        public static readonly UpgradeTreeRules DEFAULT = new UpgradeTreeRules(new[] {
            new UpgradeNodeRule(UpgradeNode.Speed, UpgradeNode.None, UpgradeCurrency.Coins, 0, true),
            new UpgradeNodeRule(UpgradeNode.Acceleration, UpgradeNode.Speed, UpgradeCurrency.Coins, 80),
            new UpgradeNodeRule(UpgradeNode.BoostPower, UpgradeNode.Acceleration, UpgradeCurrency.Coins, 150),
            new UpgradeNodeRule(UpgradeNode.Farm, UpgradeNode.Speed, UpgradeCurrency.RaceTokens, 5),
            new UpgradeNodeRule(UpgradeNode.Blacksmith, UpgradeNode.Farm, UpgradeCurrency.RaceTokens, 10),
            new UpgradeNodeRule(UpgradeNode.TrainingSpeed, UpgradeNode.Acceleration, UpgradeCurrency.Coins, 150),
            new UpgradeNodeRule(UpgradeNode.TrainingReward, UpgradeNode.TrainingSpeed, UpgradeCurrency.Coins, 180),
            new UpgradeNodeRule(UpgradeNode.GrowSpeed, UpgradeNode.Farm, UpgradeCurrency.Coins, 120),
            new UpgradeNodeRule(UpgradeNode.FarmCapacity, UpgradeNode.GrowSpeed, UpgradeCurrency.Coins, 100),
            new UpgradeNodeRule(UpgradeNode.ForgeSpeed, UpgradeNode.Blacksmith, UpgradeCurrency.Coins, 150),
            new UpgradeNodeRule(UpgradeNode.MetalEfficiency, UpgradeNode.ForgeSpeed, UpgradeCurrency.Coins, 200)
        });
        private readonly UpgradeNodeRule[] _nodes;

        public UpgradeTreeRules(UpgradeNodeRule[] nodes)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (nodes.Length != NODE_COUNT) throw new ArgumentException("Every upgrade node must be configured.", nameof(nodes));
            _nodes = new UpgradeNodeRule[NODE_COUNT];
            bool[] seen = new bool[NODE_COUNT];
            for (int i = 0; i < nodes.Length; i++)
            {
                int index = Index(nodes[i].Id);
                if (seen[index]) throw new ArgumentException("Duplicate upgrade node.", nameof(nodes));
                seen[index] = true; _nodes[index] = nodes[i];
            }
            for (int i = 0; i < NODE_COUNT; i++)
            {
                UpgradeNode parent = _nodes[i].Prerequisite;
                int depth = 0;
                while (parent != UpgradeNode.None)
                {
                    if (++depth >= NODE_COUNT) throw new ArgumentException("Upgrade prerequisites contain a cycle.", nameof(nodes));
                    if (_nodes[i].StartsUnlocked && !_nodes[Index(parent)].StartsUnlocked)
                        throw new ArgumentException("An initial unlock needs unlocked prerequisites.", nameof(nodes));
                    parent = _nodes[Index(parent)].Prerequisite;
                }
            }
        }

        public UpgradeNodeRule Rule(UpgradeNode node) => _nodes[Index(node)];
        public static int Index(UpgradeNode node)
        {
            int index = (int)node;
            if (index < 0 || index >= NODE_COUNT) throw new ArgumentOutOfRangeException(nameof(node));
            return index;
        }
        public static UpgradeNode NodeFor(HorseUpgrade upgrade)
        {
            int index = (int)upgrade;
            if (index < 0 || index > 2) throw new ArgumentOutOfRangeException(nameof(upgrade));
            return (UpgradeNode)index;
        }
        public static UpgradeNode NodeFor(ProductionUpgrade upgrade)
        {
            switch (upgrade)
            {
                case ProductionUpgrade.ForgeSpeed: return UpgradeNode.ForgeSpeed;
                case ProductionUpgrade.MetalEfficiency: return UpgradeNode.MetalEfficiency;
                case ProductionUpgrade.GrowSpeed: return UpgradeNode.GrowSpeed;
                case ProductionUpgrade.FarmCapacity: return UpgradeNode.FarmCapacity;
                case ProductionUpgrade.TrainingSpeed: return UpgradeNode.TrainingSpeed;
                case ProductionUpgrade.TrainingReward: return UpgradeNode.TrainingReward;
                case ProductionUpgrade.TrainingHorseCount: return UpgradeNode.Speed;
                case ProductionUpgrade.ClickGrowthPower: return UpgradeNode.Farm;
                case ProductionUpgrade.HarvestYield: return UpgradeNode.Farm;
                default: throw new ArgumentOutOfRangeException(nameof(upgrade));
            }
        }
        public static string Name(UpgradeNode node)
        {
            switch (node)
            {
                case UpgradeNode.Speed: return "Speed";
                case UpgradeNode.Acceleration: return "Acceleration";
                case UpgradeNode.BoostPower: return "Boost Power";
                case UpgradeNode.Farm: return "Unlock Farm";
                case UpgradeNode.Blacksmith: return "Unlock Blacksmith";
                case UpgradeNode.TrainingSpeed: return "Training Speed";
                case UpgradeNode.TrainingReward: return "Training Reward";
                case UpgradeNode.GrowSpeed: return "Grow Speed";
                case UpgradeNode.FarmCapacity: return "Farm Capacity";
                case UpgradeNode.ForgeSpeed: return "Forge Speed";
                case UpgradeNode.MetalEfficiency: return "Metal Efficiency";
                default: throw new ArgumentOutOfRangeException(nameof(node));
            }
        }
    }
}
