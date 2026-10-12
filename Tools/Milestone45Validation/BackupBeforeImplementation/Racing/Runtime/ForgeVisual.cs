using UnityEngine;

namespace Horsecremental.Racing
{
    public sealed class ForgeVisual : MonoBehaviour
    {
        [SerializeField] private ProgressionSettings _settings;
        [SerializeField] private SpriteRenderer _glow;
        [SerializeField] private Transform _hammer;
        [SerializeField] private Transform[] _sparks;
        private PlayerProgression _player;
        private float _burst;
        private void Awake()
        {
            _player = ProgressionSession.GetOrCreate(_settings);
            _player.CraftCompleted += Complete;
        }
        private void Complete(EquipmentType type) => _burst = 0.7f;
        private void Update()
        {
            _burst = Mathf.Max(0f, _burst - Time.deltaTime);
            bool working = _player.IsCrafting;
            float wave = working ? (1f + Mathf.Sin(Time.time * 13f)) * 0.5f : 0f;
            _glow.color = new Color(1f, 0.35f + wave * 0.25f, 0.08f, 0.25f + wave * 0.5f + _burst * 0.3f);
            _hammer.localRotation = Quaternion.Euler(0f, 0f, working ? -20f + wave * 65f : 20f);
            for (int i = 0; i < _sparks.Length; i++)
            {
                bool active = working || _burst > 0f;
                if (_sparks[i].gameObject.activeSelf != active) _sparks[i].gameObject.SetActive(active);
                if (active) _sparks[i].localPosition = new Vector3((i - 1) * 0.3f, 0.5f + Mathf.Repeat(Time.time * 2f + i * 0.3f, 0.8f), 0f);
            }
        }
        private void OnDestroy() { if (_player != null) _player.CraftCompleted -= Complete; }
    }
}
