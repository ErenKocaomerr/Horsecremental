using UnityEngine;

namespace Horsecremental.Racing
{
    // Interpolate before CinemachineBrain (order 100) reads the visual follow target.
    [DefaultExecutionOrder(50)]
    [RequireComponent(typeof(HorseMovement))]
    public sealed class HorseView : MonoBehaviour
    {
        private HorseMovement _movement;
        [SerializeField] private Transform _renderRoot;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Sprite _idle;
        [SerializeField] private Sprite[] _runFrames;
        private float _animationTime;
        public Transform RenderRoot => _renderRoot;

        private void Awake() => TryGetComponent(out _movement);

        public void Snap()
        {
            _animationTime = 0f;
            Render(1f);
            _renderer.sprite = _idle;
            _renderer.color = Color.white;
        }

        public void Render(float interpolation)
        {
            if (_movement == null || _movement.Simulation == null) return;
            HorseSimulation horse = _movement.Simulation;
            float fraction = Mathf.Clamp01(interpolation);
            _renderRoot.position = new Vector3(Mathf.Lerp(horse.PreviousDistance, horse.Distance, fraction), transform.position.y, 0f);
            _renderer.transform.localPosition = new Vector3(0f, Mathf.Lerp(horse.PreviousHeight, horse.Height, fraction), 0f);
        }

        private void LateUpdate()
        {
            if (_movement == null || _movement.Simulation == null) return;
            HorseSimulation horse = _movement.Simulation;
            Render(horse.IsRunning ? (Time.time - Time.fixedTime) / Time.fixedDeltaTime : 1f);
            if (horse.IsRunning)
            {
                _animationTime += Time.deltaTime * Mathf.Max(4f, horse.Speed);
                _renderer.sprite = _runFrames[(int)_animationTime % _runFrames.Length];
            }
            else
            {
                _animationTime = 0f;
                _renderer.sprite = _idle;
            }
            _renderer.color = horse.IsPenalized ? new Color(1f, 0.4f, 0.4f) :
                horse.IsBoosting ? new Color(1f, 1f, 0.35f) : Color.white;
        }
    }
}
