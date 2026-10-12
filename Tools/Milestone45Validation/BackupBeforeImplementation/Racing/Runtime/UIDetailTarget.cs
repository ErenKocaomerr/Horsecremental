using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    public sealed class UIDetailTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private UIDetailPanel _panel;
        [SerializeField] private DetailKind _kind;
        [SerializeField] private int _id;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Image _focus;
        private CanvasGroup[] _groups;
        private bool _hovered;
        private bool _selected;
        public DetailKind Kind => _kind;
        public int Id => _id;
        public Sprite Icon => _icon;
        public bool CanInspect
        {
            get
            {
                if (!gameObject.activeInHierarchy) return false;
                for (int i = 0; i < _groups.Length; i++)
                    if (!_groups[i].blocksRaycasts || _groups[i].alpha < 0.1f) return false;
                return true;
            }
        }
        private void Awake() => _groups = GetComponentsInParent<CanvasGroup>(true);
        public void OnPointerEnter(PointerEventData eventData) { _hovered = true; RefreshFocus(); Inspect(); }
        public void OnPointerExit(PointerEventData eventData) { _hovered = false; RefreshFocus(); }
        public void OnSelect(BaseEventData eventData) { _selected = true; RefreshFocus(); Inspect(); }
        public void OnDeselect(BaseEventData eventData) { _selected = false; RefreshFocus(); }
        private void OnDisable() { _selected = _hovered = false; RefreshFocus(); }
        private void RefreshFocus() { if (_focus != null) _focus.enabled = (_hovered || _selected) && CanInspect; }
        public void Inspect() => _panel.Show(this);
    }
}
