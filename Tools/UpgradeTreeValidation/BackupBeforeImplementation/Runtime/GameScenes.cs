using UnityEngine;

namespace Horsecremental.Racing
{
    // Legacy entry points navigate the loaded world. Gameplay never loads scenes.
    public static class GameScenes
    {
        public const string WORLD = "Milestone3World";
        public const string STABLE = "Milestone2Stable";
        public const string RACE = "Milestone1Race";
        public static bool ConsumeRaceStart() => false;
        public static void StartRace()
        {
            GameplayWorld world = Object.FindAnyObjectByType<GameplayWorld>();
            if (world != null) world.ShowRaceArea();
        }
        public static void ReturnToStable()
        {
            GameplayWorld world = Object.FindAnyObjectByType<GameplayWorld>();
            if (world != null) world.ShowStable();
        }
    }
}
