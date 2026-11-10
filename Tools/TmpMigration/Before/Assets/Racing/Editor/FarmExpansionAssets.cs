using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Horsecremental.Racing.Editor
{
    internal static class FarmExpansionAssets
    {
        internal const string ROOT = "Assets/Sprites/Farm Expansion v2";
        private static readonly string[] BUILDINGS = { "Stable", "FeedShed", "Blacksmith" };
        private static readonly string[] PROPS =
        {
            "Fence", "Gate", "HayBale", "WaterTrough", "Oak", "Bush", "Rocks", "FeedBucket",
            "Soil", "Sprout", "Ready", "Carrots"
        };

        internal static void Prepare()
        {
            Import("Buildings", BUILDINGS, 3, 1, 192f);
            Import("Props", PROPS, 4, 3, 160f);
            TextureImporter importer = Configure("Background", false);
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 48f;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }

        internal static Sprite Get(string sheet, string name)
        {
            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(ROOT + "/" + sheet + ".png"))
                if (asset is Sprite sprite && sprite.name == name) return sprite;
            throw new InvalidOperationException("Missing Farm Expansion sprite: " + name);
        }

        private static TextureImporter Configure(string sheet, bool alpha)
        {
            string path = ROOT + "/" + sheet + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing texture: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = alpha;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 4096;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteExtrude = 0;
            importer.SetTextureSettings(settings);
            return importer;
        }

        private static void Import(string sheet, string[] names, int columns, int rows, float logicalWidth)
        {
            TextureImporter importer = Configure(sheet, true);
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.isReadable = true;
            importer.SaveAndReimport();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(ROOT + "/" + sheet + ".png");
            importer.spritePixelsPerUnit = texture.width / logicalWidth * 24f;
            Color32[] pixels = texture.GetPixels32();
            SpriteDataProviderFactories factories = new SpriteDataProviderFactories();
            factories.Init();
            ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            SpriteRect[] previous = provider.GetSpriteRects();
            SpriteRect[] rectangles = new SpriteRect[names.Length];
            List<SpriteNameFileIdPair> pairs = new List<SpriteNameFileIdPair>();
            for (int i = 0; i < names.Length; i++)
            {
                GUID id = GUID.Generate();
                foreach (SpriteRect old in previous) if (old.name == names[i]) id = old.spriteID;
                // Building spacing is deliberately uneven in the approved source sheet.
                int[] buildingCuts = { 0, 900, 1440, 2172 };
                int left = rows == 1 ? Mathf.RoundToInt(buildingCuts[i] * texture.width / 2172f) : i % columns * texture.width / columns;
                int right = rows == 1 ? Mathf.RoundToInt(buildingCuts[i + 1] * texture.width / 2172f) : (i % columns + 1) * texture.width / columns;
                int bottom = texture.height - (i / columns + 1) * texture.height / rows;
                int top = texture.height - i / columns * texture.height / rows;
                Rect rect = Bounds(pixels, texture.width, left, right, bottom, top);
                rectangles[i] = new SpriteRect { name = names[i], spriteID = id, rect = rect,
                    alignment = SpriteAlignment.Custom, pivot = new Vector2(0.5f, 0.02f) };
                pairs.Add(new SpriteNameFileIdPair(names[i], id));
            }
            provider.SetSpriteRects(rectangles);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(pairs);
            provider.Apply();
            importer.isReadable = false;
            // Keep the approved source pixels; importer resizing adds soft edges.
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
        }

        private static Rect Bounds(Color32[] pixels, int width, int left, int right, int bottom, int top)
        {
            int minX = right, minY = top, maxX = -1, maxY = -1;
            for (int y = bottom; y < top; y++)
                for (int x = left; x < right; x++)
                {
                    if (pixels[y * width + x].a < 128) continue;
                    minX = Math.Min(minX, x); minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y);
                }
            if (maxX < 0) throw new InvalidOperationException("Empty farm sprite cell.");
            return Rect.MinMaxRect(minX, minY, maxX + 1, maxY + 1);
        }
    }
}
