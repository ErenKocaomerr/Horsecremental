using System.Collections;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Horsecremental.Racing.Tests
{
    public sealed class ProgressionLoopTests
    {
        [UnityTest]
        public IEnumerator SameSceneLoopTrainsFarmsCraftsUpgradesAndWinsRepeatedly()
        {
            ProgressionSession.Reset();
            yield return SceneManager.LoadSceneAsync(GameScenes.WORLD);
            yield return null;
            GameplayWorld world = Object.FindAnyObjectByType<GameplayWorld>();
            PlayerProgression player = world.Player;
            RaceManager race = world.Race;
            bool previousBackground = Application.runInBackground;
            Application.runInBackground = true;
            var sceneHandle = SceneManager.GetActiveScene().handle;
            int originalObjects = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Length;
            int transitions = 0;
            UnityEngine.Events.UnityAction<Scene, LoadSceneMode> onLoaded = (scene, mode) => transitions++;
            SceneManager.sceneLoaded += onLoaded;
            try
            {
                Assert.That(player.Metal, Is.Zero);
                yield return new WaitForSeconds(0.3f);
                Capture("stable-1920x1080", 1920, 1080);
                Capture("stable-1280x720", 1280, 720);
                Capture("stable-1024x768", 1024, 768);
                Capture("stable-2560x1080", 2560, 1080);
                UnlockAll(player);
                FindButton("View Farm").onClick.Invoke();
                yield return new WaitForSeconds(0.8f);
                Assert.That(Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera.Name, Is.EqualTo("Farm Cinemachine"));
                Assert.That(GameObject.Find("Stable Upgrades UI").GetComponent<CanvasGroup>().blocksRaycasts, Is.False);
                Capture("farm-1920x1080", 1920, 1080);
                FindButton("View Blacksmith").onClick.Invoke();
                yield return new WaitForSeconds(0.8f);
                FindButton("Craft Horseshoes").onClick.Invoke();
                Assert.That(player.IsCrafting, Is.False);
                Assert.That(GameObject.Find("Craft Feedback").GetComponent<Text>().text, Does.Contain("Missing:"));
                Capture("blacksmith-1920x1080", 1920, 1080);
                Capture("blacksmith-1024x768", 1024, 768);
                FindButton("View Race").onClick.Invoke();
                yield return new WaitForSeconds(0.8f);
                Capture("race-entry", 1920, 1080);
                for (int cycle = 0; cycle < 3; cycle++)
                {
                    int coins = player.Coins;
                    int metal = player.Metal;
                    double trainingElapsed = player.TrainingElapsed;
                    long trainingNumber = player.TrainingRaceNumber;
                    FindButton("Start Race").onClick.Invoke();
                    world.StartMainRace();
                    Assert.That(world.Phase, Is.EqualTo(WorldPhase.MainRacePreparing));
                    Assert.That(race.Player.Movement.Parameters.Speed, Is.EqualTo(player.Stats.Speed));
                    Time.timeScale = 10f;
                    yield return DriveRace(race);
                    Assert.That(race.Player.FinishPosition, Is.EqualTo(cycle == 0 ? 4 : 1));
                    Assert.That(player.Coins, Is.EqualTo(coins + (cycle == 0 ? 100 : 500)));
                    Assert.That(player.Metal, Is.EqualTo(metal + (cycle == 0 ? 3 : 15)));
                    Assert.That(player.TrainingElapsed, Is.EqualTo(trainingElapsed));
                    Assert.That(player.TrainingRaceNumber, Is.EqualTo(trainingNumber));
                    Time.timeScale = 1f;
                    yield return new WaitForSeconds(0.2f);
                    Capture(cycle == 0 ? "fresh-player-result" : "progressed-win-" + cycle, 1920, 1080);
                    int paidCoins = player.Coins;
                    yield return new WaitForSeconds(0.2f);
                    Assert.That(player.Coins, Is.EqualTo(paidCoins));
                    FindButton("Return to Stable").onClick.Invoke();
                    world.StartMainRace();
                    Assert.That(world.IsManagement, Is.False);
                    yield return new WaitForSeconds(0.8f);
                    Assert.That(world.IsManagement, Is.True);
                    Assert.That(SceneManager.GetActiveScene().handle, Is.EqualTo(sceneHandle));
                    Assert.That(Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Length, Is.EqualTo(originalObjects));
                    Assert.That(race.Participants[2].Movement.Parameters.Speed, Is.EqualTo(24f));
                    if (cycle == 0)
                    {
                        Time.timeScale = 40f;
                        for (int minute = 1; minute <= 24; minute++)
                        {
                            yield return new WaitForSeconds(60f);
                            world.ShowStable();
                            BuyProduction(player, ProductionUpgrade.TrainingSpeed, 2);
                            BuyProduction(player, ProductionUpgrade.TrainingReward, 2);
                            world.ShowFarm();
                            BuyProduction(player, ProductionUpgrade.GrowSpeed, 1);
                            BuyProduction(player, ProductionUpgrade.FarmCapacity, 1);
                            FindButton("Collect Carrots").onClick.Invoke();
                            while (player.Carrots >= world.Settings.FeedCost) FindButton("Feed Horse").onClick.Invoke();
                            world.ShowBlacksmith();
                            BuyProduction(player, ProductionUpgrade.ForgeSpeed, 1);
                            BuyProduction(player, ProductionUpgrade.MetalEfficiency, 1);
                            foreach (EquipmentType type in System.Enum.GetValues(typeof(EquipmentType)))
                            {
                                if (player.EquipmentLevel(type) >= 2) continue;
                                FindButton("Craft " + type).onClick.Invoke();
                                if (player.IsCrafting) yield return new WaitForSeconds(player.CraftRemaining + 0.1f);
                            }
                            world.ShowStable();
                            if (player.CanAfford(HorseUpgrade.Speed)) FindButton("Upgrade Speed").onClick.Invoke();
                            if (player.CanAfford(HorseUpgrade.BoostPower)) FindButton("Upgrade Boost Power").onClick.Invoke();
                            if (minute % 3 == 0 && player.CanAfford(HorseUpgrade.Acceleration)) FindButton("Upgrade Acceleration").onClick.Invoke();
                        }
                        Assert.That(player.EquipmentLevel(EquipmentType.Horseshoes), Is.EqualTo(2));
                        Assert.That(player.EquipmentLevel(EquipmentType.Saddle), Is.EqualTo(2));
                        Assert.That(player.EquipmentLevel(EquipmentType.Bridle), Is.EqualTo(2));
                        Assert.That(player.ProductionLevel(ProductionUpgrade.TrainingSpeed), Is.EqualTo(2));
                        Assert.That(player.FeedingSpeed, Is.GreaterThan(1f));
                        Assert.That(player.Stats.Speed, Is.GreaterThan(17f));
                        Time.timeScale = 1f;
                        yield return new WaitForSeconds(0.8f);
                        Capture("progressed-stable", 1920, 1080);
                        world.ShowBlacksmith(); yield return new WaitForSeconds(0.8f);
                        Capture("progressed-blacksmith", 1920, 1080);
                    }
                    FindButton("View Race").onClick.Invoke();
                    yield return new WaitForSeconds(0.8f);
                }
                Assert.That(transitions, Is.Zero);
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                Time.timeScale = 1f;
                Application.runInBackground = previousBackground;
                SceneManager.sceneLoaded -= onLoaded;
            }
        }

        internal static void UnlockAll(PlayerProgression player)
        {
            int coins = 0; int tokens = 0;
            for (int i = 0; i < UpgradeTreeRules.NODE_COUNT; i++)
            {
                if (player.IsUnlocked((UpgradeNode)i)) continue;
                UpgradeNodeRule rule = player.NodeRule((UpgradeNode)i);
                if (rule.Currency == UpgradeCurrency.Coins) coins += rule.Cost;
                else tokens += rule.Cost;
            }
            player.GrantDebugResources(coins, tokens);
            for (int pass = 0; pass < UpgradeTreeRules.NODE_COUNT; pass++)
                for (int i = 0; i < UpgradeTreeRules.NODE_COUNT; i++) player.TryUnlock((UpgradeNode)i);
            for (int i = 0; i < UpgradeTreeRules.NODE_COUNT; i++) Assert.That(player.IsUnlocked((UpgradeNode)i), Is.True);
        }

        internal static IEnumerator DriveRace(RaceManager race)
        {
            float timeout = Time.realtimeSinceStartup + 20f;
            float nextTap = 0f;
            while (!race.Player.HasFinished && Time.realtimeSinceStartup < timeout)
            {
                if (race.Phase == RacePhase.Racing)
                {
                    HorseSimulation horse = race.Player.Movement.Simulation;
                    if (race.ElapsedTime >= nextTap)
                    {
                        race.Player.Movement.TryBoost();
                        nextTap = race.ElapsedTime + 0.5f;
                    }
                    float distance = horse.NextObstacleDistance - horse.Distance;
                    if (distance > 0f && distance <= horse.Speed * 0.28f) race.Player.Movement.TryJump();
                }
                yield return new WaitForFixedUpdate();
            }
            Assert.That(race.Player.HasFinished, Is.True);
            Assert.That(race.Player.Movement.Simulation.ObstacleHits, Is.Zero);
        }
        private static void BuyProduction(PlayerProgression player, ProductionUpgrade upgrade, int level)
        {
            if (player.ProductionLevel(upgrade) < level && player.Coins >= player.ProductionCost(upgrade))
                FindButton("Upgrade " + upgrade).onClick.Invoke();
        }

        internal static Button FindButton(string name)
        {
            GameObject button = GameObject.Find(name);
            Assert.That(button, Is.Not.Null, name);
            return button.GetComponent<Button>();
        }

        internal static void Capture(string name, int width, int height, System.Action validateLayout = null)
        {
            const string DIRECTORY = "Tools/Milestone4Validation/Screenshots";
            System.IO.Directory.CreateDirectory(DIRECTORY);
            Camera camera = Camera.main;
            Canvas[] canvases = Object.FindObjectsByType<Canvas>();
            RenderMode[] modes = new RenderMode[canvases.Length];
            Camera[] cameras = new Camera[canvases.Length];
            float[] distances = new float[canvases.Length];
            int[] orders = new int[canvases.Length];
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture oldActive = RenderTexture.active;
            RenderTexture target = RenderTexture.GetTemporary(width, height, 24);
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                for (int i = 0; i < canvases.Length; i++)
                {
                    modes[i] = canvases[i].renderMode;
                    cameras[i] = canvases[i].worldCamera;
                    distances[i] = canvases[i].planeDistance;
                    orders[i] = canvases[i].sortingOrder;
                    canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
                    canvases[i].worldCamera = camera;
                    canvases[i].planeDistance = 1f;
                    canvases[i].sortingOrder = 100 + orders[i];
                }
                Canvas.ForceUpdateCanvases();
                validateLayout?.Invoke();
                UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,
                    new UnityEngine.Rendering.RenderPipeline.StandardRequest { destination = target });
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                System.IO.File.WriteAllBytes(DIRECTORY + "/" + name + ".png", texture.EncodeToPNG());
            }
            finally
            {
                for (int i = 0; i < canvases.Length; i++)
                {
                    canvases[i].renderMode = modes[i];
                    canvases[i].worldCamera = cameras[i];
                    canvases[i].planeDistance = distances[i];
                    canvases[i].sortingOrder = orders[i];
                }
                camera.targetTexture = previousTarget;
                Canvas.ForceUpdateCanvases();
                RenderTexture.active = oldActive;
                RenderTexture.ReleaseTemporary(target);
                Object.Destroy(texture);
            }
        }
    }
}

