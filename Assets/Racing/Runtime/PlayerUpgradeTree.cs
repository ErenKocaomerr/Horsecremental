using System;

namespace Horsecremental.Racing
{
    public sealed partial class PlayerProgression
    {
        private readonly UpgradeTreeRules _tree;
        private readonly bool[] _unlockedNodes = new bool[UpgradeTreeRules.NODE_COUNT];
        public int RaceTokens { get; private set; }
        public event Action<int> RaceTokensChanged;
        public UpgradeNodeRule NodeRule(UpgradeNode node) => _tree.Rule(node);
        public bool IsUnlocked(UpgradeNode node) => _unlockedNodes[UpgradeTreeRules.Index(node)];
        public bool IsUnlocked(HorseUpgrade upgrade) => IsUnlocked(UpgradeTreeRules.NodeFor(upgrade));
        public bool IsUnlocked(ProductionUpgrade upgrade) => IsUnlocked(UpgradeTreeRules.NodeFor(upgrade));
        public bool HasPrerequisite(UpgradeNode node)
        {
            UpgradeNode parent = NodeRule(node).Prerequisite;
            return parent == UpgradeNode.None || IsUnlocked(parent);
        }
        public bool CanUnlock(UpgradeNode node)
        {
            if (InactiveNode(node)) return false;
            if (IsUnlocked(node) || !HasPrerequisite(node)) return false;
            UpgradeNodeRule rule = NodeRule(node);
            return (rule.Currency == UpgradeCurrency.Coins ? Coins : RaceTokens) >= rule.Cost;
        }
        public bool TryUnlock(UpgradeNode node)
        {
            if (!CanUnlock(node)) return false;
            UpgradeNodeRule rule = NodeRule(node);
            _unlockedNodes[UpgradeTreeRules.Index(node)] = true;
            if (rule.Currency == UpgradeCurrency.Coins)
            {
                Coins -= rule.Cost;
                if (rule.Cost > 0) CoinsChanged?.Invoke(-rule.Cost);
            }
            else
            {
                RaceTokens -= rule.Cost;
                if (rule.Cost > 0) RaceTokensChanged?.Invoke(-rule.Cost);
            }
            Changed?.Invoke();
            return true;
        }
        // Debug grants do not open, finish, or consume a race reward claim.
        public void GrantDebugResources(int coins, int tokens)
        {
            if (coins < 0) throw new ArgumentOutOfRangeException(nameof(coins));
            if (tokens < 0) throw new ArgumentOutOfRangeException(nameof(tokens));
            CreditResources(coins, 0, tokens);
        }
    }
}
