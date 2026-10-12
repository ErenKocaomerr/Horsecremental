using UnityEngine;

namespace Horsecremental.Racing
{
    public enum DetailKind { Tree, Horse, Production, Equipment, Training, Feed, Farm, RaceHelp, Navigation, Resource }

    [CreateAssetMenu(menuName = "Horsecremental/UI Presentation")]
    public sealed class UIPresentationSettings : ScriptableObject
    {
        [SerializeField] private Sprite[] _icons;
        [SerializeField] private Vector2[] _treePositions;
        public Sprite Icon(int index) => _icons != null && index >= 0 && index < _icons.Length ? _icons[index] : null;
        public Vector2 TreePosition(UpgradeNode node) => _treePositions[UpgradeTreeRules.Index(node)];
    }
}
