using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Horsecremental.Racing
{
    public enum WorldPhase { StableManagement, MainRacePreparing, MainRaceCountdown, MainRaceRunning, MainRaceFinished }
    public enum ManagementArea { Stable, Farm, Blacksmith, Race }
    public sealed class GameplayWorld : MonoBehaviour
    {
        [SerializeField] private WorldSettings _settings;
        [SerializeField] private WorldCameraNavigation _cameras;
        [SerializeField] private GameObject _stableUI;
        [SerializeField] private GameObject _managementUI;
        [SerializeField] private GameObject _raceUI;
        [SerializeField] private CanvasGroup _stableGroup;
        private RacePrototype _prototype;
        private float _prepareRemaining;
        private bool _returning;
        private bool _initialized;
        private InputAction _left;
        private InputAction _right;
        public WorldPhase Phase { get; private set; }
        public bool ViewingRace { get; private set; }
        public ManagementArea Area { get; private set; }
        public bool IsManagement => _initialized && Phase == WorldPhase.StableManagement && !_returning;
        public WorldSettings Settings => _settings;
        public RaceManager Race { get; private set; }
        public PlayerProgression Player { get; private set; }
        public event Action Changed;
        private void Awake()
        {
            _left = new InputAction("Stable", InputActionType.Button, "<Keyboard>/a");
            _right = new InputAction("Race area", InputActionType.Button, "<Keyboard>/d");
            _left.performed += OnLeft;
            _right.performed += OnRight;
        }
        private void OnEnable() { _left.Enable(); _right.Enable(); }
        private void OnDisable() { _left.Disable(); _right.Disable(); }
        private void OnLeft(InputAction.CallbackContext context) => ShowStable();
        private void OnRight(InputAction.CallbackContext context) => ShowRaceArea();
        public void Initialize(RacePrototype prototype, RaceManager race, PlayerProgression player)
        {
            if (_settings == null || _cameras == null || _stableUI == null || _managementUI == null || _raceUI == null || _stableGroup == null)
                throw new InvalidOperationException("World references are missing. Run Horsecremental > Setup Milestone 3 World.");
            _prototype = prototype;
            Race = race;
            Player = player;
            Race.Changed += OnRaceChanged;
            _initialized = true;
            _cameras.Initialize(race.Player.Movement, _settings.CameraBlend);
            ShowStable();
        }
        public void ShowRaceArea()
        {
            if (!IsManagement) return;
            ViewingRace = true;
            Area = ManagementArea.Race;
            _cameras.ShowEntry();
            RefreshVisibility();
        }
        public void ShowStable()
        {
            if (!_initialized || (Phase != WorldPhase.StableManagement && Phase != WorldPhase.MainRaceFinished)) return;
            if (Phase == WorldPhase.MainRaceFinished)
            {
                Race.enabled = false;
                _returning = true;
                _prepareRemaining = _settings.CameraBlend;
            }
            Phase = WorldPhase.StableManagement;
            ViewingRace = false;
            Area = ManagementArea.Stable;
            _cameras.ShowStable();
            RefreshVisibility();
        }
        public void StartMainRace()
        {
            if (!IsManagement || !ViewingRace) return;
            Phase = WorldPhase.MainRacePreparing;
            _prepareRemaining = _settings.CameraBlend;
            _prototype.RefreshPlayer();
            Race.PrepareRace();
            Race.enabled = true;
            _cameras.ShowRace();
            RefreshVisibility();
        }
        public void ShowFarm() => ShowWorkstation(ManagementArea.Farm);
        public void ShowBlacksmith() => ShowWorkstation(ManagementArea.Blacksmith);
        private void ShowWorkstation(ManagementArea area)
        {
            if (!IsManagement) return;
            Area = area;
            ViewingRace = false;
            if (area == ManagementArea.Farm) _cameras.ShowFarm();
            else _cameras.ShowBlacksmith();
            RefreshVisibility();
        }
        private void OnRaceChanged()
        {
            if (Phase == WorldPhase.StableManagement || Phase == WorldPhase.MainRacePreparing) return;
            Phase = Race.Player.HasFinished ? WorldPhase.MainRaceFinished :
                Race.Phase == RacePhase.Countdown ? WorldPhase.MainRaceCountdown : WorldPhase.MainRaceRunning;
            Changed?.Invoke();
        }
        private void Update()
        {
            if (!_initialized) return;
            float dt = Time.deltaTime;
            if (_returning)
            {
                _prepareRemaining -= dt;
                if (_prepareRemaining <= 0f)
                {
                    _returning = false;
                    _prototype.RefreshPlayer();
                    Race.PrepareRace();
                    RefreshVisibility();
                }
            }
            else if (Phase == WorldPhase.MainRacePreparing)
            {
                _prepareRemaining -= dt;
                if (_prepareRemaining <= 0f)
                {
                    Phase = WorldPhase.MainRaceCountdown;
                    Race.StartRace();
                }
            }
            if (IsManagement) Player.AdvanceTraining(dt, _settings);
            Player.AdvanceFarm(dt, _settings);
            Player.AdvanceCraft(dt);
        }
        private void RefreshVisibility()
        {
            _stableUI.SetActive(Phase == WorldPhase.StableManagement);
            _stableGroup.interactable = _stableGroup.blocksRaycasts = IsManagement && Area == ManagementArea.Stable;
            _managementUI.SetActive(Phase == WorldPhase.StableManagement);
            _raceUI.SetActive(ViewingRace && !_returning);
            Changed?.Invoke();
        }
        private void OnDestroy()
        {
            if (Race != null) Race.Changed -= OnRaceChanged;
            _left.Dispose();
            _right.Dispose();
        }
    }
}
