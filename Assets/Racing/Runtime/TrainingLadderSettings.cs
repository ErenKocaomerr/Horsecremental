using System;
using UnityEngine;

namespace Horsecremental.Racing
{
    [CreateAssetMenu(menuName = "Horsecremental/Training Ladder")]
    public sealed class TrainingLadderSettings : ScriptableObject
    {
        [Serializable]
        private sealed class Tier
        {
            [SerializeField] private string _name;
            [SerializeField] private RaceSettings _race;
            [SerializeField] private int[] _coins;
            [SerializeField] private int[] _tokens;
            public string Name => _name;
            public RaceSettings Race => _race;
            public TrainingRaceRules Rules => new TrainingRaceRules(_coins, _tokens);
        }
        [SerializeField] private Tier[] _tiers;
        [SerializeField] private int[] _horseUpgradeTokenCosts = { 3, 2, 3 };
        [SerializeField, Min(1.01f)] private float _horseUpgradeCostGrowth = 1.35f;
        public int Count => _tiers == null ? 0 : _tiers.Length;
        public string Name(int tier) => GetTier(tier).Name;
        public RaceSettings Race(int tier) => GetTier(tier).Race;
        public TrainingRaceRules Rules(int tier) => GetTier(tier).Rules;
        public void Enable(PlayerProgression player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (Count != 3) throw new InvalidOperationException("This demo requires three Training tiers.");
            TrainingRaceRules[] rules = new TrainingRaceRules[Count];
            for (int i = 0; i < Count; i++)
            {
                Tier tier = GetTier(i);
                if (string.IsNullOrWhiteSpace(tier.Name) || tier.Race == null || tier.Race.OpponentStats.Length != 3)
                    throw new InvalidOperationException("Each Training tier needs a name, a race and three fixed rivals.");
                rules[i] = tier.Rules;
            }
            player.EnableManualTraining(rules, _horseUpgradeTokenCosts, _horseUpgradeCostGrowth);
        }
        private Tier GetTier(int tier)
        {
            if (tier < 0 || tier >= Count) throw new ArgumentOutOfRangeException(nameof(tier));
            return _tiers[tier] ?? throw new InvalidOperationException("Training tier is missing.");
        }
    }
}
