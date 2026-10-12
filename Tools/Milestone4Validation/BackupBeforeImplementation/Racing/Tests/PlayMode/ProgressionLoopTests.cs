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
        public IEnumerator SameSceneLoopTrainsFarmsLosesUpgradesAndWinsRepeatedly()
        {
            ProgressionSession.Reset();
            yield return SceneManager.LoadSceneAsync(GameScenes.WORLD);
            yield return null;
            GameplayWorld world = Object.FindAnyObjectByType<GameplayWorld>();
            Assert.That(world, Is.Not.Null);
            PlayerProgression player = world.Player;
            RaceManager race = world.Race;
            Assert.That(world.IsManagement, Is.True);
            Assert.That(GameObject.Find("Coins").GetComponent<Text>().text, Does.StartWith("COINS  "));
            var sceneHandle = SceneManager.GetActiveScene().handle;
            int transitions = 0;
            UnityEngine.Events.UnityAction<Scene, LoadSceneMode> onLoaded = (scene, mode) => transitions++;
            SceneManager.sceneLoaded += onLoaded;
            int originalObjects = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Length;
            int coins = player.Coins;
            double trainingElapsed = player.TrainingElapsed;
            bool previousBackground = Application.runInBackground;
            Application.runInBackground = true;
            try
            {
                Capture("stable-1920x1080", 1920, 1080);
                Capture("stable-1280x720", 1280, 720);
                Capture("stable-1024x768", 1024, 768);
                Capture("stable-2560x1080", 2560, 1080);
                // Train first for a small amount, then demonstrate that this progress pauses.
                Time.timeScale = 20f;
                yield return new WaitForSeconds(17f);
                Assert.That(player.Coins, Is.GreaterThanOrEqualTo(coins + 35));
                Assert.That(player.TrainingRaceNumber, Is.GreaterThanOrEqualTo(2));
                FindButton("View Race").onClick.Invoke();
                Time.timeScale = 1f;
                yield return new WaitForSeconds(1.1f);
                Assert.That(Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera.Name, Is.EqualTo("Race Entry Cinemachine"));
                Capture("race-entry", 1920, 1080);
                for (int cycle = 0; cycle < 3; cycle++)
                {
                    int beforeCoins = player.Coins;
                    trainingElapsed = player.TrainingElapsed;
                    long beforeRaceNumber = player.TrainingRaceNumber;
                    FindButton("Start Race").onClick.Invoke();
                    world.StartMainRace(); // Rapid duplicate request must be ignored.
                    Assert.That(world.Phase, Is.EqualTo(WorldPhase.MainRacePreparing));
                    Assert.That(race.Player.Movement.Parameters.Speed, Is.EqualTo(player.Stats.Speed));
                    Assert.That(race.Player.Movement.Parameters.BoostPower, Is.EqualTo(player.Stats.BoostPower));
                    Time.timeScale = 10f;
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
                    Assert.That(race.Player.FinishPosition, Is.EqualTo(cycle == 0 ? 4 : 1));
                    Assert.That(world.Phase, Is.EqualTo(WorldPhase.MainRaceFinished));
                    Assert.That(player.TrainingElapsed, Is.EqualTo(trainingElapsed));
                    Assert.That(player.TrainingRaceNumber, Is.EqualTo(beforeRaceNumber));
                    Assert.That(player.Coins, Is.EqualTo(beforeCoins + (cycle == 0 ? 100 : 500)));
                    Assert.That(race.Player.Movement.Simulation.ObstacleHits, Is.Zero);
                    Time.timeScale = 1f;
                    yield return null;
                    Capture(cycle == 0 ? "fresh-player-result" : "upgraded-win", 1920, 1080);
                    int paidCoins = player.Coins;
                    yield return new WaitForSeconds(0.2f);
                    Assert.That(player.Coins, Is.EqualTo(paidCoins));
                    FindButton("Return to Stable").onClick.Invoke();
                    world.StartMainRace(); // Cannot start during the return blend.
                    Assert.That(world.IsManagement, Is.False);
                    yield return new WaitForSeconds(1.1f);
                    Assert.That(world.IsManagement, Is.True);
                    Assert.That(ProgressionSession.Player, Is.SameAs(player));
                    Assert.That(SceneManager.GetActiveScene().handle, Is.EqualTo(sceneHandle));
                    Assert.That(Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Length, Is.EqualTo(originalObjects));
                    Assert.That(race.Participants[2].Movement.Parameters.Speed, Is.EqualTo(14.5f));
                    if (cycle == 0)
                    {
                        // Drive the actual Unity management timers through ten minutes of play.
                        Time.timeScale = 40f;
                        for (int minute = 0; minute < 10; minute++)
                        {
                            yield return new WaitForSeconds(60f);
                            FindButton("Collect Carrots").onClick.Invoke();
                            while (player.Carrots >= world.Settings.FeedCost) FindButton("Feed Horse").onClick.Invoke();
                            if (player.CanAfford(HorseUpgrade.Speed)) FindButton("Upgrade Speed").onClick.Invoke();
                            if (player.CanAfford(HorseUpgrade.BoostPower)) FindButton("Upgrade Boost Power").onClick.Invoke();
                            if (player.CanAfford(HorseUpgrade.Acceleration)) FindButton("Upgrade Acceleration").onClick.Invoke();
                        }
                        Assert.That(player.FeedingSpeed, Is.GreaterThan(0.4f));
                        Assert.That(player.Stats.Speed, Is.GreaterThan(13f));
                        Assert.That(player.Stats.BoostPower, Is.GreaterThan(5f));
                        Assert.That(player.TrainingRaceNumber, Is.GreaterThan(beforeRaceNumber));
                        Time.timeScale = 1f;
                        yield return new WaitForSeconds(0.4f);
                        Capture("upgraded-stable", 1920, 1080);
                    }
                    FindButton("View Race").onClick.Invoke();
                    yield return new WaitForSeconds(1f);
                }
                Assert.That(transitions, Is.Zero, "Area navigation and repeated races cannot load scenes.");
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                Time.timeScale = 1f;
                Application.runInBackground = previousBackground;
                SceneManager.sceneLoaded -= onLoaded;
            }
        }

        internal static Button FindButton(string name)
        {
            GameObject button = GameObject.Find(name);
            Assert.That(button, Is.Not.Null, name);
            return button.GetComponent<Button>();
        }

        internal static void Capture(string name, int width, int height)
        {
            const string DIRECTORY = "Tools/Milestone3Validation/Screenshots";
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
