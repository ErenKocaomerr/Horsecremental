using UnityEngine;

namespace Horsecremental.Racing
{
    [CreateAssetMenu(menuName = "Horsecremental/World Settings")]
    public sealed class WorldSettings : ScriptableObject, IIdleSettings
    {
        [SerializeField, Min(1f)] private float _trainingSeconds = 15f;
        [SerializeField, Min(1)] private int _trainingReward = 35;
        [SerializeField, Min(1f)] private float _carrotSeconds = 20f;
        [SerializeField, Min(1)] private int _farmCapacity = 10;
        [SerializeField, Min(1)] private int _feedCost = 5;
        [SerializeField, Min(0.01f)] private float _feedSpeed = 0.1f;
        [SerializeField, Range(0.5f, 1.5f)] private float _cameraBlend = 0.9f;
        [SerializeField, Min(1f)] private float _recommendedPower = 150f;
        public float TrainingSeconds => Mathf.Max(1f, _trainingSeconds);
        public int TrainingReward => Mathf.Max(1, _trainingReward);
        public float CarrotSeconds => Mathf.Max(1f, _carrotSeconds);
        public int FarmCapacity => Mathf.Max(1, _farmCapacity);
        public int FeedCost => Mathf.Max(1, _feedCost);
        public float FeedSpeed => Mathf.Max(0.01f, _feedSpeed);
        public float CameraBlend => _cameraBlend;
        public float RecommendedPower => _recommendedPower;
    }
}
