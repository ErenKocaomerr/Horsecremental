using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    [RequireComponent(typeof(Button))]
    public sealed class ButtonPulse : MonoBehaviour
    {
        private Button _button;
        private float _remaining;
        private void Awake() { TryGetComponent(out _button); _button.onClick.AddListener(Pulse); }
        public void Pulse() => _remaining = 0.22f;
        private void Update()
        {
            if (_remaining <= 0f) return;
            _remaining = Mathf.Max(0f, _remaining - Time.unscaledDeltaTime);
            transform.localScale = Vector3.one * (1f + Mathf.Sin(_remaining / 0.22f * Mathf.PI) * 0.06f);
        }
        private void OnDisable() { _remaining = 0f; transform.localScale = Vector3.one; }
        private void OnDestroy() { if (_button != null) _button.onClick.RemoveListener(Pulse); }
    }
}
