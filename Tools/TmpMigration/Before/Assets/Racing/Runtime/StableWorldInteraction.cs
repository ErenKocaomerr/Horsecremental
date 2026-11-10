using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Horsecremental.Racing
{
    public sealed class StableWorldInteraction : MonoBehaviour
    {
        [SerializeField] private GameplayWorld _world;
        [SerializeField] private Camera _camera;
        [SerializeField] private FarmWorldView _farm;
        [SerializeField] private WorldPickupManager _pickups;
        private readonly List<RaycastResult> _hits = new List<RaycastResult>(128);
        private PointerEventData _pointer;
        private Vector2 _previousWorld;
        private bool _hasPrevious;
        private InputAction _click;
        private bool _clicked;
        public bool CanInteract => _world.IsManagement && !_world.ViewingRace &&
            (_world.Area == ManagementArea.Stable || _world.Area == ManagementArea.Farm);
        private void Awake()
        {
            _click = new InputAction("Crop click", InputActionType.Button, "<Mouse>/leftButton");
            _click.performed += OnClick;
        }
        private void OnClick(InputAction.CallbackContext context) => _clicked = true;
        private void OnEnable() => _click.Enable();
        private void OnDisable() { _click.Disable(); _clicked = false; _hasPrevious = false; }
        private void OnDestroy() { _click.performed -= OnClick; _click.Dispose(); }
        private void Start() => _pointer = new PointerEventData(EventSystem.current);
        public bool IsUIBlocked(Vector2 screen)
        {
            if (_pointer == null) return true;
            _pointer.Reset();
            _pointer.position = screen;
            _hits.Clear();
            EventSystem.current.RaycastAll(_pointer, _hits);
            return _hits.Count > 0;
        }
        private void Update()
        {
            bool clicked = _clicked;
            _clicked = false;
            if (_world.Player == null || Mouse.current == null) return;
            Vector2 screen = Mouse.current.position.ReadValue();
            if (!CanInteract || IsUIBlocked(screen))
            {
                _hasPrevious = false;
                _farm.Highlight(-1);
                return;
            }
            Vector3 position = _camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -_camera.transform.position.z));
            Vector2 point = position;
            int plot = _farm.HitPlot(point);
            _farm.Highlight(plot);
            _pickups.Sweep(_hasPrevious ? _previousWorld : point, point);
            if (plot >= 0 && clicked) _farm.Click(plot);
            _previousWorld = point;
            _hasPrevious = true;
        }
    }
}

