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
            if (_world != null && (!_world.IsManagement || _world.Area != ManagementArea.Stable)) return;
            float before = Value(upgrade, _player.Stats);
            if (!_player.TryUpgrade(upgrade))
            {
                _feedback.text = "Need more coins. Training earns them automatically!";
                return;
            }
            _feedback.text = Name(upgrade) + " upgraded!  " + before.ToString("0.##") + " → " +
                Value(upgrade, _player.Stats).ToString("0.##");
        }

        private void Refresh()
        {
            HorseParameters stats = _player.Stats;
            _coins.text = "Coins: " + _player.Coins;
            _stats.text = StatLine("SPEED", HorseUpgrade.Speed) + "\n\n" +
                StatLine("ACCELERATION", HorseUpgrade.Acceleration) + "\n\n" +
                StatLine("BOOST POWER", HorseUpgrade.BoostPower) + "\n\nHORSE POWER  " + _player.HorsePower.ToString("0.0");
            for (int i = 0; i < _upgrades.Length; i++)
            {
                HorseUpgrade upgrade = (HorseUpgrade)i;
                int cost = _player.Cost(upgrade);
                bool affordable = _player.CanAfford(upgrade);
                _upgrades[i].interactable = cost < int.MaxValue;
                _upgradeLabels[i].text = Name(upgrade) + " — Lv. " + _player.Level(upgrade) +
                    "\n+" + _player.Rule(upgrade).Increase.ToString("0.##") +
                    "   |   " + (cost == int.MaxValue ? "MAX" : cost + " Coins") +
                    "\n" + (affordable ? "UPGRADE" : "Need more coins");
            }
        }
        private string StatLine(string name, HorseUpgrade stat)
        {
            StatBreakdown value = _player.Breakdown(stat);
            return name + "  " + value.Total.ToString("0.##") + "\nBase " + value.Base.ToString("0.##") +
                "  |  Upgrades +" + value.Upgrades.ToString("0.##") + "\nFood +" + value.Food.ToString("0.##") +
                "  |  Equipment +" + value.Equipment.ToString("0.##");
        }

        private static string Name(HorseUpgrade upgrade) => upgrade == HorseUpgrade.Speed ? "Speed" :
            upgrade == HorseUpgrade.Acceleration ? "Acceleration" : "Boost Power";

        private static float Value(HorseUpgrade upgrade, HorseParameters stats) => upgrade == HorseUpgrade.Speed ?
            stats.Speed : upgrade == HorseUpgrade.Acceleration ? stats.Acceleration : stats.BoostPower;
    }
}
