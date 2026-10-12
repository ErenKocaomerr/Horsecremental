using System;

namespace Horsecremental.Racing
{
    public readonly struct UIDetailContent
    {
        public string Title { get; }
        public string Description { get; }
        public string Status { get; }
        public string Cost { get; }
        public bool Unavailable { get; }
        public UIDetailContent(string title, string description, string status, string cost = "", bool unavailable = false)
        { Title = title; Description = description; Status = status; Cost = cost; Unavailable = unavailable; }
    }

    public static class UIDetailFormatter
    {
        public static UIDetailContent Create(DetailKind kind, int id, PlayerProgression player, GameplayWorld world)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (world == null) throw new ArgumentNullException(nameof(world));
            switch (kind)
            {
                case DetailKind.Tree: return Tree((UpgradeNode)id, player);
                case DetailKind.Horse: return Horse((HorseUpgrade)id, player);
                case DetailKind.Production: return Production((ProductionUpgrade)id, player, world);
                case DetailKind.Equipment: return Equipment((EquipmentType)id, player);
                case DetailKind.Training: if (player.ManualTrainingEnabled) return new UIDetailContent("Training Races", "Choose a short race. Finish 1st to unlock the next tier.", "Coins + Training Tokens", "Tokens train your horse");
                    return new UIDetailContent("Training", "Automatic training pauses during Main Races.",
                    player.TrainingHorseCount + " horses • " + player.TrainingSeconds(world.Settings).ToString("0.#") + "s / lap",
                    "+" + player.TrainingCoins(world.Settings) + " Coins per horse lap");
                case DetailKind.Feed: return new UIDetailContent("Feed Horse", "Permanent +" + world.Settings.FeedSpeed.ToString("0.##") + " Speed per feed.",
                    "Food bonus +" + player.FeedingSpeed.ToString("0.##"), world.Settings.FeedCost + " Carrots", player.Carrots < world.Settings.FeedCost);
                case DetailKind.Farm: return new UIDetailContent("Carrot Farm", "Collect ripe carrots, then feed your horse.",
                    player.PlaygroundEnabled ? "Click plants to grow / harvest. Sweep dropped carrots." : "Storage " + player.StoredCarrots + " / " + player.FarmCapacity(world.Settings),
                    player.PlaygroundEnabled ? player.HarvestYield + " Carrots / harvest" : "1 Carrot / " + player.CarrotSeconds(world.Settings).ToString("0.#") + "s");
                case DetailKind.RaceHelp: return new UIDetailContent("Main Race", "SPACE: rhythmic boost taps\nE: jump near yellow hurdle markers.",
                    "Recommended power " + world.Settings.RecommendedPower.ToString("0"), "Tokens by place: 5 / 3 / 2 / 1");
                case DetailKind.Navigation: return Navigation((ManagementArea)id, player);
                case DetailKind.Resource: return Resource(id, player);
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
        private static UIDetailContent Tree(UpgradeNode node, PlayerProgression player)
        {
            UpgradeNodeRule rule = player.NodeRule(node);
            bool unlocked = player.IsUnlocked(node);
            bool ready = player.HasPrerequisite(node);
            string status = unlocked ? "UNLOCKED • 1 / 1" : !ready ? "Requires " + UpgradeTreeRules.Name(rule.Prerequisite) : "One-time unlock • 0 / 1";
            string description = node == UpgradeNode.Farm ? "Open the Farm to grow carrots and feed your horse." :
                node == UpgradeNode.Blacksmith ? "Open the Blacksmith to craft and equip better gear." :
                "Enable " + UpgradeTreeRules.Name(node) + " upgrades in the " + AreaName(node) + ". Levels are improved there.";
            int balance = rule.Currency == UpgradeCurrency.Coins ? player.Coins : player.RaceTokens;
            string currency = rule.Currency == UpgradeCurrency.Coins ? "Coins" : "Race Tokens";
            string cost = unlocked ? "COMPLETE" : rule.Cost + " " + currency + (ready && balance < rule.Cost ? " • Need " + (rule.Cost - balance) : "");
            return new UIDetailContent(UpgradeTreeRules.Name(node), description, status, cost, !unlocked && (!ready || balance < rule.Cost));
        }
        private static string AreaName(UpgradeNode node) => node == UpgradeNode.GrowSpeed || node == UpgradeNode.FarmCapacity ? "Farm" :
            node == UpgradeNode.ForgeSpeed || node == UpgradeNode.MetalEfficiency ? "Blacksmith" : "Stable";
        private static UIDetailContent Horse(HorseUpgrade upgrade, PlayerProgression player)
        {
            UpgradeNode node = UpgradeTreeRules.NodeFor(upgrade);
            StatBreakdown value = player.Breakdown(upgrade);
            int cost = player.Cost(upgrade);
            bool unlocked = player.IsUnlocked(upgrade);
            string description = "Current " + value.Total.ToString("0.##") + (cost == int.MaxValue ? "" : " → " + (value.Total + player.Rule(upgrade).Increase).ToString("0.##")) +
                "\nBase " + value.Base.ToString("0.##") + " • Upgrades +" + value.Upgrades.ToString("0.##") +
                "\nFood +" + value.Food.ToString("0.##") + " • Equipment +" + value.Equipment.ToString("0.##");
            return new UIDetailContent(UpgradeTreeRules.Name(node), description,
                unlocked ? "Lv. " + player.Level(upgrade) : "Unlock in Upgrade Tree",
                Price(cost, player.HorseUpgradeBalance, player.ManualTrainingEnabled ? "Training Tokens" : "Coins"),
                !unlocked || cost != int.MaxValue && cost > player.HorseUpgradeBalance);
        }
        private static UIDetailContent Production(ProductionUpgrade upgrade, PlayerProgression player, GameplayWorld world)
        {
            WorldSettings settings = world.Settings;
            ProductionRule rule = player.ProductionRule(upgrade);
            int level = player.ProductionLevel(upgrade);
            int maxLevel = upgrade == ProductionUpgrade.TrainingHorseCount && player.PlaygroundEnabled ?
                Math.Min(rule.MaxLevel, world.PlaygroundSettings.MaxHorseCount - world.PlaygroundSettings.BaseHorseCount) : rule.MaxLevel;
            string effect;
            switch (upgrade)
            {
                case ProductionUpgrade.ForgeSpeed: effect = "Craft time " + Math.Max(50, 100 - level * rule.Effect * 100).ToString("0") + "%\nNext: −" + (rule.Effect * 100).ToString("0") + "% base craft time"; break;
                case ProductionUpgrade.MetalEfficiency: effect = "Metal cost " + Math.Max(75, 100 - level * rule.Effect * 100).ToString("0") + "% (rounded up)\nNext: −" + (rule.Effect * 100).ToString("0") + "% base metal cost"; break;
                case ProductionUpgrade.GrowSpeed: effect = "Crop ready / " + player.CarrotSeconds(settings).ToString("0.#") + "s\nNext: −" + (rule.Effect * 100).ToString("0") + "% base grow time"; break;
                case ProductionUpgrade.TrainingHorseCount: effect = player.TrainingHorseCount + " visible horses\nNext: +1 Training Horse"; break;
                case ProductionUpgrade.ClickGrowthPower: effect = (player.ClickGrowth * 100).ToString("0") + "% Growth / click\nNext: +" + (rule.Effect * 100).ToString("0") + "% Growth"; break;
                case ProductionUpgrade.HarvestYield: effect = player.HarvestYield + " Carrots / harvest\nNext: +" + rule.Effect.ToString("0") + " world drop"; break;
                case ProductionUpgrade.FarmCapacity: effect = "Storage " + player.FarmCapacity(settings) + " Carrots\nNext: +" + rule.Effect.ToString("0") + " storage"; break;
                case ProductionUpgrade.TrainingSpeed: effect = "Lap / " + player.TrainingSeconds(settings).ToString("0.#") + "s\nNext: −" + (rule.Effect * 100).ToString("0") + "% base training time"; break;
                default: effect = player.TrainingCoins(settings) + " Coins / horse lap\nNext: +" + (rule.Effect * 100).ToString("0") + "% base coin reward"; break;
            }
            if (level >= maxLevel) effect = effect.Split('\n')[0];
            int cost = player.ProductionCost(upgrade);
            bool unlocked = player.IsUnlocked(upgrade);
            return new UIDetailContent(rule.Name, effect, unlocked ? "Lv. " + level + " / " + maxLevel : "Unlock in Upgrade Tree",
                Price(cost, player.Coins), !unlocked || cost != int.MaxValue && cost > player.Coins);
        }
        private static UIDetailContent Equipment(EquipmentType type, PlayerProgression player)
        {
            EquipmentTier current = player.Equipment(type);
            string description = current.Name + " • +" + current.Bonus.ToString("0.#") + " " + EquipmentStat(type);
            if (!player.HasNextTier(type)) return new UIDetailContent(type.ToString(), description, "MAX TIER", "COMPLETE");
            EquipmentTier next = player.NextEquipment(type);
            description += "\nNext: " + next.Name + "\n+" + next.Bonus.ToString("0.#") + " " + EquipmentStat(type) + " • " + player.CraftSeconds(type).ToString("0.#") + "s\nAutomatically equipped when complete.";
            int metal = player.CraftMetalCost(type);
            bool missing = player.Coins < next.Coins || player.Metal < metal;
            string cost = next.Coins + " Coins / " + metal + " Metal";
            if (missing) cost += "\nNeed " + Math.Max(0, next.Coins - player.Coins) + " Coins / " + Math.Max(0, metal - player.Metal) + " Metal";
            return new UIDetailContent(type.ToString(), description, player.IsCrafting ? "BLACKSMITH BUSY" : "Tier " + player.EquipmentLevel(type), cost, missing || player.IsCrafting);
        }
        private static string EquipmentStat(EquipmentType type) => type == EquipmentType.Horseshoes ? "Speed" : type == EquipmentType.Saddle ? "Acceleration" : "Boost Power";
        private static string Price(int cost, int balance, string currency = "Coins") => cost == int.MaxValue ? "MAX LEVEL" : cost + " " + currency + (balance < cost ? " • Need " + (cost - balance) : "");
        private static UIDetailContent Navigation(ManagementArea area, PlayerProgression player)
        {
            bool locked = area == ManagementArea.Farm && !player.IsUnlocked(UpgradeNode.Farm) || area == ManagementArea.Blacksmith && !player.IsUnlocked(UpgradeNode.Blacksmith);
            return new UIDetailContent(area == ManagementArea.UpgradeTree ? "Upgrade Tree" : area.ToString(),
                locked ? "Open this area through the Upgrade Tree." : "Visit this area.", locked ? "LOCKED" : "AVAILABLE");
        }
        private static UIDetailContent Resource(int id, PlayerProgression player)
        {
            switch (id)
            {
                case 0: return new UIDetailContent("Coins", "Earn from races. Spend on Stable and Farm economy.", player.Coins.ToString("N0"));
                case 4: return new UIDetailContent("Training Tokens", "Earn from Training Races. Train Speed, Acceleration and Boost Power.", player.TrainingTokens.ToString("N0"));
                case 1: return new UIDetailContent("Race Tokens", "Earn only from Main Races. Reserved for future race progression.", player.RaceTokens.ToString("N0"));
                case 2: return new UIDetailContent("Metal", "Earn from races. Used to craft equipment.", player.Metal.ToString("N0"));
                default: return new UIDetailContent("Carrots", "Collect from the Farm and feed your horse.", player.Carrots.ToString("N0"));
            }
        }
    }
}
