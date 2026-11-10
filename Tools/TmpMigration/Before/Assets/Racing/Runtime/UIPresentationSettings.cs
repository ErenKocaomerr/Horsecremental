using UnityEngine;

namespace Horsecremental.Racing
{
    public enum DetailKind { Tree, Horse, Production, Equipment, Training, Feed, Farm, RaceHelp, Navigation, Resource }

    [CreateAssetMenu(menuName = "Horsecremental/UI Presentation")]
    public sealed class UIPresentationSettings : ScriptableObject
    {
        [SerializeField] private Sprite[] _icons;
        [SerializeField] private Vector2[] _treePositions;
        [SerializeField] private Material _treeBackground;
        [SerializeField] private Sprite _treeNodeShape;
        [SerializeField] private Sprite _treeNodeBorder;
        [SerializeField, Min(0.01f)] private float _treeRevealSeconds = 0.25f;
        public Material TreeBackground => _treeBackground;
        public Sprite TreeNodeShape => _treeNodeShape;
        public Sprite TreeNodeBorder => _treeNodeBorder;
        public float TreeRevealSeconds => _treeRevealSeconds;
        public Sprite Icon(int index) => _icons != null && index >= 0 && index < _icons.Length ? _icons[index] : null;
        public Vector2 TreePosition(UpgradeNode node) => _treePositions[UpgradeTreeRules.Index(node)];
    }
}
