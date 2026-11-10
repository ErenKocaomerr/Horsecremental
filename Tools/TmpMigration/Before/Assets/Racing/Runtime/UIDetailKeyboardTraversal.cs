using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Horsecremental.Racing
{
    public sealed class UIDetailKeyboardTraversal : MonoBehaviour
    {
        [SerializeField] private UIDetailTarget[] _targets;
        private InputAction _tab;
        private void Awake()
        {
            _tab = new InputAction("Inspect UI", InputActionType.Button, "<Keyboard>/tab");
            _tab.performed += Next;
        }
        private void OnEnable() => _tab.Enable();
        private void OnDisable() => _tab.Disable();
        private void OnDestroy() { _tab.performed -= Next; _tab.Dispose(); }
        private void Next(InputAction.CallbackContext context)
        {
            if (EventSystem.current == null || _targets.Length == 0) return;
            int start = -1;
            for (int i = 0; i < _targets.Length; i++)
                if (_targets[i].gameObject == EventSystem.current.currentSelectedGameObject) { start = i; break; }
            int direction = Keyboard.current != null && Keyboard.current.shiftKey.isPressed ? -1 : 1;
            if (start < 0 && direction < 0) start = 0;
            for (int step = 1; step <= _targets.Length; step++)
            {
                int index = (start + step * direction + _targets.Length * 2) % _targets.Length;
                if (!_targets[index].CanInspect) continue;
                EventSystem.current.SetSelectedGameObject(_targets[index].gameObject);
                return;
            }
        }
    }
}
