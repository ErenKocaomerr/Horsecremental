using UnityEngine;
using UnityEngine.InputSystem;

namespace Horsecremental.Racing
{
    [RequireComponent(typeof(HorseMovement))]
    public sealed class HorseController : MonoBehaviour
    {
        private HorseMovement _movement;
        private InputAction _boost;
        private InputAction _jump;

        private void Awake()
        {
            TryGetComponent(out _movement);
            _boost = new InputAction("Race Boost", InputActionType.Button, "<Keyboard>/space");
            _jump = new InputAction("Race Jump", InputActionType.Button, "<Keyboard>/e");
            _boost.performed += OnBoost;
            _jump.performed += OnJump;
        }

        private void OnEnable()
        {
            _boost.Enable();
            _jump.Enable();
        }

        private void OnDisable()
        {
            _boost.Disable();
            _jump.Disable();
        }

        private void OnDestroy()
        {
            _boost.performed -= OnBoost;
            _jump.performed -= OnJump;
            _boost.Dispose();
            _jump.Dispose();
        }

        private void OnBoost(InputAction.CallbackContext context) => _movement.TryBoost();
        private void OnJump(InputAction.CallbackContext context) => _movement.TryJump();
    }
}
