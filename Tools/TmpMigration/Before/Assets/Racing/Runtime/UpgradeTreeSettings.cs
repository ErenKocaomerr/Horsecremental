using System;
using UnityEngine;

namespace Horsecremental.Racing
{
    [CreateAssetMenu(menuName = "Horsecremental/Upgrade Tree Settings")]
    public sealed class UpgradeTreeSettings : ScriptableObject
    {
        [Serializable]
        private sealed class NodeDefinition
        {
            [SerializeField] private UpgradeNode _id;
            [SerializeField] private UpgradeNode _prerequisite;
            [SerializeField] private UpgradeCurrency _currency;
            [SerializeField, Min(0)] private int _cost;
            [SerializeField] private bool _startsUnlocked;
            public NodeDefinition(UpgradeNodeRule rule)
            {
                _id = rule.Id; _prerequisite = rule.Prerequisite; _currency = rule.Currency;
                _cost = rule.Cost; _startsUnlocked = rule.StartsUnlocked;
            }
            public UpgradeNodeRule Rule => new UpgradeNodeRule(_id, _prerequisite, _currency, _cost, _startsUnlocked);
        }
        [SerializeField] private NodeDefinition[] _nodes = Defaults();
        [SerializeField, Min(1)] private int _debugCoins = 100000;
        [SerializeField, Min(1)] private int _debugTokens = 1000;
        public int DebugCoins => _debugCoins;
        public int DebugTokens => _debugTokens;
        public UpgradeTreeRules CreateRules()
        {
            if (_nodes == null) throw new InvalidOperationException("Upgrade tree nodes are missing.");
            UpgradeNodeRule[] rules = new UpgradeNodeRule[_nodes.Length];
            for (int i = 0; i < _nodes.Length; i++)
                rules[i] = (_nodes[i] ?? throw new InvalidOperationException("Upgrade node is missing.")).Rule;
            return new UpgradeTreeRules(rules);
        }
        private static NodeDefinition[] Defaults()
        {
            NodeDefinition[] nodes = new NodeDefinition[UpgradeTreeRules.NODE_COUNT];
            for (int i = 0; i < nodes.Length; i++) nodes[i] = new NodeDefinition(UpgradeTreeRules.DEFAULT.Rule((UpgradeNode)i));
            return nodes;
        }
    }
}
