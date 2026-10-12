using System.Collections;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Horsecremental.Racing.Tests
{
    public sealed class RacePlayModeTests
    {
        private Keyboard _keyboard;
        private InputSettings.BackgroundBehavior _background;
        private InputSettings.EditorInputBehaviorInPlayMode _editorInput;
        private bool _runInBackground;
        private int _targetRate;
        private int _captureRate;
        private int _vSync;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _background = InputSystem.settings.backgroundBehavior;
            _editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            _runInBackground = Application.runInBackground;
            _targetRate = Application.targetFrameRate;
            _captureRate = Time.captureFramerate;
            _vSync = QualitySettings.vSyncCount;
            Application.runInBackground = true;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            ProgressionSession.Reset();
            yield return SceneManager.LoadSceneAsync(GameScenes.WORLD);
            yield return null;
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1f;
            Application.runInBackground = _runInBackground;
            Application.targetFrameRate = _targetRate;
            Time.captureFramerate = _captureRate;
            QualitySettings.vSyncCount = _vSync;
            if (_keyboard != null) InputSystem.RemoveDevice(_keyboard);
            _keyboard = null;
            InputSystem.settings.backgroundBehavior = _background;
            InputSystem.settings.editorInputBehaviorInPlayMode = _editorInput;
            yield return null;
        }
        [UnityTest]
        public IEnumerator KeyboardNavigationBlendsAndSpaceAddsEnergyOnlyPerPress()
        {
            GameplayWorld world = Object.FindAnyObjectByType<GameplayWorld>();
            RaceManager race = world.Race;
            _keyboard = InputSystem.AddDevice<Keyboard>();
            float stableX = Camera.main.transform.position.x;
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.D, Key.Space, Key.E));
            yield return null;
            Assert.That(world.ViewingRace, Is.True);
            Assert.That(race.Player.Movement.Simulation.BoostEnergy, Is.Zero);
            Assert.That(race.Player.Movement.Simulation.Height, Is.Zero);
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState());
            yield return new WaitForSeconds(0.3f);
            CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
            Assert.That(brain.IsBlending, Is.True);
            Assert.That(Camera.main.transform.position.x, Is.GreaterThan(stableX));
            Assert.That(Camera.main.transform.position.x, Is.LessThan(7f));
            yield return new WaitForSeconds(0.8f);
            Assert.That(brain.ActiveVirtualCamera.Name, Is.EqualTo("Race Entry Cinemachine"));
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.A));
            yield return null;
            Assert.That(world.ViewingRace, Is.False);
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState());
            yield return new WaitForSeconds(1f);
            world.ShowRaceArea();
            yield return new WaitForSeconds(1f);
            world.StartMainRace();
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.Space));
            yield return null;
            Assert.That(race.Player.Movement.Simulation.BoostEnergy, Is.Zero);
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState());
            yield return new WaitForSeconds(4.1f);
            Assert.That(world.Phase, Is.EqualTo(WorldPhase.MainRaceRunning));
            int presses = 0;
            System.Action onBoost = () => presses++;
            race.Player.Movement.BoostActivated += onBoost;
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.Space, Key.E));
            yield return null;
            yield return new WaitForFixedUpdate();
            Assert.That(race.Player.Movement.Simulation.BoostEnergy, Is.InRange(0.04f, 0.061f));
            Assert.That(race.Player.Movement.Simulation.Height, Is.GreaterThan(0f));
            float energy = race.Player.Movement.Simulation.BoostEnergy;
            yield return new WaitForSeconds(0.8f);
            Assert.That(presses, Is.EqualTo(1), "Holding Space cannot repeat taps.");
            Assert.That(race.Player.Movement.Simulation.BoostEnergy, Is.LessThan(energy));
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState());
            yield return null;
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.Space));
            yield return null;
            Assert.That(presses, Is.EqualTo(2));
            race.Player.Movement.BoostActivated -= onBoost;
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState());
            int hits = 0;
            System.Action onHit = () => hits++;
            race.Player.Movement.ObstacleHit += onHit;
            Time.timeScale = 10f;
            float timeout = Time.realtimeSinceStartup + 10f;
            while (hits == 0 && Time.realtimeSinceStartup < timeout) yield return new WaitForFixedUpdate();
            Assert.That(hits, Is.EqualTo(1));
            Assert.That(race.Player.Movement.Simulation.IsPenalized, Is.True);
            race.Player.Movement.ObstacleHit -= onHit;
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest]
        public IEnumerator InterpolatedVisualsStayConsistentAt30And60And120Fps()
        {
            GameplayWorld world = Object.FindAnyObjectByType<GameplayWorld>();
            world.ShowRaceArea();
            world.StartMainRace();
            Time.timeScale = 10f;
            while (world.Phase != WorldPhase.MainRaceRunning) yield return null;
            Time.timeScale = 1f;
            QualitySettings.vSyncCount = 0;
            HorseView view = world.Race.Player.GetComponent<HorseView>();
            RaceVisualProbe probe = world.Race.gameObject.AddComponent<RaceVisualProbe>();
            probe.Initialize(view, world.Race.Player.Movement.Simulation);
            foreach (int fps in new[] { 30, 60, 120 })
            {
                Application.targetFrameRate = fps;
                Time.captureFramerate = fps;
                world.Race.Player.Movement.TryBoost();
                world.Race.Player.Movement.TryJump();
                int samples = probe.Samples;
                for (int frame = 0; frame < 30; frame++) yield return null;
                Assert.That(probe.Samples, Is.GreaterThan(samples));
                Assert.That(probe.MaximumError, Is.LessThan(0.0001f), "Visual interpolation at " + fps + " FPS");
                Assert.That(probe.BackwardsSteps, Is.Zero);
            }
            ProgressionLoopTests.Capture("race-running", 1920, 1080);
            Object.Destroy(probe);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
