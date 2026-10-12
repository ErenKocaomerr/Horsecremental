using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    public sealed class FarmUI : MonoBehaviour
    {
        [SerializeField] private GameplayWorld _world;
        [SerializeField] private ProgressionSettings _settings;
        [SerializeField] private Text _status;
        [SerializeField] private Text _feedback;
        [SerializeField] private Button _collect;
        [SerializeField] private Button _feed;
        [SerializeField] private Image _fill;
        private PlayerProgression _player;
        private void Awake() => _player = ProgressionSession.GetOrCreate(_settings);
        private void OnEnable()
        {
            _collect.onClick.AddListener(Collect); _feed.onClick.AddListener(Feed);
            _player.Changed += Refresh; InvokeRepeating(nameof(Refresh), 0f, 0.1f);
        }
        private void OnDisable()
        {
            _collect.onClick.RemoveListener(Collect); _feed.onClick.RemoveListener(Feed);
            _player.Changed -= Refresh; CancelInvoke();
        }
        private bool CanAct => _world.IsManagement && _world.Area == ManagementArea.Farm;
        private void Collect()
        {
            if (!CanAct) return;
            int count = _player.CollectCarrots();
            _feedback.text = count > 0 ? "+" + count + " Carrots collected!" : "No ripe Carrots yet.";
        }
        private void Feed()
        {
            if (!CanAct) return;
            _feedback.text = _player.TryFeed(_world.Settings) ? "Horse fed! +" + _world.Settings.FeedSpeed.ToString("0.##") + " Speed" :
                "Need " + (_world.Settings.FeedCost - _player.Carrots) + " more collected Carrots.";
        }
        private void Refresh()
        {
            int capacity = _player.FarmCapacity(_world.Settings);
            _status.text = _player.StoredCarrots + " / " + capacity + "\n" +
                (_player.StoredCarrots >= capacity ? "FULL" : (_player.CarrotSeconds(_world.Settings) - _player.FarmElapsed).ToString("0.0") + "s");
        }
        private void Update() => _fill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
            410f * (_player.StoredCarrots >= _player.FarmCapacity(_world.Settings) ? 1f : (float)(_player.FarmElapsed / _player.CarrotSeconds(_world.Settings))));
    }
}
