using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    // Training belongs to the horse view; navigation and the Farm have their own presenters.
    public sealed class ManagementUI : MonoBehaviour
    {
        [SerializeField] private GameplayWorld _world;
        [SerializeField] private ProgressionSettings _progression;
        [SerializeField] private Text _training;
        [SerializeField] private Text _difficulty;
        [SerializeField] private Image _trainingFill;
        [SerializeField] private RectTransform _horseIcon;
        private PlayerProgression _player;
        private void Awake() => _player = ProgressionSession.GetOrCreate(_progression);
        private void OnEnable() { _player.Changed += RefreshText; InvokeRepeating(nameof(RefreshText), 0f, 0.1f); }
        private void OnDisable() { _player.Changed -= RefreshText; CancelInvoke(); }
        private void RefreshText()
        {
            WorldSettings settings = _world.Settings;
            _training.text = "Training #" + _player.TrainingRaceNumber + "\n" +
                (_world.IsManagement ? (_player.TrainingSeconds(settings) - _player.TrainingElapsed).ToString("0.0") + "s" : "PAUSED");
            float ratio = _player.HorsePower / settings.RecommendedPower;
            _difficulty.text = ratio < 0.8f ? "VERY HARD" : ratio < 1f ? "HARD" : "COMPETITIVE";
        }
        private void Update()
        {
            float progress = (float)(_player.TrainingElapsed / _player.TrainingSeconds(_world.Settings));
            _trainingFill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 410f * progress);
            _horseIcon.anchoredPosition = new Vector2(-205f + 410f * progress, 14f);
        }
    }
}
