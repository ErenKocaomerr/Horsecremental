using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Horsecremental.Racing.Editor
{
    public static class MainRaceArtSetup
    {
        private const string ART_NAME = "Main Race Environment Art";
        private static readonly Color LANE_COLOR = new Color(0.91f, 0.8f, 0.59f);
        private static readonly Color HURDLE_COLOR = new Color(0.74f, 0.74f, 0.68f);
        private static readonly Vector2[] VERGE_POSITIONS =
        {
            new Vector2(1.8f, -4.65f), new Vector2(2.35f, -4.78f), new Vector2(6.9f, -5.08f),
            new Vector2(9.2f, -4.55f), new Vector2(13.3f, -4.83f), new Vector2(18.3f, -5.05f)
        };

        [MenuItem("Horsecremental/Main Race Art/Apply to Current World")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != Milestone3SceneSetup.WORLD_PATH) throw new InvalidOperationException("Open Milestone3World first.");
            GameplayWorld world = UnityEngine.Object.FindAnyObjectByType<GameplayWorld>();
            if (world == null) throw new InvalidOperationException("The existing gameplay world is required.");
            Transform track = world.transform.Find("Generated Race/Prototype Track");
            if (track == null) throw new InvalidOperationException("Main Race track is missing.");
            RaceSettings settings = AssetDatabase.LoadAssetAtPath<RaceSettings>("Assets/Racing/Config/PrototypeRace.asset");
            if (settings == null) throw new InvalidOperationException("Main Race settings are missing.");
            MainRaceArtAssets.Prepare();
            ApplyToTrack(track, settings);
            ApplyToLadder(world);
            foreach (GameObject item in scene.GetRootGameObjects())
                if (item.name.StartsWith("Attached Horse Style Reference", StringComparison.Ordinal)) item.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Race art applied to all courses: terrain, fences, flags, timber hurdles and verge details.");
        }

        [MenuItem("Horsecremental/Main Race Art/Restore All Race Visuals")]
        public static void RestoreAllRaces()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            GameplayWorld world = UnityEngine.Object.FindAnyObjectByType<GameplayWorld>();
            if (world == null) throw new InvalidOperationException("Open the gameplay world first.");
            MainRaceArtAssets.Prepare();
            Transform main = world.transform.Find("Generated Race/Prototype Track");
            if (main.Find(ART_NAME) == null)
                ApplyToTrack(main, AssetDatabase.LoadAssetAtPath<RaceSettings>("Assets/Racing/Config/PrototypeRace.asset"));
            ApplyToLadder(world);
            EditorSceneManager.MarkSceneDirty(world.gameObject.scene);
            EditorSceneManager.SaveScene(world.gameObject.scene);
            AssetDatabase.SaveAssets();
        }

        private static void ApplyToLadder(GameplayWorld world)
        {
            if (world.TrainingLadder == null) return;
            SerializedProperty tracks = new SerializedObject(world.GetComponent<RacePrototype>()).FindProperty("_trainingTracks");
            for (int i = 0; i < tracks.arraySize; i++)
            {
                GameObject course = tracks.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                if (course == null) throw new InvalidOperationException("A race course reference is missing.");
                if (course.transform.Find(ART_NAME) == null) ApplyToTrack(course.transform, world.TrainingLadder.Race(i));
            }
        }

        internal static void ApplyToTrack(Transform track, RaceSettings settings)
        {
            Transform previous = track.Find(ART_NAME);
            if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
            // Retain legacy objects and references; only their old primitive art is hidden.
            foreach (SpriteRenderer renderer in track.GetComponentsInChildren<SpriteRenderer>(true)) renderer.enabled = false;
            Transform art = new GameObject(ART_NAME).transform;
            art.SetParent(track, false);
            BuildTerrain(art, settings.TrackLength);
            BuildCourse(art, settings);
            BuildVerge(art, settings.TrackLength);
        }

        private static void BuildTerrain(Transform art, float length)
        {
            Sprite dirt = Get("RaceTerrain", "Dirt");
            Sprite grass = Get("RaceTerrain", "Grass");
            Sprite backdrop = Get("RaceBackdrop", "Backdrop");
            int index = 0;
            for (float x = -20f; x < length + 40f; x += 20f, index++)
            {
                bool flip = index % 2 != 0;
                // Alternating mirrored strips meet at the same source edge colors.
                Size(art, "Grass Strip " + index, grass, new Vector2(x + 10f, 0f), new Vector2(20.03f, 18f), -29).flipX = flip;
                Size(art, "Track Strip " + index, dirt, new Vector2(x + 10f, 0f), new Vector2(20.03f, 7.8f), -20).flipX = flip;
            }
            index = 0;
            for (float x = -20f; x < length + 40f; x += 30f, index++)
                Size(art, "Tree Line " + index, backdrop, new Vector2(x + 15f, 3.7f), new Vector2(30.03f, 10f), -25).flipX = index % 2 != 0;
        }

        private static void BuildCourse(Transform art, RaceSettings settings)
        {
            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Racing/Generated/SolidSprite.asset");
            RaceObstacle[] obstacles = settings.CreateObstacles();
            float length = settings.TrackLength;
            for (int lane = 0; lane < settings.OpponentStats.Length + 1; lane++)
            {
                float y = RaceTrackBuilder.FIRST_LANE_Y + lane * RaceTrackBuilder.LANE_SPACING;
                Size(art, "Lane Marking " + lane, square, new Vector2(length * 0.5f, y - 0.25f),
                    new Vector2(length + 40f, 0.045f), -10).color = LANE_COLOR;
                for (int i = 0; i < obstacles.Length; i++)
                {
                    SpriteRenderer hurdle = Width(art, "Timber Hurdle " + i + " Lane " + lane,
                        Get("RaceHurdle", "TimberHurdle"), new Vector2(obstacles[i].Distance, y - 0.15f), 1f, 10 - lane * 2);
                    float scale = 1.2f / hurdle.sprite.bounds.size.y;
                    hurdle.transform.localScale = new Vector3(scale * 0.75f, scale, 1f);
                    hurdle.color = HURDLE_COLOR;
                    SpriteRenderer marker = Width(art, "Jump Marker " + i + " Lane " + lane,
                        Get("RaceProps", "Approach"), new Vector2(obstacles[i].Distance - 3f, y - 0.15f), 1f, -5);
                    marker.transform.localScale = Vector3.one * 0.3f;
                }
            }
            for (float x = -20f; x < length + 40f; x += 1.9f)
            {
                SpriteRenderer fence = Width(art, "Trackside Fence", Get("RaceProps", "Fence"), new Vector2(x, 3.7f), 1f, 2);
                fence.transform.localScale = Vector3.one * 1.1f;
            }
            Size(art, "Start Line", square, Vector2.zero, new Vector2(0.12f, 7.8f), -5).color = LANE_COLOR;
            for (int i = 0; i < 20; i++)
                Size(art, "Finish Checker " + i, square, new Vector2(length, -3.7f + i * 0.39f),
                    new Vector2(0.3f, 0.39f), -5).color = i % 2 == 0 ? LANE_COLOR : new Color(0.2f, 0.16f, 0.1f);
            Native(art, "Start Pennant", Get("RaceProps", "StartFlag"), new Vector2(0f, 3.7f), 12);
            Native(art, "Finish Flag", Get("RaceProps", "FinishFlag"), new Vector2(length, 3.7f), 12);
        }

        private static void BuildVerge(Transform art, float length)
        {
            for (float x = -20f; x < length + 40f; x += 20f)
            {
                for (int i = 0; i < VERGE_POSITIONS.Length; i++)
                    Native(art, "Verge Grass", Get("VergeDetails", i % 2 == 0 ? "GrassA" : "GrassB"),
                        new Vector2(x + VERGE_POSITIONS[i].x, VERGE_POSITIONS[i].y), 12).color = new Color(0.86f, 0.91f, 0.77f);
                SpriteRenderer stones = Native(art, "Low Pebbles", Get("VergeDetails", "Pebbles"), new Vector2(x + 4.4f, -4.85f), 12);
                stones.transform.localScale = Vector3.one * 0.65f; stones.color = new Color(0.86f, 0.86f, 0.8f);
                stones = Native(art, "Small Pebbles", Get("VergeDetails", "Pebbles"), new Vector2(x + 16.4f, -4.55f), 12);
                stones.transform.localScale = Vector3.one * 0.5f; stones.flipX = true; stones.color = new Color(0.86f, 0.86f, 0.8f);
                SpriteRenderer flower = Native(art, "Wildflower", Get("VergeDetails", "Flower"), new Vector2(x + 9.85f, -4.65f), 12);
                flower.transform.localScale = Vector3.one * 0.65f;
            }
        }

        private static Sprite Get(string sheet, string name) => MainRaceArtAssets.Get(sheet, name);

        private static SpriteRenderer Native(Transform parent, string name, Sprite sprite, Vector2 position, int order)
        {
            GameObject item = new GameObject(name, typeof(SpriteRenderer));
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            SpriteRenderer renderer = item.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite; renderer.sortingOrder = order;
            return renderer;
        }

        private static SpriteRenderer Width(Transform parent, string name, Sprite sprite, Vector2 position, float width, int order)
        {
            SpriteRenderer renderer = Native(parent, name, sprite, position, order);
            renderer.transform.localScale = Vector3.one * (width / sprite.bounds.size.x);
            return renderer;
        }

        private static SpriteRenderer Size(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size, int order)
        {
            SpriteRenderer renderer = Native(parent, name, sprite, position, order);
            renderer.transform.localScale = new Vector3(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y, 1f);
            return renderer;
        }
    }
}
