using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    public sealed class AreaNavigationUI : MonoBehaviour
    {
        [SerializeField] private GameplayWorld _world;
        [SerializeField] private Button[] _buttons;
        [SerializeField] private Text _title;
        private static readonly string[] AREA_NAMES = { "STABLE", "FARM", "BLACKSMITH", "MAIN RACE" };
        private void OnEnable()
        {
            _buttons[0].onClick.AddListener(_world.ShowStable);
            _buttons[1].onClick.AddListener(_world.ShowFarm);
            _buttons[2].onClick.AddListener(_world.ShowBlacksmith);
            _buttons[3].onClick.AddListener(_world.ShowRaceArea);
            _world.Changed += Refresh;
            Refresh();
        }
        private void OnDisable()
        {
            _buttons[0].onClick.RemoveListener(_world.ShowStable);
            _buttons[1].onClick.RemoveListener(_world.ShowFarm);
            _buttons[2].onClick.RemoveListener(_world.ShowBlacksmith);
            _buttons[3].onClick.RemoveListener(_world.ShowRaceArea);
            _world.Changed -= Refresh;
        }
        private void Refresh()
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].gameObject.SetActive(_world.Phase == WorldPhase.StableManagement);
                _buttons[i].interactable = _world.IsManagement && (int)_world.Area != i;
            }
            _title.text = AREA_NAMES[(int)_world.Area];
        }
    }
}
