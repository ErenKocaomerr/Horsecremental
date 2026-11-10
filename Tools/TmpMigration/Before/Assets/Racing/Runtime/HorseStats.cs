using UnityEngine;

namespace Horsecremental.Racing
{
    [CreateAssetMenu(menuName = "Horsecremental/Horse Stats")]
    public sealed class HorseStats : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float _speed = 10f;
        [SerializeField, Min(0.1f)] private float _acceleration = 12f;
        [SerializeField, Min(0f)] private float _boostPower = 5f;
        [SerializeField, Range(0.01f, 0.15f)] private float _boostPerTap = 0.06f;
        [SerializeField, Min(0.001f)] private float _boostDecayPerSecond = 0.02f;
        [SerializeField, Min(0f)] private float _boostHighDrain = 0.58f;
        [SerializeField, Min(0.1f)] private float _jumpPower = 7f;
        [SerializeField, Min(0.1f)] private float _gravity = 20f;
        [SerializeField, Range(0.1f, 0.9f)] private float _penaltyMultiplier = 0.45f;
        [SerializeField, Min(0.1f)] private float _penaltyDuration = 1.2f;

        public float BoostPower => _boostPower;
        public HorseParameters Parameters => new HorseParameters(_speed, _acceleration, _boostPower,
            _boostPerTap, _boostDecayPerSecond, _jumpPower, _gravity, _penaltyMultiplier, _penaltyDuration, _boostHighDrain);
    }
}
