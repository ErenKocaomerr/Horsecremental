using System;
using UnityEngine;

namespace Horsecremental.Racing
{
    public sealed class RacePrototype : MonoBehaviour
    {
        [SerializeField] private RaceSettings _settings;
        [SerializeField] private RaceManager _race;
        [SerializeField] private RaceParticipant[] _participants;
        [SerializeField] private RaceCamera _camera;
        [SerializeField] private RaceUI _ui;
        [SerializeField] private ProgressionSettings _progressionSettings;
        [SerializeField] private GameplayWorld _world;
        [SerializeField] private TrainingLadderSettings _trainingLadder;
        [SerializeField] private GameObject _mainTrack;
        [SerializeField] private GameObject[] _trainingTracks;
        private RaceObstacle[] _obstacles;
        private RaceRewards _rewards;
        private PlayerProgression _player;
        private int _trainingTier = -1;

        // All scene components have completed Awake before we bind their runtime state.
        private void Start()
        {
            if (_settings == null || _race == null || _camera == null || _ui == null || _participants == null)
                throw new InvalidOperationException("Rebuild the race using Horsecremental > Rebuild Milestone 1 Race.");
            if (_participants.Length != _settings.OpponentStats.Length + 1)
                throw new InvalidOperationException("Rebuild the race after changing its participant count.");
            _player = ProgressionSession.GetOrCreate(_progressionSettings);
            if (_trainingLadder != null) _trainingLadder.Enable(_player);
            ConfigureParticipants(_settings);
            _rewards = _race.gameObject.AddComponent<RaceRewards>();
            _rewards.Initialize(_race, _player, _progressionSettings);
            _camera.Initialize(_participants[0].GetComponent<HorseView>().RenderRoot, _race);
            _ui.Initialize(_race, _rewards, _player);
            if (_world != null) _world.Initialize(this, _race, _player);
        }

        private void ConfigureParticipants(RaceSettings settings)
        {
            if (settings.OpponentStats.Length + 1 != _participants.Length)
                throw new InvalidOperationException("Race settings must match the existing participants.");
            _obstacles = settings.CreateObstacles();
            for (int i = 0; i < _participants.Length; i++)
            {
                RaceParticipant participant = _participants[i];
                HorseStats stats = participant.IsPlayer ? _progressionSettings.BaseHorse : settings.OpponentStats[i - 1];
                participant.Movement.Initialize(stats, participant.IsPlayer ? _player.Stats : stats.Parameters,
                    _obstacles, RaceTrackBuilder.FIRST_LANE_Y + i * RaceTrackBuilder.LANE_SPACING);
                if (participant.AI != null) participant.AI.Initialize(settings.AiJumpLeadSeconds);
            }
            _race.Initialize(_participants, settings.TrackLength);
        }
        public void ConfigureRace(int trainingTier)
        {
            _trainingTier = trainingTier;
            ConfigureParticipants(trainingTier < 0 ? _settings : _trainingLadder.Race(trainingTier));
            _rewards.SelectTrainingTier(trainingTier);
            _ui.Initialize(_race, _rewards, _player);
        }
        public void ShowTrack(bool visible)
        {
            if (_mainTrack != null) _mainTrack.SetActive(visible && _trainingTier < 0);
            if (_trainingTracks == null) return;
            for (int i = 0; i < _trainingTracks.Length; i++)
                _trainingTracks[i].SetActive(visible && _trainingTier == i);
        }

        public void RefreshPlayer()
        {
            _race.Player.Movement.Initialize(_progressionSettings.BaseHorse, ProgressionSession.Player.Stats,
                _obstacles, RaceTrackBuilder.FIRST_LANE_Y);
        }
    }
}
