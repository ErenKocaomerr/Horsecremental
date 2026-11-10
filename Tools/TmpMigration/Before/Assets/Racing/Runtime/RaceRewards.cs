using System;
using UnityEngine;

namespace Horsecremental.Racing
{
    public sealed class RaceRewards : MonoBehaviour
    {
        private RaceManager _race;
        private PlayerProgression _player;
        private ProgressionSettings _settings;
        private long _raceId;
        private int _trainingTier = -1;
        public int EarnedCoins { get; private set; }
        public int EarnedMetal { get; private set; }
        public int EarnedTokens { get; private set; }
        public int EarnedTrainingTokens { get; private set; }
        public void SelectTrainingTier(int tier) => _trainingTier = tier;
        public event Action Awarded;

        public void Initialize(RaceManager race, PlayerProgression player, ProgressionSettings settings)
        {
            _race = race ?? throw new ArgumentNullException(nameof(race));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _race.RaceStarted += BeginRace;
            _race.ParticipantFinished += OnFinished;
        }

        private void BeginRace()
        {
            _raceId = _trainingTier < 0 ? _player.BeginRace() : _player.BeginTrainingRace(_trainingTier);
            EarnedCoins = 0;
            EarnedMetal = 0;
            EarnedTokens = 0;
            EarnedTrainingTokens = 0;
        }

        private void OnFinished(RaceParticipant participant)
        {
            if (!participant.IsPlayer) return;
            if (_trainingTier >= 0)
            {
                int coinsBefore = _player.Coins;
                int tokensBefore = _player.TrainingTokens;
                if (!_player.TryAwardTrainingRace(_raceId, _trainingTier, participant.FinishPosition)) return;
                EarnedCoins = _player.Coins - coinsBefore;
                EarnedTrainingTokens = _player.TrainingTokens - tokensBefore;
                Awarded?.Invoke();
                return;
            }
            int reward = _settings.RewardForPlace(participant.FinishPosition);
            int metal = _settings.MetalForPlace(participant.FinishPosition);
            int beforeCoins = _player.Coins;
            int beforeMetal = _player.Metal;
            int beforeTokens = _player.RaceTokens;
            if (!_player.TryAwardRace(_raceId, reward, metal, _settings.TokensForPlace(participant.FinishPosition))) return;
            EarnedCoins = _player.Coins - beforeCoins;
            EarnedMetal = _player.Metal - beforeMetal;
            EarnedTokens = _player.RaceTokens - beforeTokens;
            Awarded?.Invoke();
        }

        private void OnDestroy()
        {
            if (_race == null) return;
            _race.RaceStarted -= BeginRace;
            _race.ParticipantFinished -= OnFinished;
        }
    }
}
