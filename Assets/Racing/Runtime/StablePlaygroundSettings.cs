using UnityEngine;

namespace Horsecremental.Racing
{
    [CreateAssetMenu(menuName = "Horsecremental/Stable Playground Settings")]
    public sealed class StablePlaygroundSettings : ScriptableObject, IPlaygroundSettings
    {
        [SerializeField, Range(1, 6)] private int _baseHorseCount = 1;
        [SerializeField, Range(1, 6)] private int _maxHorseCount = 4;
        [SerializeField, Range(4, 8)] private int _plotCount = 6;
        [SerializeField, Range(0f, 0.8f)] private float _initialCropSpread = 0.45f;
        [SerializeField, Range(0.1f, 0.5f)] private float _midThreshold = 0.3f;
        [SerializeField, Range(0.51f, 0.95f)] private float _largeThreshold = 0.65f;
        [SerializeField, Range(0.01f, 0.5f)] private float _clickGrowth = 0.1f;
        [SerializeField, Min(0.1f)] private float _clickCooldown = 0.25f;
        [SerializeField, Range(1, 5)] private int _baseYield = 2;
        [SerializeField, Range(10, 100)] private int _maxPickups = 80;
        [SerializeField, Min(0.1f)] private float _dropSeconds = 0.45f;
        [SerializeField, Min(0.1f)] private float _collectionSeconds = 0.2f;
        [SerializeField, Range(0.1f, 1.5f)] private float _scatterRadius = 0.65f;
        [SerializeField, Range(0.1f, 0.8f)] private float _hoverRadius = 0.36f;
        [SerializeField, Range(0f, 0.5f)] private float _raceBlend = 0.15f;
        public int BaseHorseCount => Mathf.Clamp(_baseHorseCount, 1, MaxHorseCount);
        public int MaxHorseCount => Mathf.Clamp(_maxHorseCount, 1, 6);
        public int PlotCount => Mathf.Clamp(_plotCount, 4, 8);
        public float InitialCropSpread => _initialCropSpread;
        public float MidThreshold => _midThreshold;
        public float LargeThreshold => _largeThreshold;
        public float ClickGrowth => _clickGrowth;
        public float ClickCooldown => Mathf.Max(0.1f, _clickCooldown);
        public int BaseYield => Mathf.Max(1, _baseYield);
        public int MaxPickups => Mathf.Clamp(_maxPickups, 10, 100);
        public float DropSeconds => Mathf.Max(0.1f, _dropSeconds);
        public float CollectionSeconds => Mathf.Max(0.1f, _collectionSeconds);
        public float ScatterRadius => _scatterRadius;
        public float HoverRadius => _hoverRadius;
        public float RaceBlend => _raceBlend;
    }
}

