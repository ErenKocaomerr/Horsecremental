using System;

namespace Horsecremental.Racing
{
    public sealed partial class PlayerProgression
    {
        private TrainingRaceRules[] _trainingRaces;
        private UpgradeRule[] _trainingUpgradeRules;
        private int[] _trainingBestPlaces;
        private int _activeTrainingTier = -1;
        public bool ManualTrainingEnabled => _trainingRaces != null;
        public int TrainingTierCount => _trainingRaces == null ? 0 : _trainingRaces.Length;
        public int TrainingTokens { get; private set; }
        public int HorseUpgradeBalance => ManualTrainingEnabled ? TrainingTokens : Coins;
        public event Action<int> TrainingTokensChanged;
        public void EnableManualTraining(TrainingRaceRules[] races, int[] upgradeCosts, float costGrowth)
        {
            if (races == null) throw new ArgumentNullException(nameof(races));
            if (upgradeCosts == null) throw new ArgumentNullException(nameof(upgradeCosts));
            if (races.Length != 3 || upgradeCosts.Length != 3) throw new ArgumentException("Three tiers and three upgrade costs are required.");
            if (ManualTrainingEnabled) return;
            UpgradeRule[] upgrades = new UpgradeRule[3];
            for (int i = 0; i < races.Length; i++)
            {
                if (races[i] == null) throw new ArgumentException("Training rules cannot be null.", nameof(races));
                upgrades[i] = new UpgradeRule(upgradeCosts[i], costGrowth, _rules[i].Increase);
            }
            _trainingRaces = (TrainingRaceRules[])races.Clone();
            _trainingUpgradeRules = upgrades;
            _trainingBestPlaces = new int[races.Length];
            if (Training != null) Training.LapCompleted -= CompleteLap;
            // All three horse stats are available; tokens pay for the actual levels.
            _unlockedNodes[(int)UpgradeNode.Speed] = true;
            _unlockedNodes[(int)UpgradeNode.Acceleration] = true;
            _unlockedNodes[(int)UpgradeNode.BoostPower] = true;
            Changed?.Invoke();
        }
        public bool IsTrainingUnlocked(int tier)
        {
            ValidateTrainingTier(tier);
            return tier == 0 || _trainingBestPlaces[tier - 1] == 1;
        }
        public int TrainingBestPlace(int tier) { ValidateTrainingTier(tier); return _trainingBestPlaces[tier]; }
        public TrainingRaceRules TrainingRules(int tier) { ValidateTrainingTier(tier); return _trainingRaces[tier]; }
        public long BeginTrainingRace(int tier)
        {
            if (!IsTrainingUnlocked(tier)) throw new InvalidOperationException("Win the previous Training tier first.");
            long raceId = BeginRace();
            _activeTrainingTier = tier;
            return raceId;
        }
        public bool TryAwardTrainingRace(long raceId, int tier, int place)
        {
            ValidateTrainingTier(tier);
            TrainingRaceRules rewards = _trainingRaces[tier];
            int coins = rewards.CoinsForPlace(place);
            int tokens = rewards.TokensForPlace(place);
            if (_rewardClaimed || raceId != _raceId || _activeTrainingTier != tier) return false;
            _rewardClaimed = true;
            if (_trainingBestPlaces[tier] == 0 || place < _trainingBestPlaces[tier]) _trainingBestPlaces[tier] = place;
            int before = TrainingTokens;
            TrainingTokens = (int)Math.Min(int.MaxValue, (long)TrainingTokens + tokens);
            if (TrainingTokens != before) TrainingTokensChanged?.Invoke(TrainingTokens - before);
            CreditResources(coins, 0);
            return true;
        }
        private void ValidateTrainingTier(int tier)
        {
            if (tier < 0 || tier >= TrainingTierCount) throw new ArgumentOutOfRangeException(nameof(tier));
        }
    }
}
