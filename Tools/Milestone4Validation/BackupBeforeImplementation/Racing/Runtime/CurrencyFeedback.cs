using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    public sealed class CurrencyFeedback : MonoBehaviour
    {
        [SerializeField] private ProgressionSettings _settings;
        [SerializeField] private Text _total;
        [SerializeField] private Text _popup;
        private PlayerProgression _player;
        private double _displayed;
        private int _lastDisplayed = -1;
        private float _remaining;
        private Vector2 _origin;
        private void Awake()
        {
            _player = ProgressionSession.GetOrCreate(_settings);
            _displayed = _player.Coins;
            _origin = _popup.rectTransform.anchoredPosition;
            _player.CoinsChanged += OnCoins;
            RefreshCounter();
            InvokeRepeating(nameof(RefreshCounter), 0f, 0.05f);
        }
        private void OnCoins(int delta)
        {
            _popup.text = (delta > 0 ? "+" : string.Empty) + delta + " COINS";
            _popup.color = delta > 0 ? new Color(1f, 0.85f, 0.25f) : new Color(1f, 0.5f, 0.35f);
            _remaining = 1.4f;
        }
        private void RefreshCounter()
        {
            int value = (int)System.Math.Round(_displayed);
            if (value == _lastDisplayed) return;
            _lastDisplayed = value;
            _total.text = "COINS  " + value;
        }
        private void Update()
        {
            _displayed += (_player.Coins - _displayed) * (1d - System.Math.Exp(-10d * Time.unscaledDeltaTime));
            if (System.Math.Abs(_displayed - _player.Coins) < 0.5d) _displayed = _player.Coins;
            _remaining = Mathf.Max(0f, _remaining - Time.unscaledDeltaTime);
            Color color = _popup.color;
            color.a = Mathf.Clamp01(_remaining * 2f);
            _popup.color = color;
            _popup.rectTransform.anchoredPosition = _origin + Vector2.up * (1.4f - _remaining) * 22f;
        }
        private void OnDestroy() { if (_player != null) _player.CoinsChanged -= OnCoins; }
    }
}
