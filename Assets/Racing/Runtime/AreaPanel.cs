using UnityEngine;

namespace Horsecremental.Racing
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class AreaPanel : MonoBehaviour
    {
        [SerializeField] private GameplayWorld _world;
        [SerializeField] private ManagementArea _area;
        private CanvasGroup _group;
        private bool _visible;
        private void Awake() { TryGetComponent(out _group); _group.alpha = 0f; }
        private void OnEnable() { _world.Changed += Refresh; Refresh(); }
        private void OnDisable() { _world.Changed -= Refresh; }
        private void Refresh()
        {
            _visible = _world.IsManagement && _world.Area == _area;
            if (_world.PlaygroundSettings != null && (_area == ManagementArea.Stable || _area == ManagementArea.Farm))
                _visible = _world.IsManagement && (_world.Area == ManagementArea.Stable || _world.Area == ManagementArea.Farm);
            _group.interactable = _group.blocksRaycasts = _visible;
        }
        private void Update() => _group.alpha = Mathf.MoveTowards(_group.alpha, _visible ? 1f : 0f, Time.unscaledDeltaTime * 6f);
    }
}
