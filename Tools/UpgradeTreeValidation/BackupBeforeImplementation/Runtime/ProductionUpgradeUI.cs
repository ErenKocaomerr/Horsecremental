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
            int cost = _player.ProductionCost(_upgrade);
            if (!_player.TryUpgradeProduction(_upgrade, _world.Settings))
            { _feedback.text = "Need " + Mathf.Max(0, cost - _player.Coins) + " more Coins."; return; }
            _feedback.text = _player.ProductionRule(_upgrade).Name + " upgraded! Production improved.";
        }
        private void Refresh()
        {
            ProductionRule rule = _player.ProductionRule(_upgrade);
            int cost = _player.ProductionCost(_upgrade);
            _button.interactable = cost < int.MaxValue;
            _label.text = rule.Name + "  Lv." + _player.ProductionLevel(_upgrade) + " / " + rule.MaxLevel +
                "\n" + EffectText() + "\n" + (cost == int.MaxValue ? "MAX LEVEL" : cost + " Coins  " + (_player.Coins >= cost ? "✓ UPGRADE" : "— need " + (cost - _player.Coins)));
        }
        private string EffectText()
        {
            switch (_upgrade)
            {
                case ProductionUpgrade.ForgeSpeed: return "Craft time " + Mathf.Max(50, 100 - _player.ProductionLevel(_upgrade) * _player.ProductionRule(_upgrade).Effect * 100).ToString("0") + "%";
                case ProductionUpgrade.MetalEfficiency: return "Metal cost " + Mathf.Max(75, 100 - _player.ProductionLevel(_upgrade) * _player.ProductionRule(_upgrade).Effect * 100).ToString("0") + "% (rounded up)";
                case ProductionUpgrade.GrowSpeed: return "1 Carrot / " + _player.CarrotSeconds(_world.Settings).ToString("0.#") + "s";
                case ProductionUpgrade.FarmCapacity: return "Storage " + _player.FarmCapacity(_world.Settings) + " Carrots";
                case ProductionUpgrade.TrainingSpeed: return "Race / " + _player.TrainingSeconds(_world.Settings).ToString("0.#") + "s";
                default: return _player.TrainingCoins(_world.Settings) + " Coins + " + _world.Settings.TrainingMetal + " Metal / race";
            }
        }
    }
}
