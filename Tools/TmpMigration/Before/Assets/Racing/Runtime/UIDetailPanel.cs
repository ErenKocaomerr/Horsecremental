using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    public sealed class UIDetailPanel : MonoBehaviour
    {
        [SerializeField] private GameplayWorld _world;
        [SerializeField] private ProgressionSettings _settings;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Image _icon;
        [SerializeField] private Text _title;
        [SerializeField] private Text _description;
        [SerializeField] private Text _status;
        [SerializeField] private Text _cost;
        private PlayerProgression _player;
        private UIDetailTarget _target;
        private ManagementArea _area;
        private WorldPhase _phase;
        public bool IsVisible => _group.alpha > 0f;
        public string Title => _title.text;
        public string Status => _status.text;
        private void Awake() => _player = ProgressionSession.GetOrCreate(_settings);
        private void OnEnable()
        {
            _world.Changed += OnWorldChanged;
            _player.Changed += Refresh;
            _area = _world.Area;
            _phase = _world.Phase;
            Clear();
        }
        private void OnDisable()
        {
            _world.Changed -= OnWorldChanged;
            _player.Changed -= Refresh;
        }
        public void Show(UIDetailTarget target)
        {
            if (target == null || !target.CanInspect) return;
            _target = target;
            Refresh();
        }
        public void Clear()
        {
            _target = null;
            _group.alpha = 0f;
        }
        private void OnWorldChanged()
        {
            if (_area != _world.Area || _phase != _world.Phase)
            {
                _area = _world.Area;
                _phase = _world.Phase;
                Clear();
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            }
        }
        private void Refresh()
        {
            if (_target == null || !_target.CanInspect) { Clear(); return; }
            UIDetailContent content = UIDetailFormatter.Create(_target.Kind, _target.Id, _player, _world);
            _icon.sprite = _target.Icon;
            _title.text = content.Title;
            _description.text = content.Description;
            _status.text = content.Status;
            _cost.text = content.Cost;
            _cost.color = content.Unavailable ? new Color(1f, 0.4f, 0.38f) : new Color(1f, 0.85f, 0.35f);
            _group.alpha = 1f;
        }
    }
}
