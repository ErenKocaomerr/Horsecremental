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
        public IEnumerator PhysicalStableSurvivesTwoMainRacesWithoutPassiveTraining()
        {
            ProgressionSession.Reset(); Application.runInBackground = true;
            yield return SceneManager.LoadSceneAsync(GameScenes.WORLD); yield return null;
            GameplayWorld world = Object.FindAnyObjectByType<GameplayWorld>();
            PlayerProgression player = world.Player; var handle = SceneManager.GetActiveScene().handle;
            WorldPickupManager pickups = Object.FindAnyObjectByType<WorldPickupManager>();
            player.AdvanceFarm(30, world.Settings);
            Object.FindAnyObjectByType<FarmWorldView>().Click(0);
            yield return new WaitForSeconds(0.6f);
            Assert.That(pickups.PendingValue, Is.EqualTo(2)); Assert.That(player.Carrots, Is.Zero);
            int objects = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Length;
            try
            {
                for (int cycle = 0; cycle < 2; cycle++)
                {
                    world.ShowRaceArea(); world.StartMainRace();
                    double phase = player.Training.Phase(0); int coins = player.Coins; int tokens = player.RaceTokens;
                    double crop = player.Crops.Progress(0);
                    Time.timeScale = 10f; yield return DriveRace(world.Race); Time.timeScale = 1f;
                    Assert.That(world.Phase, Is.EqualTo(WorldPhase.MainRaceFinished));
                    Assert.That(player.Training.Phase(0), Is.EqualTo(phase));
                    Assert.That(player.Coins, Is.EqualTo(coins + 100)); Assert.That(player.RaceTokens, Is.EqualTo(tokens + 1));
                    if (cycle == 0) Assert.That(player.Crops.Progress(0), Is.GreaterThan(crop));
                    Assert.That(pickups.PendingValue, Is.EqualTo(2));
                    Assert.That(Object.FindAnyObjectByType<StableWorldInteraction>().CanInteract, Is.False);
                    int paid = player.Coins; yield return new WaitForSeconds(0.15f); Assert.That(player.Coins, Is.EqualTo(paid));
                    Capture("race-result-" + cycle, 1920, 1080);
                    world.ShowStable(); yield return new WaitForSeconds(0.35f);
                    Assert.That(world.IsManagement, Is.True); Assert.That(player.Training.Phase(0), Is.EqualTo(phase));
                    Assert.That(SceneManager.GetActiveScene().handle, Is.EqualTo(handle));
                    Assert.That(Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Length, Is.EqualTo(objects));
                    Assert.That(world.Race.Participants[2].Movement.Parameters.Speed, Is.EqualTo(24));

                }
                LogAssert.NoUnexpectedReceived();
            }
            finally { Time.timeScale = 1f; }
        }
        internal static void UnlockAll(PlayerProgression player)
        {
            int coins = 0; int tokens = 0;
            for (int i = 0; i < UpgradeTreeRules.NODE_COUNT; i++)
            {
                if (player.IsUnlocked((UpgradeNode)i) || (player.PlaygroundEnabled && ((UpgradeNode)i == UpgradeNode.Blacksmith || (UpgradeNode)i == UpgradeNode.ForgeSpeed || (UpgradeNode)i == UpgradeNode.MetalEfficiency || (UpgradeNode)i == UpgradeNode.FarmCapacity))) continue;
                UpgradeNodeRule rule = player.NodeRule((UpgradeNode)i);
                if (rule.Currency == UpgradeCurrency.Coins) coins += rule.Cost;
                else tokens += rule.Cost;
            }
            player.GrantDebugResources(coins, tokens);
            for (int pass = 0; pass < UpgradeTreeRules.NODE_COUNT; pass++)
                for (int i = 0; i < UpgradeTreeRules.NODE_COUNT; i++) player.TryUnlock((UpgradeNode)i);
            if (!player.ManualTrainingEnabled) Assert.That(player.IsUnlocked(UpgradeNode.TrainingReward), Is.True);
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

        internal static void Capture(string name, int width, int height, System.Action validateLayout = null,
            string directory = "Tools/Milestone45Validation/Screenshots")
        {
            System.IO.Directory.CreateDirectory(directory);
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
                System.IO.File.WriteAllBytes(directory + "/" + name + ".png", texture.EncodeToPNG());
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




