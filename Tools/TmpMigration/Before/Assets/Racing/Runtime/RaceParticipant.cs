using UnityEngine;

namespace Horsecremental.Racing
{
    [RequireComponent(typeof(HorseMovement))]
    public sealed class RaceParticipant : MonoBehaviour
    {
        [SerializeField] private AIHorseController _ai;
        [SerializeField] private string _displayName;
        [SerializeField] private bool _isPlayer;
        public HorseMovement Movement { get; private set; }
        public AIHorseController AI => _ai;
        public string DisplayName => _displayName;
        public bool IsPlayer => _isPlayer;
        public int FinishPosition { get; private set; }
        public float FinishTime { get; private set; }
        public bool HasFinished => FinishPosition > 0;

        private void Awake()
        {
            TryGetComponent(out HorseMovement movement);
            Movement = movement;
        }

        public void Initialize(string displayName, bool isPlayer, AIHorseController ai)
        {
            _displayName = displayName;
            _isPlayer = isPlayer;
            _ai = ai;
        }

        public void ResetParticipant()
        {
            FinishPosition = 0;
            FinishTime = 0f;
            Movement.ResetHorse();
        }

        public void Finish(int position, float time, float trackLength)
        {
            if (HasFinished) return;
            FinishPosition = position;
            FinishTime = time;
            Movement.Simulation.Finish(trackLength);
            Movement.SynchronizePosition();
        }
    }
}
