using System;

namespace Horsecremental.Racing
{
    // One runtime model owns currency and levels. It has no scene or Unity dependencies.
    public sealed partial class PlayerProgression
    {
        private readonly HorseParameters _baseStats;
        private readonly UpgradeRule[] _rules;
        private readonly int[] _levels = { 1, 1, 1 };
        private long _raceId;
        private bool _rewardClaimed = true;

        public int Coins { get; private set; }
        public int Metal { get; private set; }
        public int Carrots { get; private set; }
        public int StoredCarrots { get; private set; }
        public float FeedingSpeed { get; private set; }
        public double TrainingElapsed { get; private set; }
        public long TrainingRaceNumber { get; private set; } = 1;
        public double FarmElapsed { get; private set; }
        public event Action Changed;
        public event Action<int> CoinsChanged;
        public event Action<int> MetalChanged;
        public event Action<int> CarrotsChanged;
        public event Action TrainingCompleted;

        public PlayerProgression(HorseParameters baseStats, UpgradeRule speed,
            UpgradeRule acceleration, UpgradeRule boost, IWorkshopSettings workshop = null, UpgradeTreeRules tree = null)
        {
            _baseStats = baseStats;
            _rules = new[] { speed, acceleration, boost };
            _workshop = workshop;
            _tree = tree ?? UpgradeTreeRules.DEFAULT;
            for (int i = 0; i < _unlockedNodes.Length; i++)
                _unlockedNodes[i] = _tree.Rule((UpgradeNode)i).StartsUnlocked;
        }

        public int Level(HorseUpgrade upgrade) => _levels[Index(upgrade)];
        public UpgradeRule Rule(HorseUpgrade upgrade) => _rules[Index(upgrade)];
        public int Cost(HorseUpgrade upgrade) => Rule(upgrade).CostAtLevel(Level(upgrade));
        public bool CanAfford(HorseUpgrade upgrade) => IsUnlocked(upgrade) && Cost(upgrade) < int.MaxValue && Coins >= Cost(upgrade);

        public HorseParameters Stats
        {
            get
            {
                return new HorseParameters(Breakdown(HorseUpgrade.Speed).Total,
                    Breakdown(HorseUpgrade.Acceleration).Total, Breakdown(HorseUpgrade.BoostPower).Total,
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

        public bool TryAwardRace(long raceId, int reward, int metal = 0, int tokens = 0)
        {
            if (reward < 0) throw new ArgumentOutOfRangeException(nameof(reward));
            if (metal < 0) throw new ArgumentOutOfRangeException(nameof(metal));
            if (tokens < 0) throw new ArgumentOutOfRangeException(nameof(tokens));
            if (_rewardClaimed || raceId != _raceId) return false;
            _rewardClaimed = true;
            CreditResources(reward, metal, tokens);
            return true;
        }

        // Training never opens or consumes a Main Race reward token.
        public void AdvanceTraining(float seconds, IIdleSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (seconds <= 0f || float.IsNaN(seconds) || float.IsInfinity(seconds)) return;
            TrainingElapsed += seconds;
            float duration = TrainingSeconds(settings);
            while (TrainingElapsed >= duration)
            {
                TrainingElapsed -= duration;
                TrainingRaceNumber++;
                CreditResources(TrainingCoins(settings), settings.TrainingMetal);
                TrainingCompleted?.Invoke();
            }
        }

        public void AdvanceFarm(float seconds, IIdleSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (!IsUnlocked(UpgradeNode.Farm)) return;
            int capacity = FarmCapacity(settings);
            if (seconds <= 0f || float.IsNaN(seconds) || float.IsInfinity(seconds) || StoredCarrots >= capacity) return;
            FarmElapsed += seconds;
            bool changed = false;
            float duration = CarrotSeconds(settings);
            while (FarmElapsed >= duration && StoredCarrots < capacity)
            {
                FarmElapsed -= duration;
                StoredCarrots++;
                changed = true;
            }
            if (StoredCarrots >= capacity) FarmElapsed = 0d;
            if (changed) Changed?.Invoke();
        }

        public int CollectCarrots()
        {
            if (!IsUnlocked(UpgradeNode.Farm)) return 0;
            int amount = (int)Math.Min(StoredCarrots, (long)int.MaxValue - Carrots);
            if (amount == 0) return 0;
            Carrots += amount;
            StoredCarrots -= amount;
            CarrotsChanged?.Invoke(amount);
            Changed?.Invoke();
            return amount;
        }

        public bool TryFeed(IIdleSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (!IsUnlocked(UpgradeNode.Farm) || Carrots < settings.FeedCost) return false;
            Carrots -= settings.FeedCost;
            FeedingSpeed += settings.FeedSpeed;
            CarrotsChanged?.Invoke(-settings.FeedCost);
            Changed?.Invoke();
            return true;
        }

        private void CreditResources(int reward, int metal, int tokens = 0)
        {
            int before = Coins;
            int metalBefore = Metal;
            int tokensBefore = RaceTokens;
            Coins = (int)Math.Min(int.MaxValue, (long)Coins + reward);
            Metal = (int)Math.Min(int.MaxValue, (long)Metal + metal);
            RaceTokens = (int)Math.Min(int.MaxValue, (long)RaceTokens + tokens);
            if (Coins != before) CoinsChanged?.Invoke(Coins - before);
            if (Metal != metalBefore) MetalChanged?.Invoke(Metal - metalBefore);
            if (RaceTokens != tokensBefore) RaceTokensChanged?.Invoke(RaceTokens - tokensBefore);
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
