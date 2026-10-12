using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    public sealed class ManagementUI : MonoBehaviour
    {
        [SerializeField] private GameplayWorld _world;
        [SerializeField] private ProgressionSettings _progression;
        [SerializeField] private Text _training;
        [SerializeField] private Text _farm;
        [SerializeField] private Text _difficulty;
        [SerializeField] private Text _feedback;
        [SerializeField] private Image _trainingFill;
        [SerializeField] private RectTransform _horseIcon;
        [SerializeField] private Image _farmFill;
        [SerializeField] private Button _collect;
        [SerializeField] private Button _feed;
        [SerializeField] private Button _left;
        [SerializeField] private Button _right;
        [SerializeField] private CanvasGroup _panels;
        private PlayerProgression _player;
        private float _feedbackRemaining;
        private void Awake() => _player = ProgressionSession.GetOrCreate(_progression);
        private void OnEnable()
        {
            _collect.onClick.AddListener(Collect);
            _feed.onClick.AddListener(Feed);
            _left.onClick.AddListener(_world.ShowStable);
            _right.onClick.AddListener(_world.ShowRaceArea);
            InvokeRepeating(nameof(RefreshText), 0f, 0.1f);
        }
        private void OnDisable()
        {
            _collect.onClick.RemoveListener(Collect);
            _feed.onClick.RemoveListener(Feed);
            _left.onClick.RemoveListener(_world.ShowStable);
            _right.onClick.RemoveListener(_world.ShowRaceArea);
            CancelInvoke();
        }
        private void Collect()
        {
            if (!_world.IsManagement || _world.ViewingRace) return;
            int amount = _player.CollectCarrots();
            if (amount > 0) ShowFeedback("+" + amount + " CARROTS COLLECTED");
            RefreshText();
        }
        private void Feed()
        {
            if (!_world.IsManagement || _world.ViewingRace) return;
            float before = _player.Stats.Speed;
            if (_player.TryFeed(_world.Settings))
                ShowFeedback("FED HORSE!  SPEED " + before.ToString("0.00") + " → " + _player.Stats.Speed.ToString("0.00"));
            RefreshText();
        }
        private void ShowFeedback(string text) { _feedback.text = text; _feedbackRemaining = 2f; }
        private void RefreshText()
        {
            WorldSettings settings = _world.Settings;
            _training.text = "TRAINING RACE #" + _player.TrainingRaceNumber + "\nNext race: " +
                (settings.TrainingSeconds - _player.TrainingElapsed).ToString("0.0") + "s    Reward: " + settings.TrainingReward + " coins";
            _farm.text = "CARROT FARM\nStorage: " + _player.StoredCarrots + " / " + settings.FarmCapacity +
                "    Collected: " + _player.Carrots + "\n" + (_player.StoredCarrots >= settings.FarmCapacity ? "STORAGE FULL — COLLECT" :
                "Next carrot: " + (settings.CarrotSeconds - _player.FarmElapsed).ToString("0.0") + "s") +
                "\nFeed: " + settings.FeedCost + " carrots → +" + settings.FeedSpeed.ToString("0.0") + " permanent speed";
            float power = _player.Stats.Speed * 10f + _player.Stats.Acceleration * 0.25f + _player.Stats.BoostPower;
            float ratio = power / settings.RecommendedPower;
            _difficulty.text = "MAIN RACE  •  " + (ratio < 0.8f ? "VERY HARD" : ratio < 1f ? "HARD" : "COMPETITIVE") +
                "\nYour power: " + power.ToString("0") + "    Recommended: " + settings.RecommendedPower.ToString("0") +
                "\nFixed rivals. Upgrade, then try again!";
            _collect.interactable = _world.IsManagement && _player.StoredCarrots > 0;
            _feed.interactable = _world.IsManagement && _player.Carrots >= settings.FeedCost;
            _left.interactable = _world.IsManagement && _world.ViewingRace;
            _right.interactable = _world.IsManagement && !_world.ViewingRace;
        }
        private void Update()
        {
            _trainingFill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                410f * (float)(_player.TrainingElapsed / _world.Settings.TrainingSeconds));
            _horseIcon.anchoredPosition = new Vector2(-205f + 410f * (float)(_player.TrainingElapsed / _world.Settings.TrainingSeconds), 14f);
            _farmFill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                410f * (_player.StoredCarrots >= _world.Settings.FarmCapacity ? 1f : (float)(_player.FarmElapsed / _world.Settings.CarrotSeconds)));
            float target = _world.ViewingRace ? 0f : 1f;
            _panels.alpha = Mathf.MoveTowards(_panels.alpha, target, Time.unscaledDeltaTime * 5f);
            _panels.interactable = _panels.blocksRaycasts = !_world.ViewingRace && _world.IsManagement;
            if (_feedbackRemaining <= 0f) return;
            _feedbackRemaining -= Time.unscaledDeltaTime;
            Color color = _feedback.color;
            color.a = Mathf.Clamp01(_feedbackRemaining);
            _feedback.color = color;
        }
    }
}
