using UnityEngine;

namespace Horsecremental.Racing
{
    [RequireComponent(typeof(HorseMovement))]
    public sealed class AIHorseController : MonoBehaviour
    {
        private HorseMovement _movement;
        private float _jumpLeadSeconds;

        private void Awake() => TryGetComponent(out _movement);

        public void Initialize(float jumpLeadSeconds) => _jumpLeadSeconds = jumpLeadSeconds;

        public void Think()
        {
            HorseSimulation horse = _movement.Simulation;
            if (!horse.IsRunning || !horse.IsGrounded) return;
            float distance = horse.NextObstacleDistance - horse.Distance;
            if (distance > 0f && distance <= horse.Speed * _jumpLeadSeconds)
                _movement.TryJump();
        }
    }
}
