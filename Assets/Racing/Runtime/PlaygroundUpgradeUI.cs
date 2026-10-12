using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    [RequireComponent(typeof(Button))]
    [DefaultExecutionOrder(100)]
    public sealed class PlaygroundUpgradeUI : MonoBehaviour
    {
        [SerializeField] private GameplayWorld _world;
        [SerializeField] private ProductionUpgrade _upgrade;
        [SerializeField] private Text _label;
        [SerializeField] private StableAudioHooks _audio;
        private Button _button;
        private PlayerProgression _player;
        private void Awake() => TryGetComponent(out _button);
        private void Start()
        {
            _player = _world.Player;
            _button.onClick.AddListener(Buy);
            _player.Changed += Refresh;
            _world.Changed += Refresh;
            Refresh();
        }
        private void Buy()
        {
            if (!_world.IsManagement || _world.ViewingRace ||
                (_world.Area != ManagementArea.Stable && _world.Area != ManagementArea.Farm)) return;
            if (_player.TryUpgradeProduction(_upgrade, _world.Settings)) _audio.Play(StableSound.TrainingUpgrade);
        }
        private void Refresh()
        {
            int cost = _player.ProductionCost(_upgrade);
            bool unlocked = _player.IsUnlocked(_upgrade);
            _button.interactable = _world.IsManagement && !_world.ViewingRace && unlocked &&
                cost < int.MaxValue && _player.Coins >= cost;
            _label.text = ShortName(_upgrade) + "\nLv." + _player.ProductionLevel(_upgrade) +
                "\n" + (!unlocked ? "Unlock in Tree" : cost == int.MaxValue ? "MAX" : cost + " Coins");
        }
        private static string ShortName(ProductionUpgrade upgrade)
        {
            switch (upgrade)
            {
                case ProductionUpgrade.TrainingHorseCount: return "Horses";
                case ProductionUpgrade.TrainingSpeed: return "Lap Speed";
                case ProductionUpgrade.TrainingReward: return "Lap Reward";
                case ProductionUpgrade.GrowSpeed: return "Auto Growth";
                case ProductionUpgrade.ClickGrowthPower: return "Click Growth";
                default: return "Harvest Yield";
            }
        }
        private void OnDestroy()
        {
            if (_player == null) return;
            _button.onClick.RemoveListener(Buy);
            _player.Changed -= Refresh;
            _world.Changed -= Refresh;
        }
    }
}

