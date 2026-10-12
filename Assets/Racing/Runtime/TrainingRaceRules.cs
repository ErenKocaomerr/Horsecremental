using System;

namespace Horsecremental.Racing
{
    // Immutable, Unity-independent rewards for one authored four-horse race.
    public sealed class TrainingRaceRules
    {
        public const int PARTICIPANT_COUNT = 4;
        private readonly int[] _coins;
        private readonly int[] _tokens;
        public TrainingRaceRules(int[] coins, int[] tokens)
        {
            _coins = CopyRewards(coins, nameof(coins));
            _tokens = CopyRewards(tokens, nameof(tokens));
        }
        public int CoinsForPlace(int place) => _coins[PlaceIndex(place)];
        public int TokensForPlace(int place) => _tokens[PlaceIndex(place)];
        public int MinimumCoins => _coins[PARTICIPANT_COUNT - 1];
        public int MaximumCoins => _coins[0];
        public int MinimumTokens => _tokens[PARTICIPANT_COUNT - 1];
        public int MaximumTokens => _tokens[0];
        private static int[] CopyRewards(int[] values, string name)
        {
            if (values == null) throw new ArgumentNullException(name);
            if (values.Length != PARTICIPANT_COUNT) throw new ArgumentException("Configure all four finishing positions.", name);
            int[] copy = (int[])values.Clone();
            for (int i = 0; i < copy.Length; i++)
                if (copy[i] < 1 || i > 0 && copy[i] > copy[i - 1])
                    throw new ArgumentException("Rewards must be positive and descending by finishing position.", name);
            return copy;
        }
        private static int PlaceIndex(int place)
        {
            if (place < 1 || place > PARTICIPANT_COUNT) throw new ArgumentOutOfRangeException(nameof(place));
            return place - 1;
        }
    }
}
