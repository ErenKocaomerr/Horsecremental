using System;
using UnityEngine;

namespace Horsecremental.Racing
{
    [CreateAssetMenu(menuName = "Horsecremental/Progression Settings")]
    public sealed class ProgressionSettings : ScriptableObject
    {
        [Serializable]
        private sealed class UpgradeConfiguration
        {
            [SerializeField, Min(1)] private int _baseCost = 60;
            [SerializeField, Min(1.01f)] private float _growthRate = 1.45f;
            [SerializeField, Min(0.01f)] private float _increase = 0.75f;
            public UpgradeRule Rule => new UpgradeRule(_baseCost, _growthRate, _increase);
        }

        [SerializeField] private HorseStats _baseHorse;
        [SerializeField] private WorkshopSettings _workshop;
        [SerializeField] private UpgradeTreeSettings _upgradeTree;
        [SerializeField] private int[] _placeTokenRewards = { 5, 3, 2, 1 };
        [SerializeField] private int[] _placeMetalRewards = { 15, 10, 6, 3 };
        [SerializeField] private UpgradeConfiguration _speed = new UpgradeConfiguration();
        [SerializeField] private UpgradeConfiguration _acceleration = new UpgradeConfiguration();
        [SerializeField] private UpgradeConfiguration _boost = new UpgradeConfiguration();
        [SerializeField] private int[] _placeRewards = { 100, 70, 50, 30 };
        public HorseStats BaseHorse => _baseHorse;
        public WorkshopSettings Workshop => _workshop;
        public UpgradeTreeSettings UpgradeTree => _upgradeTree;

        public PlayerProgression CreatePlayer()
        {
            if (_baseHorse == null) throw new InvalidOperationException("Progression needs the player's base HorseStats.");
            return new PlayerProgression(_baseHorse.Parameters, _speed.Rule, _acceleration.Rule, _boost.Rule, _workshop,
                _upgradeTree != null ? _upgradeTree.CreateRules() : UpgradeTreeRules.DEFAULT);
        }

        public int RewardForPlace(int position)
        {
            if (position < 1) throw new ArgumentOutOfRangeException(nameof(position));
            return _placeRewards != null && position <= _placeRewards.Length
                ? Math.Max(0, _placeRewards[position - 1]) : 0;
        }

        private void OnValidate()
        {
            ClampRewards(_placeRewards); ClampRewards(_placeMetalRewards); ClampRewards(_placeTokenRewards);
        }
        private static void ClampRewards(int[] rewards)
        {
            if (rewards == null) return;
            for (int i = 0; i < rewards.Length; i++) rewards[i] = Math.Max(0, rewards[i]);
        }
        public int TokensForPlace(int position)
        {
            if (position < 1) throw new ArgumentOutOfRangeException(nameof(position));
            return _placeTokenRewards != null && position <= _placeTokenRewards.Length
                ? Math.Max(0, _placeTokenRewards[position - 1]) : 0;
        }
        public int MetalForPlace(int position)
        {
            if (position < 1) throw new ArgumentOutOfRangeException(nameof(position));
            return _placeMetalRewards != null && position <= _placeMetalRewards.Length
                ? Math.Max(0, _placeMetalRewards[position - 1]) : 0;
        }
    }
}
