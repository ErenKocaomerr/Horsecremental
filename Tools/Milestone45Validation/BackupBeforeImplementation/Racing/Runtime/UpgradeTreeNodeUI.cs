using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    [RequireComponent(typeof(Button))]
    public sealed class UpgradeTreeNodeUI : MonoBehaviour
    {
        [SerializeField] private UpgradeTreeUI _treeUI;
        [SerializeField] private ProgressionSettings _settings;
        [SerializeField] private UpgradeNode _node;
        [SerializeField] private Text _label;
        [SerializeField] private Image _connection;
        private PlayerProgression _player;
        private Button _button;
        private Image _background;
        private void Awake()
        {
            _player = ProgressionSession.GetOrCreate(_settings);
            TryGetComponent(out _button); TryGetComponent(out _background);
        }
        private void OnEnable() { _button.onClick.AddListener(Buy); _player.Changed += Refresh; Refresh(); }
        private void OnDisable() { _button.onClick.RemoveListener(Buy); _player.Changed -= Refresh; }
        private void Buy() => _treeUI.Buy(_node);
        private void Refresh()
        {
            bool unlocked = _player.IsUnlocked(_node);
            bool affordable = _player.CanUnlock(_node);
            _label.text = string.Empty;
            _background.color = new Color(0.08f, 0.085f, 0.095f);
            _button.interactable = affordable;
            if (_connection != null) _connection.color = unlocked ? new Color(0.4f, 0.85f, 0.6f) : new Color(0.3f, 0.34f, 0.4f);
        }
    }
}
