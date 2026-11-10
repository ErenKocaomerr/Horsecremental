using UnityEngine;

namespace Horsecremental.Racing
{
    public sealed class WorldPickupManager : MonoBehaviour
    {
        private enum Motion { Empty, Dropping, Settled, Collecting }
        [SerializeField] private Transform[] _items;
        [SerializeField] private WorldFeedback _feedback;
        [SerializeField] private StableAudioHooks _audio;
        private Motion[] _motions;
        private Vector3[] _starts;
        private Vector3[] _ends;
        private float[] _elapsed;
        private long[] _values;
        private PickupResource[] _resources;
        private IPlaygroundSettings _settings;
        private PlayerProgression _player;
        public int ActiveCount { get; private set; }
        public long PendingValue
        {
            get { long total = 0; for (int i = 0; i < _values.Length; i++) total += _values[i]; return total; }
        }
        public Vector3 ItemPosition(int index) => _items[index].position;
        public bool IsSettled(int index) => _motions[index] == Motion.Settled;
        public void Initialize(PlayerProgression player, IPlaygroundSettings settings)
        {
            _player = player;
            _settings = settings;
            int count = Mathf.Min(_items.Length, settings.MaxPickups);
            _motions = new Motion[count];
            _starts = new Vector3[count];
            _ends = new Vector3[count];
            _elapsed = new float[count];
            _values = new long[count];
            _resources = new PickupResource[count];
            for (int i = 0; i < _items.Length; i++) _items[i].gameObject.SetActive(false);
        }
        public void Spawn(Vector3 origin, int amount, PickupResource resource)
        {
            if (amount <= 0) return;
            for (int unit = 0; unit < amount; unit++)
            {
                int slot = -1;
                for (int i = 0; i < _motions.Length; i++)
                    if (_motions[i] == Motion.Empty) { slot = i; break; }
                if (slot < 0)
                {
                    // Merge the whole remaining harvest into a nearby same-resource stack.
                    int nearest = -1;
                    float distance = float.MaxValue;
                    for (int i = 0; i < _motions.Length; i++)
                    {
                        if (_resources[i] != resource) continue;
                        float candidate = (_items[i].position - origin).sqrMagnitude;
                        if (candidate < distance) { nearest = i; distance = candidate; }
                    }
                    if (nearest < 0) throw new System.InvalidOperationException("Pickup pool needs a slot for each supported resource.");
                    _values[nearest] += amount - unit;
                    return;
                }
                _motions[slot] = Motion.Dropping;
                _values[slot] = 1;
                _resources[slot] = resource;
                _elapsed[slot] = 0f;
                _starts[slot] = origin;
                Vector2 scatter = Random.insideUnitCircle * _settings.ScatterRadius;
                _ends[slot] = origin + new Vector3(scatter.x, scatter.y * 0.4f - 0.55f, 0f);
                _items[slot].position = origin;
                _items[slot].localScale = Vector3.one * 0.45f;
                _items[slot].gameObject.SetActive(true);
                ActiveCount++;
            }
        }
        public void Sweep(Vector2 from, Vector2 to)
        {
            Vector2 line = to - from;
            float length = line.sqrMagnitude;
            float radius = _settings.HoverRadius;
            for (int i = 0; i < _motions.Length; i++)
            {
                if (_motions[i] != Motion.Settled) continue;
                Vector2 point = _items[i].position;
                float t = length > 0f ? Mathf.Clamp01(Vector2.Dot(point - from, line) / length) : 0f;
                if ((point - (from + line * t)).sqrMagnitude > radius * radius) continue;
                _motions[i] = Motion.Collecting;
                _elapsed[i] = 0f;
                _starts[i] = _items[i].position;
                _ends[i] = new Vector3(to.x, to.y + 0.8f, 0f);
            }
        }
        private void Update()
        {
            if (_player == null) return;
            for (int i = 0; i < _motions.Length; i++)
            {
                Motion motion = _motions[i];
                if (motion == Motion.Empty) continue;
                _elapsed[i] += Time.deltaTime;
                if (motion == Motion.Dropping)
                {
                    float t = Mathf.Clamp01(_elapsed[i] / _settings.DropSeconds);
                    float arc = Mathf.Sin(t * Mathf.PI) * 0.8f;
                    float bounce = t > 0.8f ? Mathf.Sin((t - 0.8f) * Mathf.PI / 0.2f) * 0.12f : 0f;
                    _items[i].position = Vector3.Lerp(_starts[i], _ends[i], t) + Vector3.up * (arc + bounce);
                    if (t == 1f) { _motions[i] = Motion.Settled; _elapsed[i] = 0f; }
                }
                else if (motion == Motion.Settled)
                {
                    _items[i].position = _ends[i] + Vector3.up * (Mathf.Sin(Time.time * 3f + i) * 0.035f);
                    _items[i].localScale = Vector3.one * (0.45f + Mathf.Min(0.3f, (_values[i] - 1) * 0.04f));
                }
                else
                {
                    float t = Mathf.Clamp01(_elapsed[i] / _settings.CollectionSeconds);
                    _items[i].position = Vector3.Lerp(_starts[i], _ends[i], t * t);
                    _items[i].localScale = Vector3.one * (0.55f * (1f - t));
                    if (t < 1f) continue;
                    int credited = _player.CreditPickup(_resources[i], _values[i]);
                    _values[i] -= credited;
                    if (credited > 0)
                    {
                        _feedback.Show(_ends[i], "+" + credited + " Carrots", new Color(0.7f, 1f, 0.45f));
                        _audio.Play(StableSound.Pickup);
                    }
                    if (_values[i] == 0)
                    {
                        _motions[i] = Motion.Empty;
                        _items[i].gameObject.SetActive(false);
                        ActiveCount--;
                    }
                    else
                    {
                        _motions[i] = Motion.Settled;
                        _ends[i] = _starts[i];
                        _elapsed[i] = 0f;
                    }
                }
            }
        }
    }
}

