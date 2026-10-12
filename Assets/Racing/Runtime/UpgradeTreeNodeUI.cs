using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    [RequireComponent(typeof(Button), typeof(CanvasGroup))]
    public sealed class UpgradeTreeNodeUI : MonoBehaviour
    {
        [SerializeField] private UpgradeTreeUI _treeUI;
        [SerializeField] private ProgressionSettings _settings;
        [SerializeField] private UpgradeNode _node;
        [SerializeField] private Text _label;
        [SerializeField] private Image _connection;
        [SerializeField] private UIPresentationSettings _presentation;
        private PlayerProgression _player;
        private Button _button;
        private Image _background;
        private CanvasGroup _group;
        private bool _revealed;
        private bool _fading;
        private float _opacity;
        private Color _connectionColor;
        private static readonly Color OPEN = new Color(0.23f, 0.17f, 0.19f);
        private static readonly Color LOCKED = new Color(0.10f, 0.11f, 0.13f);
        private static readonly Color OPEN_LINE = new Color(0.91f, 0.73f, 0.51f);
        private static readonly Color LOCKED_LINE = new Color(0.36f, 0.33f, 0.39f);
        public UpgradeNode Node => _node;
        public bool IsRevealed => _revealed;
        private void Awake()
            => Initialize();
        private void Initialize()
        {
            if (_player != null) return;
            _player = ProgressionSession.GetOrCreate(_settings);
            TryGetComponent(out _button); TryGetComponent(out _background);
            TryGetComponent(out _group);
        }
        private void OnEnable() { _button.onClick.AddListener(Buy); _player.Changed += Refresh; Refresh(); }
        private void OnDisable() { _button.onClick.RemoveListener(Buy); _player.Changed -= Refresh; }
        private void Buy() { if (_revealed) _treeUI.Buy(_node); }
        public void SetRevealed(bool revealed, bool animate)
        {
            Initialize();
            bool changed = _revealed != revealed;
            _revealed = revealed;
            _group.blocksRaycasts = _group.interactable = revealed;
            if (!revealed || !animate) { _opacity = revealed ? 1f : 0f; _fading = false; }
            else if (changed) { _opacity = 0f; _fading = true; }
            Refresh();
            ApplyOpacity();
        }
        private void Update()
        {
            if (!_fading) return;
            float duration = _presentation == null ? 0.25f : Mathf.Max(0.01f, _presentation.TreeRevealSeconds);
            _opacity = Mathf.MoveTowards(_opacity, 1f, Time.unscaledDeltaTime / duration);
            _fading = _opacity < 1f;
            ApplyOpacity();
        }
        private void ApplyOpacity()
        {
            _group.alpha = _opacity;
            if (_connection == null) return;
            Color color = _connectionColor;
            color.a = _opacity;
            _connection.color = color;
        }
        private void Refresh()
        {
            bool unlocked = _player.IsUnlocked(_node);
            bool affordable = _player.CanUnlock(_node);
            _label.text = string.Empty;
            _background.color = unlocked ? OPEN : LOCKED;
            _background.sprite = unlocked && _presentation != null ? _presentation.TreeNodeShape : null;
            _button.interactable = _revealed && affordable;
            _connectionColor = unlocked ? OPEN_LINE : LOCKED_LINE;
            ApplyOpacity();
        }
    }
}
