using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    public sealed class StableUI : MonoBehaviour
    {
        [SerializeField] private ProgressionSettings _settings;
        [SerializeField] private Text _coins;
        [SerializeField] private Text _stats;
        [SerializeField] private Text _feedback;
        [SerializeField] private Button _startRace;
        [SerializeField] private Button[] _upgrades;
        [SerializeField] private Text[] _upgradeLabels;
        private PlayerProgression _player;
        private GameplayWorld _world;

        private void Awake()
        {
            _player = ProgressionSession.GetOrCreate(_settings);
            _world = FindAnyObjectByType<GameplayWorld>();
        }

        private void OnEnable()
        {
            _player.Changed += Refresh;
            _startRace.onClick.AddListener(GameScenes.StartRace);
            _upgrades[0].onClick.AddListener(BuySpeed);
            _upgrades[1].onClick.AddListener(BuyAcceleration);
            _upgrades[2].onClick.AddListener(BuyBoost);
            Refresh();
        }

        private void OnDisable()
        {
            if (_player == null) return;
            _player.Changed -= Refresh;
            _startRace.onClick.RemoveListener(GameScenes.StartRace);
            _upgrades[0].onClick.RemoveListener(BuySpeed);
            _upgrades[1].onClick.RemoveListener(BuyAcceleration);
            _upgrades[2].onClick.RemoveListener(BuyBoost);
        }

        private void BuySpeed() => Buy(HorseUpgrade.Speed);
        private void BuyAcceleration() => Buy(HorseUpgrade.Acceleration);
        private void BuyBoost() => Buy(HorseUpgrade.BoostPower);

        private void Buy(HorseUpgrade upgrade)
        {
            if (_world != null && (!_world.IsManagement || (_world.Area != ManagementArea.Stable &&
                !(_world.PlaygroundSettings != null && _world.Area == ManagementArea.Farm)))) return;
            if (!_player.IsUnlocked(upgrade)) { _feedback.text = "Unlock in Tree first."; return; }
            float before = Value(upgrade, _player.Stats);
            if (!_player.TryUpgrade(upgrade))
            {
                _feedback.text = _player.ManualTrainingEnabled ? "Need more Training Tokens." : "Need more Coins.";
                return;
            }
            _feedback.text = Name(upgrade) + " upgraded!  " + before.ToString("0.##") + " → " +
                Value(upgrade, _player.Stats).ToString("0.##");
        }

        private void Refresh()
        {
            HorseParameters stats = _player.Stats;
            _coins.text = "Coins: " + _player.Coins;
            _stats.text = "Speed  " + stats.Speed.ToString("0.##") + "\nAcceleration  " + stats.Acceleration.ToString("0.##") +
                "\nBoost  " + stats.BoostPower.ToString("0.##") + "\nPower  " + _player.HorsePower.ToString("0.0");
            for (int i = 0; i < _upgrades.Length; i++)
            {
                HorseUpgrade upgrade = (HorseUpgrade)i;
                int cost = _player.Cost(upgrade);
                bool unlocked = _player.IsUnlocked(upgrade);
                _upgrades[i].interactable = _player.ManualTrainingEnabled ? _player.CanAfford(upgrade) : unlocked && cost < int.MaxValue;
                _upgradeLabels[i].text = _player.ManualTrainingEnabled ? Name(upgrade).ToUpperInvariant() + "  Lv." + _player.Level(upgrade) +
                    "\n" + Value(upgrade, stats).ToString("0.##") + " → " + (Value(upgrade, stats) + _player.Rule(upgrade).Increase).ToString("0.##") +
                    "\n" + (cost == int.MaxValue ? "MAX" : cost + " Tokens • TRAIN") : string.Empty;
            }
        }
        private static string Name(HorseUpgrade upgrade) => upgrade == HorseUpgrade.Speed ? "Speed" :
            upgrade == HorseUpgrade.Acceleration ? "Acceleration" : "Boost Power";

        private static float Value(HorseUpgrade upgrade, HorseParameters stats) => upgrade == HorseUpgrade.Speed ?
            stats.Speed : upgrade == HorseUpgrade.Acceleration ? stats.Acceleration : stats.BoostPower;
    }
}
