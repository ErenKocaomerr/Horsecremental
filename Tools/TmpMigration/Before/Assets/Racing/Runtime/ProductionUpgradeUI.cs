using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    public sealed class ProductionUpgradeUI : MonoBehaviour
    {
        [SerializeField] private GameplayWorld _world;
        [SerializeField] private ProgressionSettings _settings;
        [SerializeField] private ProductionUpgrade _upgrade;
        [SerializeField] private ManagementArea _area;
        [SerializeField] private Text _label;
        [SerializeField] private Text _feedback;
        private Button _button;
        private PlayerProgression _player;
        private void Awake() { TryGetComponent(out _button); _player = ProgressionSession.GetOrCreate(_settings); }
        private void OnEnable() { _button.onClick.AddListener(Buy); _player.Changed += Refresh; Refresh(); }
        private void OnDisable() { _button.onClick.RemoveListener(Buy); _player.Changed -= Refresh; }
        private void Buy()
        {
            if (!_world.IsManagement || _world.Area != _area) return;
            if (!_player.IsUnlocked(_upgrade)) { _feedback.text = "Unlock in Tree first."; return; }
            int cost = _player.ProductionCost(_upgrade);
            if (!_player.TryUpgradeProduction(_upgrade, _world.Settings))
            { _feedback.text = "Need " + Mathf.Max(0, cost - _player.Coins) + " more Coins."; return; }
            _feedback.text = _player.ProductionRule(_upgrade).Name + " upgraded!";
        }
        private void Refresh()
        {
            int cost = _player.ProductionCost(_upgrade);
            bool unlocked = _player.IsUnlocked(_upgrade);
            _button.interactable = unlocked && cost < int.MaxValue;
            _label.text = string.Empty;
        }
    }
}
