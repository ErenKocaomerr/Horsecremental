using NUnit.Framework;

namespace Horsecremental.Racing.Tests
{
    internal static class UpgradeTreeTestSupport
    {
        // Existing feature tests exercise their systems after all one-time unlocks.
        // Pay exactly the unlock costs, preserving the fixture's original balances.
        internal static PlayerProgression UnlockAll(PlayerProgression player)
        {
            int coins = 0; int tokens = 0;
            for (int i = 0; i < UpgradeTreeRules.NODE_COUNT; i++)
            {
                UpgradeNode node = (UpgradeNode)i;
                if (player.IsUnlocked(node)) continue;
                UpgradeNodeRule rule = player.NodeRule(node);
                if (rule.Currency == UpgradeCurrency.Coins) coins += rule.Cost;
                else tokens += rule.Cost;
            }
            player.GrantDebugResources(coins, tokens);
            for (int pass = 0; pass < UpgradeTreeRules.NODE_COUNT; pass++)
                for (int i = 0; i < UpgradeTreeRules.NODE_COUNT; i++) player.TryUnlock((UpgradeNode)i);
            for (int i = 0; i < UpgradeTreeRules.NODE_COUNT; i++) Assert.That(player.IsUnlocked((UpgradeNode)i), Is.True);
            return player;
        }
    }
}
