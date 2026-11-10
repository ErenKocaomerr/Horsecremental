using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;

namespace Horsecremental.Racing.Editor
{
    public static class RaceSceneBaker
    {
        private const string GENERATED_PATH = "Assets/Racing/Generated";

        [MenuItem("Horsecremental/Legacy/Rebuild Milestone 1 Race")]
        public static void Rebuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Bake();
        }

        // Also callable from a batch editor. Saves only the race scene and generated assets.
        public static void Bake()
        {
            RaceSettings settings = AssetDatabase.LoadAssetAtPath<RaceSettings>("Assets/Racing/Config/PrototypeRace.asset");
            if (settings == null) throw new InvalidOperationException("Race settings asset is missing.");
            int count = settings.OpponentStats.Length + 1;
            if (count < 3 || count > 4 || settings.HorseSheets.Length < count)
                throw new InvalidOperationException("The prototype requires three or four racers and matching horse sheets.");
            var scene = EditorSceneManager.OpenScene(RacePrototypeMenu.SCENE_PATH);
            RacePrototype prototype = null;
            foreach (GameObject rootObject in scene.GetRootGameObjects())
                if (rootObject.TryGetComponent(out RacePrototype found)) prototype = found;
            if (prototype == null) throw new InvalidOperationException("RacePrototype root is missing.");
            Transform oldGenerated = prototype.transform.Find("Generated Race");
            if (oldGenerated != null) UnityEngine.Object.DestroyImmediate(oldGenerated.gameObject);
            GameObject generated = new GameObject("Generated Race");
            generated.transform.SetParent(prototype.transform, false);
            if (!AssetDatabase.IsValidFolder(GENERATED_PATH)) AssetDatabase.CreateFolder("Assets/Racing", "Generated");

            Sprite square = CreateSquare();
            GameObject track = new GameObject("Prototype Track");
            track.transform.SetParent(generated.transform, false);
            RaceTrackBuilder.Build(track.transform, square, settings.TrackLength, settings.CreateObstacles(), count);
            RaceManager race = generated.AddComponent<RaceManager>();
            RaceParticipant[] participants = new RaceParticipant[count];
            for (int i = 0; i < count; i++) participants[i] = CreateHorse(generated.transform, settings.HorseSheets[i], i, square);

            GameObject cameraObject = new GameObject("Race Camera", typeof(Camera), typeof(AudioListener), typeof(RaceCamera));
            cameraObject.transform.SetParent(generated.transform, false);
            cameraObject.transform.localPosition = new Vector3(7f, 0.5f, -10f);
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.16f, 0.32f, 0.23f);
            camera.allowMSAA = false;
            UniversalAdditionalCameraData cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = false;
            cameraData.antialiasing = AntialiasingMode.None;
            PixelPerfectCamera pixelCamera = cameraObject.AddComponent<PixelPerfectCamera>();
            pixelCamera.assetsPPU = 24;
            pixelCamera.refResolutionX = 640;
            pixelCamera.refResolutionY = 360;
            pixelCamera.gridSnapping = PixelPerfectCamera.GridSnapping.UpscaleRenderTexture;
            pixelCamera.cropFrame = PixelPerfectCamera.CropFrame.Windowbox;
            SerializedObject pixelSettings = new SerializedObject(pixelCamera);
            pixelSettings.FindProperty("m_FilterMode").enumValueIndex = (int)PixelPerfectCamera.PixelPerfectFilterMode.Point;
            pixelSettings.ApplyModifiedPropertiesWithoutUndo();
            CinemachineSetup.ConfigureRace(camera, cameraObject.GetComponent<RaceCamera>(), participants[0].GetComponent<HorseView>().RenderRoot);

            GameObject eventSystem = new GameObject("Race Event System", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.transform.SetParent(generated.transform, false);
            eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            RaceUI ui = RaceUIBuilder.Build(generated.transform, count);
            SetReference(prototype, "_race", race);
            SetReference(prototype, "_camera", cameraObject.GetComponent<RaceCamera>());
            SetReference(prototype, "_ui", ui);
            SetArray(prototype, "_participants", participants);
            SetReference(prototype, "_progressionSettings", Milestone2SceneSetup.EnsureSettings());
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Milestone 1 race saved with visible track, horses, camera and UI.");
        }

        private static RaceParticipant CreateHorse(Transform parent, Texture2D sheet, int index, Sprite square)
        {
            bool isPlayer = index == 0;
            GameObject horse = new GameObject(isPlayer ? "Player Horse" : "AI Horse " + index, typeof(HorseMovement));
            horse.transform.SetParent(parent, false);
            horse.transform.localPosition = new Vector3(0f, RaceTrackBuilder.FIRST_LANE_Y + index * RaceTrackBuilder.LANE_SPACING, 0f);
            AIHorseController ai = null;
            if (isPlayer) horse.AddComponent<HorseController>();
            else ai = horse.AddComponent<AIHorseController>();
            RaceParticipant participant = horse.AddComponent<RaceParticipant>();
            participant.Initialize(isPlayer ? "Sen" : "Rakip " + index, isPlayer, ai);
            GameObject renderRoot = new GameObject("Horse Render Root");
            renderRoot.transform.SetParent(horse.transform, false);
            int order = 10 - index * 2;
            RaceTrackBuilder.Rectangle(renderRoot.transform, square, "Shadow", new Vector2(0f, -0.02f),
                new Vector2(1.4f, 0.13f), new Color(0f, 0f, 0f, 0.35f), order - 1);
            if (isPlayer)
                RaceTrackBuilder.Rectangle(renderRoot.transform, square, "Player marker", new Vector2(-1.1f, 0.3f),
                    new Vector2(0.18f, 0.45f), new Color(1f, 0.83f, 0.2f), order);
            Sprite[] importedFrames = RaceSpriteAssets.ImportFrames(sheet, index);
            Sprite idle = importedFrames[0];
            Sprite[] run = new Sprite[6];
            Array.Copy(importedFrames, 1, run, 0, run.Length);
            GameObject visual = new GameObject("Horse Sprite", typeof(SpriteRenderer));
            visual.transform.SetParent(renderRoot.transform, false);
            SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
            renderer.sprite = idle;
            renderer.sortingOrder = order;
            HorseView view = horse.AddComponent<HorseView>();
            SetReference(view, "_renderRoot", renderRoot.transform);
            SetReference(view, "_renderer", renderer);
            SetReference(view, "_idle", idle);
            SetArray(view, "_runFrames", run);
            return participant;
        }

        private static Sprite CreateSquare()
        {
            string path = GENERATED_PATH + "/SolidTexture.asset";
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null)
            {
                texture = new Texture2D(2, 2, TextureFormat.RGBA32, false) { name = "SolidTexture", filterMode = FilterMode.Point };
                texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
                texture.Apply();
                AssetDatabase.CreateAsset(texture, path);
            }
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f, 0, SpriteMeshType.FullRect);
            sprite.name = "SolidSprite";
            return SaveSprite(sprite, GENERATED_PATH + "/SolidSprite.asset");
        }

        private static Sprite SaveSprite(Sprite sprite, string path)
        {
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(sprite, path);
                return sprite;
            }
            EditorUtility.CopySerialized(sprite, existing);
            EditorUtility.SetDirty(existing);
            UnityEngine.Object.DestroyImmediate(sprite);
            return existing;
        }

        internal static void SetReference(UnityEngine.Object target, string name, UnityEngine.Object reference)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(name).objectReferenceValue = reference;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void SetArray<T>(UnityEngine.Object target, string name, T[] references) where T : UnityEngine.Object
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty array = serialized.FindProperty(name);
            array.arraySize = references.Length;
            for (int i = 0; i < references.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = references[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
