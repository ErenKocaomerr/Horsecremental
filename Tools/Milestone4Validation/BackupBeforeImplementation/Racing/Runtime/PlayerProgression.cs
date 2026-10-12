using System;

namespace Horsecremental.Racing
{
    // One runtime model owns currency and levels. It has no scene or Unity dependencies.
    public sealed class PlayerProgression
    {
        private readonly HorseParameters _baseStats;
        private readonly UpgradeRule[] _rules;
        private readonly int[] _levels = { 1, 1, 1 };
        private long _raceId;
        private bool _rewardClaimed = true;

        public int Coins { get; private set; }
        public int Carrots { get; private set; }
        public int StoredCarrots { get; private set; }
        public float FeedingSpeed { get; private set; }
        public double TrainingElapsed { get; private set; }
        public long TrainingRaceNumber { get; private set; } = 1;
        public double FarmElapsed { get; private set; }
        public event Action Changed;
        public event Action<int> CoinsChanged;

        public PlayerProgression(HorseParameters baseStats, UpgradeRule speed,
            UpgradeRule acceleration, UpgradeRule boost)
        {
            _baseStats = baseStats;
            _rules = new[] { speed, acceleration, boost };
        }

        public int Level(HorseUpgrade upgrade) => _levels[Index(upgrade)];
        public UpgradeRule Rule(HorseUpgrade upgrade) => _rules[Index(upgrade)];
        public int Cost(HorseUpgrade upgrade) => Rule(upgrade).CostAtLevel(Level(upgrade));
        public bool CanAfford(HorseUpgrade upgrade) => Cost(upgrade) < int.MaxValue && Coins >= Cost(upgrade);

        public HorseParameters Stats
        {
            get
            {
                float boost = _baseStats.BoostPower + (_levels[2] - 1) * _rules[2].Increase;
                return new HorseParameters(_baseStats.Speed + FeedingSpeed + (_levels[0] - 1) * _rules[0].Increase,
                    _baseStats.Acceleration + (_levels[1] - 1) * _rules[1].Increase, boost,
                    _baseStats.BoostPerTap, _baseStats.BoostDecayPerSecond, _baseStats.JumpPower, _baseStats.Gravity,
                    _baseStats.PenaltyMultiplier, _baseStats.PenaltyDuration, _baseStats.BoostHighDrain);
            }
        }

        public bool TryUpgrade(HorseUpgrade upgrade)
        {
            int index = Index(upgrade);
            if (!CanAfford(upgrade)) return false;
            int cost = Cost(upgrade);
            Coins -= cost;
            _levels[index]++;
            CoinsChanged?.Invoke(-cost);
            Changed?.Invoke();
            return true;
        }

        public long BeginRace()
        {
            _raceId++;
            _rewardClaimed = false;
            return _raceId;
        }

        public bool TryAwardRace(long raceId, int reward)
        {
            if (reward < 0) throw new ArgumentOutOfRangeException(nameof(reward));
            if (_rewardClaimed || raceId != _raceId) return false;
            _rewardClaimed = true;
            CreditCoins(reward);
            return true;
        }

        // Training never opens or consumes a Main Race reward token.
        public void AdvanceTraining(float seconds, IIdleSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (seconds <= 0f) return;
            TrainingElapsed += seconds;
            while (TrainingElapsed >= settings.TrainingSeconds)
            {
                TrainingElapsed -= settings.TrainingSeconds;
                TrainingRaceNumber++;
                CreditCoins(settings.TrainingReward);
            }
        }

        public void AdvanceFarm(float seconds, IIdleSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (seconds <= 0f || StoredCarrots >= settings.FarmCapacity) return;
            FarmElapsed += seconds;
            bool changed = false;
            while (FarmElapsed >= settings.CarrotSeconds && StoredCarrots < settings.FarmCapacity)
            {
                FarmElapsed -= settings.CarrotSeconds;
                StoredCarrots++;
                changed = true;
            }
            if (StoredCarrots >= settings.FarmCapacity) FarmElapsed = 0d;
            if (changed) Changed?.Invoke();
        }

        public int CollectCarrots()
        {
            int amount = (int)Math.Min(StoredCarrots, (long)int.MaxValue - Carrots);
            if (amount == 0) return 0;
            Carrots += amount;
            StoredCarrots -= amount;
            Changed?.Invoke();
            return amount;
        }

        public bool TryFeed(IIdleSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (Carrots < settings.FeedCost) return false;
            Carrots -= settings.FeedCost;
            FeedingSpeed += settings.FeedSpeed;
            Changed?.Invoke();
            return true;
        }

        private void CreditCoins(int reward)
        {
            int before = Coins;
            Coins = (int)Math.Min(int.MaxValue, (long)Coins + reward);
            if (Coins != before) CoinsChanged?.Invoke(Coins - before);
            Changed?.Invoke();
        }

        private static int Index(HorseUpgrade upgrade)
        {
            int index = (int)upgrade;
            if (index < 0 || index > 2) throw new ArgumentOutOfRangeException(nameof(upgrade));
            return index;
        }
    }
}
