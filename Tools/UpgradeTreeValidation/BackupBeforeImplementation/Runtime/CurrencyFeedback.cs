using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    public sealed class CurrencyFeedback : MonoBehaviour
    {
        [SerializeField] private ProgressionSettings _settings;
        [SerializeField] private Text _total;
        [SerializeField] private Text _popup;
        [SerializeField] private Text _metal;
        [SerializeField] private Text _metalPopup;
        [SerializeField] private Text _carrots;
        [SerializeField] private Text _carrotPopup;
        [SerializeField] private WorldSettings _worldSettings;
        private PlayerProgression _player;
        private double _displayed;
        private int _lastDisplayed = -1;
        private float _remaining;
        private Vector2 _origin;
        private float _metalRemaining;
        private float _carrotRemaining;
        private Vector2 _metalOrigin;
        private Vector2 _carrotOrigin;
        private double _displayedMetal;
        private int _lastMetal = -1;
        private void Awake()
        {
            _player = ProgressionSession.GetOrCreate(_settings);
            _displayed = _player.Coins;
            _origin = _popup.rectTransform.anchoredPosition;
            _player.CoinsChanged += OnCoins;
            _player.MetalChanged += OnMetal;
            _player.CarrotsChanged += OnCarrots;
            _player.Changed += RefreshCarrots;
            _displayedMetal = _player.Metal;
            if (_metalPopup != null) _metalOrigin = _metalPopup.rectTransform.anchoredPosition;
            if (_carrotPopup != null) _carrotOrigin = _carrotPopup.rectTransform.anchoredPosition;
            RefreshCarrots();
            RefreshCounter();
            InvokeRepeating(nameof(RefreshCounter), 0f, 0.05f);
        }
        private void OnMetal(int delta)
        {
            if (_metalPopup == null) return;
            _metalPopup.text = (delta > 0 ? "+" : string.Empty) + delta + " METAL";
            _metalPopup.color = delta > 0 ? new Color(0.65f, 0.85f, 1f) : new Color(1f, 0.5f, 0.35f);
            _metalRemaining = 1.4f;
        }
        private void OnCarrots(int delta)
        {
            if (_carrotPopup == null) return;
            _carrotPopup.text = (delta > 0 ? "+" : string.Empty) + delta + " CARROTS";
            _carrotPopup.color = delta > 0 ? new Color(0.7f, 1f, 0.45f) : new Color(1f, 0.5f, 0.35f);
            _carrotRemaining = 1.4f;
        }
        private void RefreshCarrots()
        {
            if (_carrots == null || _worldSettings == null) return;
            _carrots.text = "CARROTS  " + _player.Carrots + "  |  FARM  " + _player.StoredCarrots + " / " + _player.FarmCapacity(_worldSettings);
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
            if (value != _lastDisplayed) { _lastDisplayed = value; _total.text = "COINS  " + value.ToString("N0"); }
            int metal = (int)System.Math.Round(_displayedMetal);
            if (_metal != null && metal != _lastMetal) { _lastMetal = metal; _metal.text = "METAL  " + metal.ToString("N0"); }
        }
        private void Update()
        {
            _displayed += (_player.Coins - _displayed) * (1d - System.Math.Exp(-10d * Time.unscaledDeltaTime));
            if (System.Math.Abs(_displayed - _player.Coins) < 0.5d) _displayed = _player.Coins;
            _remaining = Mathf.Max(0f, _remaining - Time.unscaledDeltaTime);
            Color color = _popup.color;
            color.a = Mathf.Clamp01(_remaining * 2f);
            _popup.color = color;
            _popup.rectTransform.anchoredPosition = _origin + Vector2.down * (1.4f - _remaining) * 8f;
            _displayedMetal += (_player.Metal - _displayedMetal) * (1d - System.Math.Exp(-10d * Time.unscaledDeltaTime));
            if (System.Math.Abs(_displayedMetal - _player.Metal) < 0.5d) _displayedMetal = _player.Metal;
            AnimatePopup(_metalPopup, _metalOrigin, ref _metalRemaining);
            AnimatePopup(_carrotPopup, _carrotOrigin, ref _carrotRemaining);
        }
        private static void AnimatePopup(Text popup, Vector2 origin, ref float remaining)
        {
            if (popup == null) return;
            remaining = Mathf.Max(0f, remaining - Time.unscaledDeltaTime);
            Color color = popup.color; color.a = Mathf.Clamp01(remaining * 2f); popup.color = color;
            popup.rectTransform.anchoredPosition = origin + Vector2.down * (1.4f - remaining) * 8f;
        }
        private void OnDestroy()
        {
            if (_player == null) return;
            _player.CoinsChanged -= OnCoins; _player.MetalChanged -= OnMetal;
            _player.CarrotsChanged -= OnCarrots; _player.Changed -= RefreshCarrots;
        }
    }
}
