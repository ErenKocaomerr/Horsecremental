using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Horsecremental.Racing.Editor
{
    public static class WoodlandExpansionSetup
    {
        private const string ROOT = "Assets/Sprites/Horse Woodland Expansion";
        private const float TILE_SIZE = 32f / 24f;
        private static readonly string[] TERRAIN_NAMES =
        {
            "GrassA", "GrassB", "DirtA", "DirtB",
            "EdgeTop", "EdgeBottom", "EdgeLeft", "EdgeRight",
            "OuterTopLeft", "OuterTopRight", "OuterBottomLeft", "OuterBottomRight",
            "InnerTopLeft", "InnerTopRight", "InnerBottomLeft", "InnerBottomRight"
        };
        private static readonly string[] OBSTACLE_NAMES =
        {
            "OakLog", "SingleRail", "DoubleRail", "RockPile",
            "HayBale", "HayBales", "TimberGate", "BrokenFence",
            "MossyLog", "Stump", "FallenBranch", "StoneWall"
        };

        [MenuItem("Horsecremental/Woodland Expansion/Prepare Assets")]
        public static void PrepareAssets()
        {
            Directory.CreateDirectory(ROOT + "/Tiles");
            Directory.CreateDirectory(ROOT + "/Prefabs");
            AssetDatabase.Refresh();
            ImportSheet("WoodlandTerrain", TERRAIN_NAMES, false);
            ImportSheet("WoodlandObstacles", OBSTACLE_NAMES, true);
            ImportReferenceHorse();
            Material material = GetMaterial();
            foreach (string name in TERRAIN_NAMES)
            {
                string path = ROOT + "/Tiles/" + name + ".asset";
                Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
                if (tile == null)
                {
                    tile = ScriptableObject.CreateInstance<Tile>();
                    AssetDatabase.CreateAsset(tile, path);
                }
                tile.sprite = FindSprite("WoodlandTerrain", name);
                tile.colliderType = Tile.ColliderType.None;
                EditorUtility.SetDirty(tile);
            }
            foreach (string name in OBSTACLE_NAMES)
            {
                GameObject visual = CreateVisual(name, FindSprite("WoodlandObstacles", name), material, Vector2.zero, 0);
                try { PrefabUtility.SaveAsPrefabAsset(visual, ROOT + "/Prefabs/" + name + ".prefab"); }
                finally { UnityEngine.Object.DestroyImmediate(visual); }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Woodland Expansion ready: 16 terrain sprites/tiles, 12 obstacle sprites/prefabs.");
        }

        private static void ImportSheet(string sheet, string[] names, bool obstacles)
        {
            string path = ROOT + "/" + sheet + ".png";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing texture: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = obstacles;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            importer.isReadable = obstacles;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteExtrude = 0;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            importer.spritePixelsPerUnit = texture.width / (obstacles ? 256f : 128f) * 24f;
            SpriteDataProviderFactories factories = new SpriteDataProviderFactories();
            factories.Init();
            ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            SpriteRect[] previous = provider.GetSpriteRects();
            SpriteRect[] rectangles = new SpriteRect[names.Length];
            List<SpriteNameFileIdPair> pairs = new List<SpriteNameFileIdPair>(names.Length);
            Color32[] pixels = obstacles ? texture.GetPixels32() : null;
            for (int i = 0; i < names.Length; i++)
            {
                GUID id = GUID.Generate();
                foreach (SpriteRect old in previous)
                    if (old.name == names[i]) id = old.spriteID;
                Rect rectangle = obstacles ? ObstacleBounds(pixels, texture.width, texture.height, i) :
                    new Rect(i % 4 * texture.width / 4f, texture.height - (i / 4 + 1) * texture.height / 4f,
                        texture.width / 4f, texture.height / 4f);
                rectangles[i] = new SpriteRect
                {
                    name = names[i], spriteID = id, rect = rectangle,
                    alignment = SpriteAlignment.Custom,
                    pivot = obstacles ? new Vector2(0.5f, 2f / rectangle.height) : new Vector2(0.5f, 0.5f)
                };
                pairs.Add(new SpriteNameFileIdPair(names[i], id));
            }
            provider.SetSpriteRects(rectangles);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(pairs);
            provider.Apply();
            importer.isReadable = false;
            importer.SaveAndReimport();
            foreach (string name in names) FindSprite(sheet, name);
        }

        private static Rect ObstacleBounds(Color32[] pixels, int width, int height, int index)
        {
            // The generated sheet has uneven empty margins; isolate the visible alpha bounds.
            int[] columns = { 0, 330, 635, 925, 1254 };
            int[] rowTops = { 0, 550, 860 };
            int[] rowBottoms = { 550, 860, 1254 };
            int column = index % 4;
            int row = index / 4;
            int left = Mathf.RoundToInt(columns[column] * width / 1254f);
            int right = Mathf.RoundToInt(columns[column + 1] * width / 1254f);
            int bottom = height - Mathf.RoundToInt(rowBottoms[row] * height / 1254f);
            int top = height - Mathf.RoundToInt(rowTops[row] * height / 1254f);
            int minX = right, minY = top, maxX = -1, maxY = -1;
            for (int y = bottom; y < top; y++)
                for (int x = left; x < right; x++)
                {
                    if (pixels[y * width + x].a < 16) continue;
                    minX = Math.Min(minX, x); minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y);
                }
            if (maxX < 0) throw new InvalidOperationException("Empty obstacle cell: " + index);
            return Rect.MinMaxRect(Math.Max(left, minX - 2), Math.Max(bottom, minY - 2),
                Math.Min(right, maxX + 3), Math.Min(top, maxY + 3));
        }

        private static Sprite FindSprite(string sheet, string name)
        {
            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(ROOT + "/" + sheet + ".png"))
                if (asset is Sprite sprite && sprite.name == name) return sprite;
            throw new InvalidOperationException("Missing sprite: " + name);
        }

        private static void ImportReferenceHorse()
        {
            string path = ROOT + "/ReferenceHorse.png";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing reference horse.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(0.5f, 0.14f);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            importer.spritePixelsPerUnit = texture.width * 0.885f / 64f * 24f;
            importer.SaveAndReimport();
        }

        private static Material GetMaterial()
        {
            string path = ROOT + "/WoodlandUnlit.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) throw new InvalidOperationException("No sprite shader available.");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static GameObject CreateVisual(string name, Sprite sprite, Material material, Vector2 position, int order)
        {
            GameObject visual = new GameObject(name);
            visual.transform.position = position;
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = order;
            return visual;
        }

        [MenuItem("Horsecremental/Woodland Expansion/Create Preview Scene")]
        public static void CreatePreviewScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Create the preview in Edit mode.");
            Scene previous = SceneManager.GetActiveScene();
            Scene preview = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
                Application.isBatchMode ? NewSceneMode.Single : NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(preview);
                PrepareAssets();
                Material material = GetMaterial();
                GameObject grid = new GameObject("Woodland Ground", typeof(Grid));
                grid.GetComponent<Grid>().cellSize = new Vector3(TILE_SIZE, TILE_SIZE, 1f);
                GameObject ground = new GameObject("Grass and Dirt Track", typeof(Tilemap), typeof(TilemapRenderer));
                ground.transform.SetParent(grid.transform, false);
                Tilemap map = ground.GetComponent<Tilemap>();
                TilemapRenderer renderer = ground.GetComponent<TilemapRenderer>();
                renderer.sharedMaterial = material;
                renderer.sortingOrder = -100;
                for (int y = -5; y <= 4; y++)
                    for (int x = -9; x <= 8; x++)
                    {
                        string name = y == 1 ? "EdgeTop" : y == -2 ? "EdgeBottom" :
                            y >= -1 && y <= 0 ? ((x + y) % 2 == 0 ? "DirtA" : "DirtB") :
                            (x + y) % 2 == 0 ? "GrassA" : "GrassB";
                        map.SetTile(new Vector3Int(x, y, 0),
                            AssetDatabase.LoadAssetAtPath<Tile>(ROOT + "/Tiles/" + name + ".asset"));
                    }
                for (int i = 0; i < OBSTACLE_NAMES.Length; i++)
                {
                    Vector2 position = new Vector2(-10f + i % 6 * 4f, i < 6 ? 4f : -5f);
                    CreateVisual(OBSTACLE_NAMES[i], FindSprite("WoodlandObstacles", OBSTACLE_NAMES[i]), material, position, 10);
                }
                Sprite horse = AssetDatabase.LoadAssetAtPath<Sprite>(ROOT + "/ReferenceHorse.png");
                if (horse == null) throw new InvalidOperationException("Missing reference horse sprite.");
                CreateVisual("Attached Horse Style Reference", horse, material, new Vector2(-6f, -0.4f), 20);
                CreateVisual("Track Jump", FindSprite("WoodlandObstacles", "SingleRail"), material, new Vector2(0f, -0.4f), 10);
                CreateVisual("Track Log", FindSprite("WoodlandObstacles", "OakLog"), material, new Vector2(7f, -0.4f), 10);
                Camera camera = new GameObject("Preview Camera", typeof(Camera)).GetComponent<Camera>();
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.orthographic = true;
                camera.orthographicSize = 7.5f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.13f, 0.19f, 0.1f);
                string scenePath = Application.isBatchMode ? ROOT + "/WoodlandPreview.unity" :
                    AssetDatabase.GenerateUniqueAssetPath(ROOT + "/WoodlandPreview.unity");
                EditorSceneManager.SaveScene(preview, scenePath);
                Directory.CreateDirectory("Tools/WoodlandExpansion");
                RenderTexture target = new RenderTexture(1920, 1080, 24);
                RenderTexture previousTarget = RenderTexture.active;
                Texture2D screenshot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
                try
                {
                    camera.targetTexture = target;
                    camera.Render();
                    RenderTexture.active = target;
                    screenshot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
                    screenshot.Apply();
                    File.WriteAllBytes("Tools/WoodlandExpansion/Preview.png", screenshot.EncodeToPNG());
                }
                finally
                {
                    camera.targetTexture = null;
                    RenderTexture.active = previousTarget;
                    target.Release();
                    UnityEngine.Object.DestroyImmediate(target);
                    UnityEngine.Object.DestroyImmediate(screenshot);
                }
                Debug.Log("Woodland preview saved to " + scenePath);
            }
            finally
            {
                if (!Application.isBatchMode)
                {
                    if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                    EditorSceneManager.CloseScene(preview, true);
                }
            }
        }
    }
}
