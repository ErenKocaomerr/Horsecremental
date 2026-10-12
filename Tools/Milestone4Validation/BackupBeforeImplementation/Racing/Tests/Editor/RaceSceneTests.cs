using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Horsecremental.Racing.Tests
{
    public sealed class RaceSceneTests
    {
        [Test]
        public void SavedSceneHasVisiblePersistentObjectsWithoutPlayMode()
        {
            const string SCENE_PATH = "Assets/Racing/Scenes/Milestone1Race.unity";
            for (int reload = 0; reload < 2; reload++)
            {
                EditorSceneManager.OpenScene(SCENE_PATH);
                Assert.That(Object.FindObjectsByType<RaceParticipant>().Length, Is.EqualTo(4));
                Assert.That(Object.FindObjectsByType<Camera>().Length, Is.EqualTo(1));
                Assert.That(Object.FindObjectsByType<RacePrototype>().Length, Is.EqualTo(1));
                Assert.That(GameObject.Find("Prototype Track"), Is.Not.Null);
                Assert.That(GameObject.Find("Race entry"), Is.Not.Null);
                Assert.That(GameObject.Find("Start Race").GetComponent<Button>().IsActive(), Is.True);
                foreach (SpriteRenderer renderer in Object.FindObjectsByType<SpriteRenderer>())
                {
                    Assert.That(renderer.sprite, Is.Not.Null, renderer.name);
                    Assert.That(EditorUtility.IsPersistent(renderer.sprite), Is.True, renderer.name);
                    Assert.That(renderer.sprite.texture, Is.Not.Null);
                    if (renderer.name == "Horse Sprite")
                    {
                        Vector2[] coordinates = renderer.sprite.uv;
                        Assert.That(coordinates.Length, Is.GreaterThanOrEqualTo(4));
                        Assert.That(Vector2.Distance(coordinates[0], coordinates[1]), Is.GreaterThan(0f),
                            "Horse sprites must retain distinct texture coordinates after saving and reopening.");
                    }
                }
                PixelPerfectCamera camera = Object.FindAnyObjectByType<PixelPerfectCamera>();
                Assert.That(camera.assetsPPU, Is.EqualTo(24));
                Assert.That(camera.refResolutionX, Is.EqualTo(640));
                Assert.That(camera.refResolutionY, Is.EqualTo(360));
                Assert.That(camera.cropFrame, Is.EqualTo(PixelPerfectCamera.CropFrame.Windowbox));
                Assert.That(camera.gridSnapping, Is.EqualTo(PixelPerfectCamera.GridSnapping.UpscaleRenderTexture));
                Assert.That(new SerializedObject(camera).FindProperty("m_FilterMode").enumValueIndex,
                    Is.EqualTo((int)PixelPerfectCamera.PixelPerfectFilterMode.Point));
            }
        }
    }
}
