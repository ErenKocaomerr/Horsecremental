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
        private RaceObstacle[] _obstacles;

        // All scene components have completed Awake before we bind their runtime state.
        private void Start()
        {
            if (_settings == null || _race == null || _camera == null || _ui == null || _participants == null)
                throw new InvalidOperationException("Rebuild the race using Horsecremental > Rebuild Milestone 1 Race.");
            if (_participants.Length != _settings.OpponentStats.Length + 1)
                throw new InvalidOperationException("Rebuild the race after changing its participant count.");
            RaceObstacle[] obstacles = _settings.CreateObstacles();
            _obstacles = obstacles;
            PlayerProgression player = ProgressionSession.GetOrCreate(_progressionSettings);
            for (int i = 0; i < _participants.Length; i++)
            {
                RaceParticipant participant = _participants[i];
                HorseStats stats = participant.IsPlayer ? _progressionSettings.BaseHorse : _settings.OpponentStats[i - 1];
                participant.Movement.Initialize(stats, participant.IsPlayer ? player.Stats : stats.Parameters,
                    obstacles, RaceTrackBuilder.FIRST_LANE_Y + i * RaceTrackBuilder.LANE_SPACING);
                if (participant.AI != null) participant.AI.Initialize(_settings.AiJumpLeadSeconds);
            }
            _race.Initialize(_participants, _settings.TrackLength);
            RaceRewards rewards = _race.gameObject.AddComponent<RaceRewards>();
            rewards.Initialize(_race, player, _progressionSettings);
            _camera.Initialize(_participants[0].GetComponent<HorseView>().RenderRoot, _race);
            _ui.Initialize(_race, rewards, player);
            if (_world != null) _world.Initialize(this, _race, player);
        }

        public void RefreshPlayer()
        {
            _race.Player.Movement.Initialize(_progressionSettings.BaseHorse, ProgressionSession.Player.Stats,
                _obstacles, RaceTrackBuilder.FIRST_LANE_Y);
        }
    }
}
