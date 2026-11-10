using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    public sealed class UpgradeIconState : MonoBehaviour
    {
        [SerializeField] private ProgressionSettings _settings;
        [SerializeField] private DetailKind _kind;
        [SerializeField] private int _id;
        [SerializeField] private Image _border;
        [SerializeField] private Image _mark;
        [SerializeField] private Sprite _lock;
        [SerializeField] private Sprite _check;
        [SerializeField] private Image _icon;
        [SerializeField] private UIPresentationSettings _presentation;
        private PlayerProgression _player;
        private static readonly Color GREEN = new Color(0.27f, 0.94f, 0.34f);
        private static readonly Color YELLOW = new Color(1f, 0.86f, 0.2f);
        private static readonly Color RED = new Color(0.97f, 0.25f, 0.26f);
        private static readonly Color GREY = new Color(0.35f, 0.39f, 0.43f);
        private void Awake() => _player = ProgressionSession.GetOrCreate(_settings);
        private void OnEnable() { _player.Changed += Refresh; Refresh(); }
        private void OnDisable() => _player.Changed -= Refresh;
        private void Refresh()
        {
            bool locked;
            bool complete;
            bool affordable;
            if (_kind == DetailKind.Tree)
            {
                UpgradeNode node = (UpgradeNode)_id;
                locked = !_player.HasPrerequisite(node);
                complete = _player.IsUnlocked(node);
                affordable = _player.CanUnlock(node);
                // Discovered nodes keep their padlock until actually purchased.
                _mark.enabled = !complete;
                _mark.sprite = _lock;
                _mark.color = new Color(0.75f, 0.72f, 0.72f);
                _border.color = complete ? new Color(0.97f, 0.79f, 0.55f) :
                    affordable ? new Color(0.92f, 0.71f, 0.41f) : GREY;
                if (_presentation != null)
                    _border.sprite = complete ? _presentation.TreeNodeBorder : _presentation.Icon(20);
                if (_icon != null) _icon.color = complete ? Color.white : new Color(0.52f, 0.52f, 0.56f);
                return;
            }
            else if (_kind == DetailKind.Horse)
            {
                HorseUpgrade upgrade = (HorseUpgrade)_id;
                locked = !_player.IsUnlocked(upgrade);
                complete = _player.Cost(upgrade) == int.MaxValue;
                affordable = _player.CanAfford(upgrade);
            }
            else
            {
                ProductionUpgrade upgrade = (ProductionUpgrade)_id;
                locked = !_player.IsUnlocked(upgrade);
                complete = _player.ProductionCost(upgrade) == int.MaxValue;
                affordable = _player.Coins >= _player.ProductionCost(upgrade);
            }
            _border.color = complete ? GREEN : locked ? GREY : affordable ? YELLOW : RED;
            _mark.enabled = locked || complete;
            _mark.sprite = complete ? _check : _lock;
            _mark.color = complete ? GREEN : Color.white;
        }
    }
}
