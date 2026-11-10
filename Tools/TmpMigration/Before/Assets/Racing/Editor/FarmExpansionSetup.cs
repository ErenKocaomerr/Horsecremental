using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Horsecremental.Racing.Editor
{
    public static class FarmExpansionSetup
    {
        [MenuItem("Horsecremental/Farm Expansion/Apply to Current World")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != Milestone3SceneSetup.WORLD_PATH) throw new InvalidOperationException("Open Milestone3World first.");
            GameplayWorld world = UnityEngine.Object.FindAnyObjectByType<GameplayWorld>();
            if (world == null) throw new InvalidOperationException("The existing gameplay world is required.");
            FarmExpansionAssets.Prepare();
            Transform root = world.transform;
            Transform previous = root.Find("Farm Expansion Art");
            if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
            Transform art = new GameObject("Farm Expansion Art").transform;
            art.SetParent(root, false);
            Sprite background = AssetDatabase.LoadAssetAtPath<Sprite>(FarmExpansionAssets.ROOT + "/Background.png");
            Place(art, "Farm Background", background, new Vector2(-25.7f, 0.4f), 34f, -25);
            Transform environment = root.Find("Stable and Farm Environment");
            foreach (SpriteRenderer renderer in environment.GetComponentsInChildren<SpriteRenderer>(true))
                if (renderer.name != "Stable Player Horse") renderer.enabled = false;
            Transform horse = environment.Find("Stable Player Horse");
            horse.position = new Vector3(-25.5f, 1.0f, 0);
            horse.localScale = Vector3.one * (2.2f / environment.lossyScale.x);
            horse.GetComponent<SpriteRenderer>().sortingOrder = 30;
            Place(art, "Stable", Building("Stable"), new Vector2(-25.8f, 1.65f), 7f, 15);
            Place(art, "Feed Shed", Building("FeedShed"), new Vector2(-19.4f, 3.8f), 3.1f, 14);
            Place(art, "Blacksmith", Building("Blacksmith"), new Vector2(-35.2f, 2.0f), 4.8f, 15);
            Transform forge = root.Find("Blacksmith Workstation");
            foreach (SpriteRenderer renderer in forge.GetComponentsInChildren<SpriteRenderer>(true)) renderer.enabled = false;
            MoveLabel(forge.Find("Smith Sign"), new Vector3(-35.2f, 6.0f), "BLACKSMITH");
            Decorate(art);
            ApplyFarm(root, world);
            foreach (SpriteRenderer renderer in root.Find("Stable Playground/Carrot Pickup Pool").GetComponentsInChildren<SpriteRenderer>(true))
                renderer.sprite = Prop("Carrots");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Farm Expansion applied: 3 buildings, environment props, background and event-driven crop stages.");
        }

        private static void Decorate(Transform art)
        {
            Place(art, "Hay by Stable", Prop("HayBale"), new Vector2(-29.1f, 1.8f), 1.0f, 26);
            Place(art, "Water Trough", Prop("WaterTrough"), new Vector2(-22.4f, 0.5f), 1.8f, 26);
            Place(art, "Feed Bucket", Prop("FeedBucket"), new Vector2(-23.1f, 2.0f), 0.6f, 26);
            for (int i = 0; i < 5; i++)
                Place(art, "Paddock Fence " + i, Prop(i == 2 ? "Gate" : "Fence"), new Vector2(-29.0f + i * 1.75f, -0.8f), 1.78f, 35);
            Place(art, "Oak by Smith", Prop("Oak"), new Vector2(-39.6f, 2.9f), 2.9f, 16);
            Place(art, "Bush by Smith", Prop("Bush"), new Vector2(-38.0f, 1.1f), 1.5f, 20);
            Place(art, "Rocks by Field", Prop("Rocks"), new Vector2(-11.1f, 0.4f), 1.3f, 20);
        }

        private static void ApplyFarm(Transform root, GameplayWorld world)
        {
            Transform farm = root.Find("Stable Playground/Physical Farm");
            FarmCropAppearance appearance = farm.GetComponent<FarmCropAppearance>();
            if (appearance == null) appearance = farm.gameObject.AddComponent<FarmCropAppearance>();
            SpriteRenderer[] plots = new SpriteRenderer[6];
            for (int i = 0; i < plots.Length; i++)
            {
                Transform plot = farm.Find("Crop Plot " + i);
                plot.position = new Vector3(-17.0f + i % 3 * 1.7f, 3.8f - i / 3 * 1.8f);
                plot.Find("Soil").GetComponent<SpriteRenderer>().enabled = false;
                plot.Find("Growing Carrot").GetComponent<SpriteRenderer>().enabled = false;
                Transform previous = plot.Find("Crop Stage Art");
                if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
                plots[i] = Place(plot, "Crop Stage Art", Prop("Sprout"), (Vector2)plot.position + new Vector2(0, -0.55f), 1.45f, 24);
            }
            RaceSceneBaker.SetReference(appearance, "_world", world);
            RaceSceneBaker.SetArray(appearance, "_plots", plots);
            RaceSceneBaker.SetArray(appearance, "_stages", new[] { Prop("Soil"), Prop("Sprout"), Prop("Sprout"), Prop("Ready") });
            MoveLabel(farm.Find("Farm Sign"), new Vector3(-15.3f, 5.1f), "FARM");
            MoveLabel(farm.Find("Farm Hint"), new Vector3(-15.3f, 0.7f), "Click crops • Sweep drops");
        }

        private static Sprite Building(string name) => FarmExpansionAssets.Get("Buildings", name);
        private static Sprite Prop(string name) => FarmExpansionAssets.Get("Props", name);

        private static SpriteRenderer Place(Transform parent, string name, Sprite sprite, Vector2 position, float width, int order)
        {
            GameObject item = new GameObject(name, typeof(SpriteRenderer));
            item.transform.SetParent(parent, false);
            item.transform.position = new Vector3(position.x, position.y, 0);
            item.transform.localScale = Vector3.one * (width / sprite.bounds.size.x);
            SpriteRenderer renderer = item.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            return renderer;
        }

        private static void MoveLabel(Transform label, Vector3 position, string text)
        {
            if (label == null) return;
            label.position = position;
            label.GetComponent<TextMesh>().text = text;
        }
    }
}
