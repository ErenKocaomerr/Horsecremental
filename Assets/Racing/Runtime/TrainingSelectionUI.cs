using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    // One contextual selector; race simulation remains in the existing RaceManager.
    [DefaultExecutionOrder(100)]
    public sealed class TrainingSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameplayWorld _world;
        [SerializeField] private Button _open;
        [SerializeField] private Button _close;
        [SerializeField] private TrainingTierCardUI[] _cards;
        private void Start()
        {
            _open.onClick.AddListener(_world.ShowTraining);
            _close.onClick.AddListener(_world.ShowStable);
            for (int i = 0; i < _cards.Length; i++) _cards[i].Initialize(_world, i);
            _world.Player.Changed += Refresh;
            _world.Changed += Refresh;
            Refresh();
        }
        private void Refresh()
        {
            _open.gameObject.SetActive(_world.Phase == WorldPhase.StableManagement);
            _open.interactable = _world.IsManagement;
            for (int i = 0; i < _cards.Length; i++) _cards[i].Refresh();
        }
        private void OnDestroy()
        {
            _open.onClick.RemoveListener(_world.ShowTraining);
            _close.onClick.RemoveListener(_world.ShowStable);
            if (_world.Player == null) return;
            _world.Player.Changed -= Refresh;
            _world.Changed -= Refresh;
        }
    }
}
