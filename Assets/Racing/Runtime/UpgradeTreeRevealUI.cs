using UnityEngine;

namespace Horsecremental.Racing
{
    // Keeps hidden views subscribed so a purchase can reveal their next branch.
    public sealed class UpgradeTreeRevealUI : MonoBehaviour
    {
        [SerializeField] private ProgressionSettings _settings;
        private PlayerProgression _player;
        private UpgradeTreeNodeUI[] _nodes;
        private bool _initialized;

        private void Awake()
        {
            _player = ProgressionSession.GetOrCreate(_settings);
            _nodes = GetComponentsInChildren<UpgradeTreeNodeUI>(true);
        }
        private void OnEnable() { _player.Changed += Refresh; Refresh(); }
        private void OnDisable() => _player.Changed -= Refresh;
        private void Refresh()
        {
            for (int i = 0; i < _nodes.Length; i++)
            {
                UpgradeTreeNodeUI view = _nodes[i];
                // Authored exclusions, such as Playground crafting, stay excluded.
                if (!view.gameObject.activeSelf) continue;
                view.SetRevealed(_player.IsUnlocked(view.Node) || _player.HasPrerequisite(view.Node), _initialized);
            }
            _initialized = true;
        }
    }
}
