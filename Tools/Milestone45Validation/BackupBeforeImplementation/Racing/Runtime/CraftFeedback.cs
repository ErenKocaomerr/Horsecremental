using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    public sealed class CraftFeedback : MonoBehaviour
    {
        [SerializeField] private ProgressionSettings _settings;
        [SerializeField] private Text _banner;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private UnityEvent _onCraftComplete = new UnityEvent();
        private PlayerProgression _player;
        private float _remaining;
        private void Awake()
        {
            _player = ProgressionSession.GetOrCreate(_settings);
            _player.CraftCompleted += Complete;
        }
        private void Complete(EquipmentType type)
        {
            EquipmentTier item = _player.Equipment(type);
            _banner.text = item.Name + " equipped!";
            _remaining = 3.5f;
            _onCraftComplete.Invoke();
        }
        private void Update()
        {
            _remaining = Mathf.Max(0f, _remaining - Time.unscaledDeltaTime);
            _group.alpha = Mathf.Clamp01(_remaining);
            _banner.transform.localScale = Vector3.one * (1f + Mathf.Max(0f, _remaining - 3.2f) * 0.4f);
        }
        private void OnDestroy() { if (_player != null) _player.CraftCompleted -= Complete; }
    }
}
