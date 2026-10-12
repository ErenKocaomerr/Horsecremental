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
        [SerializeField] private int[] _placeMetalRewards = { 15, 10, 6, 3 };
        [SerializeField] private UpgradeConfiguration _speed = new UpgradeConfiguration();
        [SerializeField] private UpgradeConfiguration _acceleration = new UpgradeConfiguration();
        [SerializeField] private UpgradeConfiguration _boost = new UpgradeConfiguration();
        [SerializeField] private int[] _placeRewards = { 100, 70, 50, 30 };
        public HorseStats BaseHorse => _baseHorse;
        public WorkshopSettings Workshop => _workshop;

        public PlayerProgression CreatePlayer()
        {
            if (_baseHorse == null) throw new InvalidOperationException("Progression needs the player's base HorseStats.");
            return new PlayerProgression(_baseHorse.Parameters, _speed.Rule, _acceleration.Rule, _boost.Rule, _workshop);
        }

        public int RewardForPlace(int position)
        {
            if (position < 1) throw new ArgumentOutOfRangeException(nameof(position));
            return _placeRewards != null && position <= _placeRewards.Length
                ? Math.Max(0, _placeRewards[position - 1]) : 0;
        }

        private void OnValidate()
        {
            if (_placeRewards == null) return;
            for (int i = 0; i < _placeRewards.Length; i++) _placeRewards[i] = Math.Max(0, _placeRewards[i]);
        }
        public int MetalForPlace(int position)
        {
            if (position < 1) throw new ArgumentOutOfRangeException(nameof(position));
            return _placeMetalRewards != null && position <= _placeMetalRewards.Length
                ? Math.Max(0, _placeMetalRewards[position - 1]) : 0;
        }
    }
}
