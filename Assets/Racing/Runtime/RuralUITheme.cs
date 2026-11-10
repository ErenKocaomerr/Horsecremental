using UnityEngine;

namespace Horsecremental.Racing
{
    [CreateAssetMenu(menuName = "Horsecremental/Rural UI Theme")]
    public sealed class RuralUITheme : ScriptableObject
    {
        [SerializeField] private Color _ink = new Color32(70, 48, 35, 255);
        [SerializeField] private Color _muted = new Color32(125, 99, 68, 255);
        [SerializeField] private Color _positive = new Color32(61, 102, 65, 255);
        [SerializeField] private Color _warning = new Color32(166, 70, 43, 255);
        [SerializeField] private Color _activeTab = new Color32(255, 224, 157, 255);
        [SerializeField] private Color _inactiveTab = new Color32(190, 171, 128, 255);
        public Color Ink => _ink;
        public Color Muted => _muted;
        public Color Positive => _positive;
        public Color Warning => _warning;
        public Color ActiveTab => _activeTab;
        public Color InactiveTab => _inactiveTab;
    }
}
