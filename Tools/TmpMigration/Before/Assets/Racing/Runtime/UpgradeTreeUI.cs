using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    public sealed class UpgradeTreeUI : MonoBehaviour
    {
        [SerializeField] private GameplayWorld _world;
        [SerializeField] private ProgressionSettings _settings;
        [SerializeField] private Text _summary;
        [SerializeField] private Text _feedback;
        private PlayerProgression _player;
        private void Awake() => _player = ProgressionSession.GetOrCreate(_settings);
        private void OnEnable() { _player.Changed += Refresh; Refresh(); }
        private void OnDisable() => _player.Changed -= Refresh;
        public void Buy(UpgradeNode node)
        {
            if (!_world.IsManagement || _world.Area != ManagementArea.UpgradeTree) return;
            if (_player.TryUnlock(node))
                _feedback.text = UpgradeTreeRules.Name(node) + " unlocked!";
            else if (_player.IsUnlocked(node)) _feedback.text = "Already unlocked.";
            else if (!_player.HasPrerequisite(node))
                _feedback.text = "First unlock " + UpgradeTreeRules.Name(_player.NodeRule(node).Prerequisite) + ".";
            else
            {
                UpgradeNodeRule rule = _player.NodeRule(node);
                int missing = rule.Cost - (rule.Currency == UpgradeCurrency.Coins ? _player.Coins : _player.RaceTokens);
                _feedback.text = "Need " + missing + (rule.Currency == UpgradeCurrency.Coins ? " more Coins." : " more Race Tokens. Complete Main Races!");
            }
        }
        private void Refresh()
        {
            _summary.text = string.Empty;
        }
    }
}
