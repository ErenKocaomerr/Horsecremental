using UnityEngine;

namespace Horsecremental.Racing
{
    public sealed class TrainingTrackView : MonoBehaviour
    {
        [SerializeField] private GameplayWorld _world;
        [SerializeField] private Transform[] _horses;
        [SerializeField] private SpriteRenderer[] _renderers;
        [SerializeField] private Sprite[] _idleFrames;
        [SerializeField] private Sprite[] _runFrames;
        [SerializeField] private Vector3[] _path;
        [SerializeField] private WorldFeedback _feedback;
        [SerializeField] private StableAudioHooks _audio;
        private float[] _lengths;
        private float[] _entrance;
        private float _length;
        private float _animation;
        private int _shown;
        private PlayerProgression _player;
        public int VisibleHorseCount => _shown;
        public Vector3 HorsePosition(int index) => _horses[index].position;
        public void Initialize(PlayerProgression player)
        {
            _player = player;
            _lengths = new float[_path.Length];
            _entrance = new float[_horses.Length];
            for (int i = 0; i < _path.Length; i++)
            {
                _length += Vector3.Distance(_path[i], _path[(i + 1) % _path.Length]);
                _lengths[i] = _length;
            }
            _player.TrainingLapCompleted += Complete;
            _player.Changed += RefreshCount;
            RefreshCount();
        }
        private void RefreshCount()
        {
            for (int i = _shown; i < _player.TrainingHorseCount; i++)
            {
                _horses[i].gameObject.SetActive(true);
                _horses[i].position = Sample((float)_player.Training.Phase(i));
                _entrance[i] = 0.5f;
                if (_shown > 0)
                {
                    _feedback.Show(_horses[i].position + Vector3.up, "NEW HORSE!", new Color(1f, 0.9f, 0.4f));
                    _audio.Play(StableSound.NewHorse);
                }
            }
            _shown = _player.TrainingHorseCount;
        }
        private Vector3 Sample(float phase)
        {
            float distance = phase * _length;
            for (int i = 0; i < _path.Length; i++)
            {
                if (distance > _lengths[i]) continue;
                float previous = i == 0 ? 0f : _lengths[i - 1];
                return Vector3.Lerp(_path[i], _path[(i + 1) % _path.Length],
                    (distance - previous) / (_lengths[i] - previous));
            }
            return _path[0];
        }
        private void Complete(int horse, int coins)
        {
            _feedback.Show(_path[0] + new Vector3(-0.2f, 1.2f + horse * 0.14f, 0),
                "+" + coins + " Coins", new Color(1f, 0.85f, 0.3f));
            _audio.Play(StableSound.TrainingLap);
        }
        private void LateUpdate()
        {
            if (_player == null) return;
            if (_world.IsManagement) _animation += Time.deltaTime * 10f * _world.Settings.TrainingSeconds / _player.TrainingSeconds(_world.Settings);
            for (int i = 0; i < _shown; i++)
            {
                float phase = (float)_player.Training.Phase(i);
                Vector3 position = Sample(phase);
                Vector3 next = Sample(Mathf.Repeat(phase + 0.003f, 1f));
                _horses[i].position = position;
                _renderers[i].flipX = next.x < position.x;
                _renderers[i].sortingOrder = 20 - Mathf.RoundToInt(position.y * 2f);
                int variant = i % _idleFrames.Length;
                _renderers[i].sprite = _world.IsManagement ? _runFrames[variant * 6 + (int)_animation % 6] : _idleFrames[variant];
                _entrance[i] = Mathf.Max(0f, _entrance[i] - Time.deltaTime);
                _horses[i].localScale = Vector3.one * Mathf.Lerp(0.35f, 0.75f, 1f - _entrance[i] / 0.5f);
            }
        }
        private void OnDestroy()
        {
            if (_player == null) return;
            _player.TrainingLapCompleted -= Complete;
            _player.Changed -= RefreshCount;
        }
    }
}

