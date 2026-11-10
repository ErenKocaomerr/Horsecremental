using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Horsecremental.Racing.Editor
{
    internal static class MainRaceArtAssets
    {
        internal const string ROOT = "Assets/Sprites/Main Race Expansion";
        private static readonly string[] PROP_NAMES =
        {
            "Fence", "HurdleDraft", "Hay", "DistanceStake", "StartFlag", "FinishFlag",
            "Arrow", "Approach", "Bush", "Rocks", "Bench", "Flowers"
        };

        internal static void Prepare()
        {
            Import("RaceTerrain", new[] { "Dirt", "Grass" }, 2, 1, false, 256f);
            Import("RaceBackdrop", new[] { "Backdrop" }, 1, 1, false, 288f);
            Import("RaceProps", PROP_NAMES, 4, 3, true, 160f);
            Import("RaceHurdle", new[] { "TimberHurdle" }, 1, 1, true, 40f);
            Import("VergeDetails", new[] { "GrassA", "GrassB", "Pebbles", "Flower" }, 4, 1, true, 96f);
        }

        internal static Sprite Get(string sheet, string name)
        {
            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(ROOT + "/" + sheet + ".png"))
                if (asset is Sprite sprite && sprite.name == name) return sprite;
            throw new InvalidOperationException("Missing Main Race sprite: " + name);
        }

        private static void Import(string sheet, string[] names, int columns, int rows, bool alpha, float logicalWidth)
        {
            string path = ROOT + "/" + sheet + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing race texture: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = alpha;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 4096;
            importer.isReadable = true;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteExtrude = 0;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            importer.spritePixelsPerUnit = texture.width / logicalWidth * 24f;
            Color32[] pixels = alpha ? texture.GetPixels32() : null;
            SpriteDataProviderFactories factories = new SpriteDataProviderFactories();
            factories.Init();
            ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            SpriteRect[] previous = provider.GetSpriteRects();
            SpriteRect[] rectangles = new SpriteRect[names.Length];
            List<SpriteNameFileIdPair> pairs = new List<SpriteNameFileIdPair>(names.Length);
            for (int i = 0; i < names.Length; i++)
            {
                GUID id = GUID.Generate();
                foreach (SpriteRect old in previous) if (old.name == names[i]) id = old.spriteID;
                int left = i % columns * texture.width / columns;
                int right = (i % columns + 1) * texture.width / columns;
                int bottom = texture.height - (i / columns + 1) * texture.height / rows;
                int top = texture.height - i / columns * texture.height / rows;
                Rect rect = alpha ? Bounds(pixels, texture.width, left, right, bottom, top) :
                    new Rect(left, bottom, right - left, top - bottom);
                rectangles[i] = new SpriteRect
                {
                    name = names[i], spriteID = id, rect = rect, alignment = SpriteAlignment.Custom,
                    pivot = alpha ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f)
                };
                pairs.Add(new SpriteNameFileIdPair(names[i], id));
            }
            provider.SetSpriteRects(rectangles);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(pairs);
            provider.Apply();
            importer.isReadable = false;
            importer.SaveAndReimport();
        }

        private static Rect Bounds(Color32[] pixels, int width, int left, int right, int bottom, int top)
        {
            int minX = right, minY = top, maxX = -1, maxY = -1;
            for (int y = bottom; y < top; y++)
                for (int x = left; x < right; x++)
                {
                    if (pixels[y * width + x].a < 180) continue;
                    minX = Math.Min(minX, x); minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y);
                }
            if (maxX < 0) throw new InvalidOperationException("Empty race sprite cell.");
            return Rect.MinMaxRect(minX, minY, maxX + 1, maxY + 1);
        }
    }
}
