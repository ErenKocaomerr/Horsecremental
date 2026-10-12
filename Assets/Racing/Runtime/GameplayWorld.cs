using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Horsecremental.Racing
{
    public enum WorldPhase { StableManagement, MainRacePreparing, MainRaceCountdown, MainRaceRunning, MainRaceFinished }
    public enum ManagementArea { Stable, Farm, Blacksmith, Race, UpgradeTree, Training }
    public sealed class GameplayWorld : MonoBehaviour
    {
        [SerializeField] private WorldSettings _settings;
        [SerializeField] private WorldCameraNavigation _cameras;
        [SerializeField] private GameObject _stableUI;
        [SerializeField] private GameObject _managementUI;
        [SerializeField] private GameObject _raceUI;
        [SerializeField] private CanvasGroup _stableGroup;
        [SerializeField] private bool _blacksmithDisabled;
        [SerializeField] private StablePlaygroundSettings _playground;
        [SerializeField] private TrainingTrackView _trainingTrack;
        [SerializeField] private FarmWorldView _farmWorld;
        [SerializeField] private WorldPickupManager _pickups;
        [SerializeField] private StableAudioHooks _audio;
        [SerializeField] private GameObject _mainTrack;
        [SerializeField] private TrainingLadderSettings _trainingLadder;
        [SerializeField] private GameObject _trainingSelectionUI;
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
        public StablePlaygroundSettings PlaygroundSettings => _playground;
        private float TransitionSeconds => _playground == null ? _settings.CameraBlend : _playground.RaceBlend;
        public RaceManager Race { get; private set; }
        public PlayerProgression Player { get; private set; }
        public TrainingLadderSettings TrainingLadder => _trainingLadder;
        public int SelectedTrainingTier { get; private set; } = -1;
        public bool IsTrainingRace => SelectedTrainingTier >= 0;
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
            if (_trainingLadder != null) _trainingLadder.Enable(player);
            if (_playground != null)
            {
                Player.EnablePlayground(_playground, _settings);
                _pickups.Initialize(Player, _playground);
                if (!Player.ManualTrainingEnabled) _trainingTrack.Initialize(Player);
                _farmWorld.Initialize(Player, _playground);
            }
            Race.Changed += OnRaceChanged;
            _initialized = true;
            _cameras.Initialize(race.Player.Movement, TransitionSeconds);
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
                _prepareRemaining = TransitionSeconds;
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
            BeginRace(-1);
        }
        public void ShowTraining()
        {
            if (!IsManagement || _trainingLadder == null) return;
            Area = ManagementArea.Training;
            ViewingRace = false;
            _cameras.ShowStable();
            RefreshVisibility();
        }
        public bool StartTrainingRace(int tier)
        {
            if (!IsManagement || Area != ManagementArea.Training || _trainingLadder == null ||
                tier < 0 || tier >= _trainingLadder.Count || !Player.IsTrainingUnlocked(tier)) return false;
            BeginRace(tier);
            return true;
        }
        private void BeginRace(int trainingTier)
        {
            SelectedTrainingTier = trainingTier;
            ViewingRace = true;
            Phase = WorldPhase.MainRacePreparing;
            _prepareRemaining = TransitionSeconds;
            if (_audio != null) _audio.Play(StableSound.RaceTransition);
            _prototype.ConfigureRace(trainingTier);
            Race.PrepareRace();
            Race.enabled = true;
            _cameras.ShowRace();
            RefreshVisibility();
        }
        public void ShowFarm() => ShowWorkstation(ManagementArea.Farm);
        public void ShowBlacksmith() => ShowWorkstation(ManagementArea.Blacksmith);
        public bool CanVisit(ManagementArea area) => Player != null &&
            (area != ManagementArea.Blacksmith || !_blacksmithDisabled) &&
            (area != ManagementArea.Farm || Player.IsUnlocked(UpgradeNode.Farm)) &&
            (area != ManagementArea.Blacksmith || Player.IsUnlocked(UpgradeNode.Blacksmith));
        public void ShowUpgradeTree()
        {
            if (!IsManagement) return;
            Area = ManagementArea.UpgradeTree;
            ViewingRace = false;
            _cameras.ShowStable();
            RefreshVisibility();
        }
        private void ShowWorkstation(ManagementArea area)
        {
            if (!IsManagement || !CanVisit(area)) return;
            Area = area;
            ViewingRace = false;
            if (_playground != null) _cameras.ShowStable();
            else if (area == ManagementArea.Farm) _cameras.ShowFarm();
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
                    SelectedTrainingTier = -1;
                    _prototype.ConfigureRace(-1);
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
            if (IsManagement && !Player.ManualTrainingEnabled) Player.AdvanceTraining(dt, _settings);
            Player.AdvanceFarm(dt, _settings);
            if (!_blacksmithDisabled) Player.AdvanceCraft(dt);
        }
        private void RefreshVisibility()
        {
            if (_trainingLadder != null) _prototype.ShowTrack(ViewingRace);
            else if (_mainTrack != null) _mainTrack.SetActive(ViewingRace);
            if (_trainingSelectionUI != null) _trainingSelectionUI.SetActive(IsManagement && Area == ManagementArea.Training);
            _stableUI.SetActive(Phase == WorldPhase.StableManagement);
            _stableGroup.interactable = _stableGroup.blocksRaycasts = IsManagement &&
                (Area == ManagementArea.Stable || _playground != null && Area == ManagementArea.Farm);
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
