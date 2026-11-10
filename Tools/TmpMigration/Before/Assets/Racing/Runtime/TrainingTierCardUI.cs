using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    public sealed class TrainingTierCardUI : MonoBehaviour
    {
        [SerializeField] private Text _title;
        [SerializeField] private Text _status;
        [SerializeField] private Text _best;
        [SerializeField] private Text _rewards;
        [SerializeField] private Text _requirement;
        [SerializeField] private Button _start;
        [SerializeField] private Image _background;
        private GameplayWorld _world;
        private int _tier;
        private Text _startLabel;
        public void Initialize(GameplayWorld world, int tier)
        {
            _world = world;
            _tier = tier;
            _startLabel = _start.GetComponentInChildren<Text>();
            _title.text = tier < 0 ? "Main Race" : world.TrainingLadder.Name(tier);
            _start.onClick.AddListener(StartRace);
            if (tier < 0) return;
            TrainingRaceRules rules = world.Player.TrainingRules(tier);
            _rewards.text = rules.MinimumCoins + "–" + rules.MaximumCoins + " Coins\n" +
                rules.MinimumTokens + "–" + rules.MaximumTokens + " Upgrade Tokens";
        }
        public void Refresh()
        {
            bool main = _tier < 0;
            bool unlocked = main ? _world.CanEnterMainRace : _world.Player.IsTrainingUnlocked(_tier);
            int best = main ? 0 : _world.Player.TrainingBestPlace(_tier);
            _status.text = unlocked ? "READY" : "LOCKED";
            _status.color = unlocked ? new Color(0.66f, 0.9f, 0.57f) : new Color(0.75f, 0.76f, 0.8f);
            _best.text = main ? "Power " + _world.Player.HorsePower.ToString("0.0") + " / " +
                _world.Settings.RecommendedPower.ToString("0.#") : "Best: " + (best == 0 ? "—" : Place(best));
            _requirement.text = main ? (unlocked ? "Power requirement met" : "Requires " +
                _world.Settings.RecommendedPower.ToString("0.#") + " power") :
                unlocked ? "4 horses • Finish to earn rewards" : "Win " + _world.TrainingLadder.Name(_tier - 1);
            _start.interactable = unlocked && _world.IsManagement && _world.Area == ManagementArea.Training;
            _startLabel.text = unlocked ? "START RACE" : "LOCKED";
            _background.color = unlocked ? new Color(0.12f, 0.19f, 0.17f, 0.98f) : new Color(0.12f, 0.13f, 0.16f, 0.98f);
        }
        private void StartRace()
        {
            if (_tier >= 0) { _world.StartTrainingRace(_tier); return; }
            if (!_world.CanEnterMainRace || !_world.IsManagement || _world.Area != ManagementArea.Training) return;
            _world.ShowRaceArea();
            _world.StartMainRace();
        }
        private static string Place(int place) => place == 1 ? "1st" : place == 2 ? "2nd" : place == 3 ? "3rd" : "4th";
        private void OnDestroy() => _start.onClick.RemoveListener(StartRace);
    }
}
