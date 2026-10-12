using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    [RequireComponent(typeof(Text))]
    public sealed class TransientUIFeedback : MonoBehaviour
    {
        private Text _text;
        private string _previous;
        private float _remaining;
        private void Awake() => TryGetComponent(out _text);
        private void LateUpdate()
        {
            if (_text.text != _previous)
            {
                _previous = _text.text;
                _remaining = 2.5f;
            }
            if (_remaining <= 0f || _previous.Length == 0) return;
            _remaining -= Time.unscaledDeltaTime;
            if (_remaining <= 0f) { _text.text = string.Empty; _previous = string.Empty; }
        }
    }
}
