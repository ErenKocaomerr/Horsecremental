using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing
{
    public sealed class AreaNavigationUI : MonoBehaviour
    {
        [SerializeField] private GameplayWorld _world;
        [SerializeField] private Button[] _buttons;
        [SerializeField] private Text _title;
        [SerializeField] private ProgressionSettings _settings;
        private PlayerProgression _player;
        private Text[] _labels;
        private static readonly string[] AREA_NAMES = { "STABLE", "FARM", "BLACKSMITH", "MAIN RACE", "UPGRADE TREE" };
        private void Awake()
        {
            _player = ProgressionSession.GetOrCreate(_settings);
            _labels = new Text[_buttons.Length];
            for (int i = 0; i < _buttons.Length; i++) _labels[i] = _buttons[i].GetComponentInChildren<Text>();
        }
        private void OnEnable()
        {
            _buttons[0].onClick.AddListener(_world.ShowStable);
            _buttons[1].onClick.AddListener(_world.ShowFarm);
            _buttons[2].onClick.AddListener(_world.ShowBlacksmith);
            _buttons[3].onClick.AddListener(_world.ShowRaceArea);
            _buttons[4].onClick.AddListener(_world.ShowUpgradeTree);
            _world.Changed += Refresh;
            _player.Changed += Refresh;
            Refresh();
        }
        private void OnDisable()
        {
            _buttons[0].onClick.RemoveListener(_world.ShowStable);
            _buttons[1].onClick.RemoveListener(_world.ShowFarm);
            _buttons[2].onClick.RemoveListener(_world.ShowBlacksmith);
            _buttons[3].onClick.RemoveListener(_world.ShowRaceArea);
            _buttons[4].onClick.RemoveListener(_world.ShowUpgradeTree);
            _world.Changed -= Refresh;
            _player.Changed -= Refresh;
        }
        private void Refresh()
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].gameObject.SetActive(_world.Phase == WorldPhase.StableManagement);
                bool unlocked = _world.CanVisit((ManagementArea)i);
                _buttons[i].interactable = _world.IsManagement && unlocked && (int)_world.Area != i;
                _labels[i].text = AREA_NAMES[i];
            }
            _title.text = AREA_NAMES[(int)_world.Area];
        }
    }
}
