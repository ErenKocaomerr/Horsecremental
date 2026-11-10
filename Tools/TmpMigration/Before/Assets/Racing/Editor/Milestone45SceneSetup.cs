using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

namespace Horsecremental.Racing.Editor
{
    public static class Milestone45SceneSetup
    {
        public const string SETTINGS_PATH = "Assets/Racing/Config/StablePlayground.asset";
        [MenuItem("Horsecremental/Apply Milestone 4.5 Playground")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != Milestone3SceneSetup.WORLD_PATH)
                EditorSceneManager.OpenScene(Milestone3SceneSetup.WORLD_PATH);
            GameplayWorld world = UnityEngine.Object.FindAnyObjectByType<GameplayWorld>();
            if (world == null) throw new InvalidOperationException("Open the existing gameplay world first.");
            StablePlaygroundSettings settings = AssetDatabase.LoadAssetAtPath<StablePlaygroundSettings>(SETTINGS_PATH);
            if (settings == null)
            { settings = ScriptableObject.CreateInstance<StablePlaygroundSettings>(); AssetDatabase.CreateAsset(settings, SETTINGS_PATH); }
            ConfigureAssets(world);
            Transform root = world.transform;
            Transform previous = root.Find("Stable Playground");
            if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
            previous = root.Find("Playground UI");
            if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
            Transform playground = Child(root, "Stable Playground");
            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Racing/Generated/SolidSprite.asset");
            Sprite carrot = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Racing/Generated/UI/Icon17.png");
            StableAudioHooks audio = playground.gameObject.AddComponent<StableAudioHooks>();
            playground.GetComponent<AudioSource>().playOnAwake = false;
            WorldFeedback feedback = BuildFeedback(playground);
            WorldPickupManager pickups = BuildPickups(playground, carrot, settings.MaxPickups, feedback, audio);
            FarmWorldView farm = BuildFarm(playground, square, carrot, settings.PlotCount, pickups, feedback, audio);
            TrainingTrackView training = BuildTrack(playground, square, world, settings.MaxHorseCount, feedback, audio);
            StableWorldInteraction interaction = playground.gameObject.AddComponent<StableWorldInteraction>();
            Bind(interaction, "_world", world); Bind(interaction, "_camera", UnityEngine.Object.FindAnyObjectByType<Camera>());
            Bind(interaction, "_farm", farm); Bind(interaction, "_pickups", pickups);
            Bind(world, "_playground", settings); Bind(world, "_trainingTrack", training);
            Bind(world, "_farmWorld", farm); Bind(world, "_pickups", pickups); Bind(world, "_audio", audio);
            Bind(world, "_mainTrack", root.Find("Generated Race/Prototype Track").gameObject);
            root.Find("Generated Race/Prototype Track").gameObject.SetActive(false);
            SetBool(world, "_blacksmithDisabled", true);
            ArrangeWorld(root, world);
            Milestone45UIBuilder.Build(world, audio);
            if (AssetDatabase.LoadAssetAtPath<TrainingLadderSettings>(TrainingRaceSceneSetup.LADDER_PATH) != null)
                TrainingRaceSceneSetup.Apply(world);
            EditorSceneManager.MarkSceneDirty(world.gameObject.scene);
            EditorSceneManager.SaveScene(world.gameObject.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Milestone 4.5 applied: physical training, six crops, pooled hover pickups and ambient smith.");
        }
        private static void ConfigureAssets(GameplayWorld world)
        {
            SerializedObject data = new SerializedObject(world.Settings);
            data.FindProperty("_trainingMetal").intValue = 0; data.ApplyModifiedPropertiesWithoutUndo();
            UpgradeTreeSettings tree = AssetDatabase.LoadAssetAtPath<UpgradeTreeSettings>("Assets/Racing/Config/UpgradeTree.asset");
            data = new SerializedObject(tree);
            SerializedProperty farm = data.FindProperty("_nodes").GetArrayElementAtIndex((int)UpgradeNode.Farm);
            farm.FindPropertyRelative("_startsUnlocked").boolValue = true;
            farm.FindPropertyRelative("_cost").intValue = 0; data.ApplyModifiedPropertiesWithoutUndo();
            WorkshopSettings workshop = AssetDatabase.LoadAssetAtPath<WorkshopSettings>("Assets/Racing/Config/Workshop.asset");
            data = new SerializedObject(workshop);
            data.FindProperty("_horseCount").FindPropertyRelative("_growth").floatValue = 1.8f;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static WorldFeedback BuildFeedback(Transform root)
        {
            Transform pool = Child(root, "Floating Feedback");
            WorldFeedback feedback = pool.gameObject.AddComponent<WorldFeedback>();
            TextMesh[] labels = new TextMesh[16];
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = WorldLabel(pool, "Feedback " + i, Vector3.zero, string.Empty, 0.065f);
                labels[i].gameObject.SetActive(false);
            }
            RaceSceneBaker.SetArray(feedback, "_labels", labels);
            return feedback;
        }
        private static WorldPickupManager BuildPickups(Transform root, Sprite carrot, int count, WorldFeedback feedback, StableAudioHooks audio)
        {
            Transform pool = Child(root, "Carrot Pickup Pool");
            WorldPickupManager pickups = pool.gameObject.AddComponent<WorldPickupManager>();
            Transform[] items = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                SpriteRenderer item = Rect(pool, carrot, "Carrot Pickup " + i, Vector2.zero, Vector2.one * 0.35f, Color.white, 40);
                items[i] = item.transform; item.gameObject.SetActive(false);
            }
            RaceSceneBaker.SetArray(pickups, "_items", items);
            Bind(pickups, "_feedback", feedback); Bind(pickups, "_audio", audio);
            return pickups;
        }
        private static FarmWorldView BuildFarm(Transform root, Sprite square, Sprite carrot, int count,
            WorldPickupManager pickups, WorldFeedback feedback, StableAudioHooks audio)
        {
            Transform area = Child(root, "Physical Farm");
            FarmWorldView farm = area.gameObject.AddComponent<FarmWorldView>();
            Transform[] plants = new Transform[count]; SpriteRenderer[] halos = new SpriteRenderer[count];
            WorldLabel(area, "Farm Sign", new Vector3(-30.1f, 2.8f), "FARM", 0.075f);
            WorldLabel(area, "Farm Hint", new Vector3(-30.1f, -2.8f), "Click crops • Sweep drops", 0.045f);
            for (int i = 0; i < count; i++)
            {
                Transform plot = Child(area, "Crop Plot " + i);
                plot.position = new Vector3(-31.7f + i % 3 * 1.6f, 0.8f - i / 3 * 1.8f);
                halos[i] = Rect(plot, square, "Hover and Ready Halo", new Vector2(0, -0.25f), new Vector2(1.4f, 0.64f), new Color(0.48f, 0.65f, 0.26f, 0.15f), 9);
                Rect(plot, square, "Soil", new Vector2(0, -0.25f), new Vector2(1.2f, 0.43f), new Color(0.35f, 0.23f, 0.13f), 10);
                SpriteRenderer plant = Rect(plot, carrot, "Growing Carrot", new Vector2(0, 0.1f), Vector2.one * 0.32f, Color.white, 25);
                plants[i] = plant.transform;
            }
            RaceSceneBaker.SetArray(farm, "_plants", plants); RaceSceneBaker.SetArray(farm, "_halos", halos);
            Bind(farm, "_pickups", pickups); Bind(farm, "_feedback", feedback); Bind(farm, "_audio", audio);
            return farm;
        }
        private static TrainingTrackView BuildTrack(Transform root, Sprite square, GameplayWorld world, int maximum,
            WorldFeedback feedback, StableAudioHooks audio)
        {
            Transform track = Child(root, "Physical Training Track");
            Vector3[] path = new Vector3[40];
            for (int i = 0; i < path.Length; i++)
            {
                float angle = i * Mathf.PI * 2f / path.Length;
                path[i] = new Vector3(-18.2f + Mathf.Cos(angle) * 4f, 0.2f + Mathf.Sin(angle) * 1.6f);
            }
            Rect(track, square, "Track Infield", new Vector2(-18.2f, 0.2f), new Vector2(7.7f, 3.1f), new Color(0.2f, 0.42f, 0.24f), 1);
            for (int i = 0; i < path.Length; i++)
            {
                Vector3 delta = path[(i + 1) % path.Length] - path[i];
                SpriteRenderer strip = Rect(track, square, "Loop Segment " + i, (path[i] + path[(i + 1) % path.Length]) * 0.5f,
                    new Vector2(delta.magnitude + 0.04f, 0.52f), new Color(0.59f, 0.43f, 0.24f), 5);
                strip.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            }
            Rect(track, square, "Lap Finish", new Vector2(path[0].x, path[0].y), new Vector2(0.55f, 0.12f), Color.white, 6);
            WorldLabel(track, "Training Sign", new Vector3(-18.2f, 3.3f), "TRAINING", 0.075f);
            WorldLabel(track, "Training Hint", new Vector3(-18.2f, -2.8f), "Each lap earns Coins", 0.045f);
            Sprite[] idle = new Sprite[4]; Sprite[] run = new Sprite[24];
            for (int i = 0; i < 4; i++)
            {
                foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath("Assets/Racing/Generated/Horse" + i + "Frames.png"))
                {
                    Sprite sprite = asset as Sprite;
                    if (sprite == null) continue;
                    if (sprite.name.EndsWith("Idle", StringComparison.Ordinal)) idle[i] = sprite;
                    for (int frame = 0; frame < 6; frame++) if (sprite.name.EndsWith("Run" + frame, StringComparison.Ordinal)) run[i * 6 + frame] = sprite;
                }
            }
            Transform[] horses = new Transform[maximum]; SpriteRenderer[] renderers = new SpriteRenderer[maximum];
            for (int i = 0; i < maximum; i++)
            {
                horses[i] = Child(track, "Training Horse " + i); horses[i].position = path[i * path.Length / maximum];
                Rect(horses[i], square, "Shadow", new Vector2(0, -0.22f), new Vector2(1.4f, 0.12f), new Color(0, 0, 0, 0.22f), 12);
                renderers[i] = Rect(horses[i], idle[i % 4], "Animated Horse", Vector2.zero, Vector2.one, Color.white, 20);
                horses[i].localScale = Vector3.one * 0.75f; horses[i].gameObject.SetActive(i == 0);
            }
            TrainingTrackView view = track.gameObject.AddComponent<TrainingTrackView>();
            Bind(view, "_world", world); Bind(view, "_feedback", feedback); Bind(view, "_audio", audio);
            RaceSceneBaker.SetArray(view, "_horses", horses); RaceSceneBaker.SetArray(view, "_renderers", renderers);
            RaceSceneBaker.SetArray(view, "_idleFrames", idle); RaceSceneBaker.SetArray(view, "_runFrames", run);
            SerializedObject data = new SerializedObject(view); SerializedProperty points = data.FindProperty("_path");
            points.arraySize = path.Length; for (int i = 0; i < path.Length; i++) points.GetArrayElementAtIndex(i).vector3Value = path[i];
            data.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }
        private static void ArrangeWorld(Transform root, GameplayWorld world)
        {
            Transform environment = root.Find("Stable and Farm Environment");
            environment.localPosition = new Vector3(-22, 1.5f); environment.localScale = Vector3.one * 0.55f;
            Transform forge = root.Find("Blacksmith Workstation");
            forge.localPosition = new Vector3(-35.2f, 1.7f); forge.localScale = Vector3.one * 0.65f;
            SetBool(forge.GetComponent<ForgeVisual>(), "_ambientOnly", true);
            Transform oldSign = forge.Find("Smith Sign"); if (oldSign != null) UnityEngine.Object.DestroyImmediate(oldSign.gameObject);
            WorldLabel(forge, "Smith Sign", new Vector3(-35.2f, 3.3f), "BLACKSMITH", 0.11f);
            foreach (Transform item in root)
                if (item.name.StartsWith("Carrot Plot", StringComparison.Ordinal) || item.name.StartsWith("Carrot Leaves", StringComparison.Ordinal) || item.name == "Stable Track Connection") item.gameObject.SetActive(false);
            foreach (string cameraName in new[] { "Stable Cinemachine", "Farm Cinemachine" })
            {
                CinemachineCamera camera = root.Find(cameraName).GetComponent<CinemachineCamera>();
                camera.transform.position = new Vector3(-25.7f, 0.4f, -10);
                LensSettings lens = camera.Lens; lens.OrthographicSize = 9.5f; camera.Lens = lens;
            }
            Camera physical = UnityEngine.Object.FindAnyObjectByType<Camera>();
            physical.transform.position = new Vector3(-25.7f, 0.4f, -10); physical.orthographicSize = 9.5f;
            physical.GetComponent<CinemachineBrain>().DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 0.15f);
            // The expanded connected view should not be windowboxed by the old race-only pixel camera.
            if (physical.TryGetComponent(out PixelPerfectCamera pixel)) pixel.enabled = false;
        }
        internal static TextMesh WorldLabel(Transform parent, string name, Vector3 position, string text, float size)
        {
            Transform label = Child(parent, name); label.position = position;
            TextMesh mesh = label.gameObject.AddComponent<TextMesh>(); mesh.text = text;
            mesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); mesh.fontSize = 64; mesh.characterSize = size;
            mesh.anchor = TextAnchor.MiddleCenter; mesh.alignment = TextAlignment.Center; mesh.color = new Color(1, 0.92f, 0.7f);
            MeshRenderer renderer = label.GetComponent<MeshRenderer>(); renderer.sharedMaterial = mesh.font.material; renderer.sortingOrder = 100;
            return mesh;
        }
        private static Transform Child(Transform parent, string name)
        { Transform result = new GameObject(name).transform; result.SetParent(parent, false); return result; }
        private static SpriteRenderer Rect(Transform parent, Sprite sprite, string name, Vector2 position, Vector2 size, Color color, int order)
            => RaceTrackBuilder.Rectangle(parent, sprite, name, position, size, color, order).GetComponent<SpriteRenderer>();
        internal static void Bind(UnityEngine.Object target, string field, UnityEngine.Object value) => RaceSceneBaker.SetReference(target, field, value);
        private static void SetBool(UnityEngine.Object target, string field, bool value)
        { SerializedObject data = new SerializedObject(target); data.FindProperty(field).boolValue = value; data.ApplyModifiedPropertiesWithoutUndo(); }
    }
}
