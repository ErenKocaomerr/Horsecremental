using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Horsecremental.Racing.Tests
{
    public sealed class StableMouseInputTests
    {
        [UnityTest]
        public IEnumerator MouseClicksGrowHarvestAndHoverCollectsWithoutAnotherClick()
        {
            bool background = Application.runInBackground;
            var inputBackground = InputSystem.settings.backgroundBehavior;
            var editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            Application.runInBackground = true;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            try
            {
                ProgressionSession.Reset(); yield return SceneManager.LoadSceneAsync(GameScenes.WORLD); yield return new WaitForSeconds(0.25f);
                GameplayWorld world = Object.FindAnyObjectByType<GameplayWorld>();
                FarmWorldView farm = Object.FindAnyObjectByType<FarmWorldView>();
                WorldPickupManager pickups = Object.FindAnyObjectByType<WorldPickupManager>();
                Vector2 screen = Camera.main.WorldToScreenPoint(farm.PlotPosition(0) + Vector3.up * 0.2f);
                StableWorldInteraction interaction = Object.FindAnyObjectByType<StableWorldInteraction>();
                Assert.That(interaction.CanInteract, Is.True);
                Assert.That(interaction.IsUIBlocked(screen), Is.False, "A physical crop must not be covered by an invisible UI graphic.");
                Vector3 converted = Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -Camera.main.transform.position.z));
                Assert.That(farm.HitPlot(converted), Is.EqualTo(0));
                double before = world.Player.Crops.Progress(0);
                InputSystem.QueueStateEvent(mouse, new MouseState { position = screen }.WithButton(MouseButton.Left)); yield return null; yield return null;
                Assert.That(Mouse.current, Is.SameAs(mouse));
                Assert.That(world.Player.Crops.Progress(0), Is.GreaterThanOrEqualTo(before + 0.099));
                InputSystem.QueueStateEvent(mouse, new MouseState { position = screen }); yield return new WaitForSeconds(0.27f);
                world.Player.Crops.Grow(0, 1);
                InputSystem.QueueStateEvent(mouse, new MouseState { position = screen }.WithButton(MouseButton.Left)); yield return null; yield return null;
                Assert.That(world.Player.Crops.Stage(0), Is.EqualTo(CropStage.Sprout)); Assert.That(world.Player.Carrots, Is.Zero);
                Assert.That(pickups.PendingValue, Is.EqualTo(2));
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(0, 0) }); yield return new WaitForSeconds(0.6f);
                for (int i = 0; i < 2; i++)
                {
                    screen = Camera.main.WorldToScreenPoint(pickups.ItemPosition(i));
                    InputSystem.QueueStateEvent(mouse, new MouseState { position = screen }); yield return new WaitForSeconds(0.27f);
                }
                Assert.That(world.Player.Carrots, Is.EqualTo(2)); Assert.That(pickups.PendingValue, Is.Zero);
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                InputSystem.RemoveDevice(mouse); Application.runInBackground = background;
                InputSystem.settings.backgroundBehavior = inputBackground;
                InputSystem.settings.editorInputBehaviorInPlayMode = editorInput;
            }
        }
    }
}
