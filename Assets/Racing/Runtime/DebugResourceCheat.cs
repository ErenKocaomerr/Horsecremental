using UnityEngine;
using UnityEngine.InputSystem;

namespace Horsecremental.Racing
{
    public sealed class DebugResourceCheat : MonoBehaviour
    {
        [SerializeField] private ProgressionSettings _settings;
        private InputAction _grant;
        private PlayerProgression _player;
        private void Awake()
        {
            _player = ProgressionSession.GetOrCreate(_settings);
            _grant = new InputAction("Grant resources", InputActionType.Button, "<Keyboard>/k");
            _grant.performed += Grant;
        }
        private void OnEnable() => _grant.Enable();
        private void OnDisable() => _grant.Disable();
        private void Grant(InputAction.CallbackContext context)
        {
            UpgradeTreeSettings tree = _settings.UpgradeTree;
            _player.GrantDebugResources(tree.DebugCoins, tree.DebugTokens);
        }
        private void OnDestroy()
        {
            _grant.performed -= Grant;
            _grant.Dispose();
        }
    }
}
