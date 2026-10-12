using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Horsecremental.Racing.Editor
{
    internal static class RaceSpriteAssets
    {
        internal static Sprite[] ImportFrames(Texture2D source, int horse)
        {
            string path = "Assets/Racing/Generated/Horse" + horse + "Frames.png";
            File.Copy(AssetDatabase.GetAssetPath(source), path, true);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 24;
            TextureImporterSettings textureSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(textureSettings);
            textureSettings.spriteMeshType = SpriteMeshType.FullRect;
            textureSettings.spriteExtrude = 0;
            importer.SetTextureSettings(textureSettings);
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            SpriteDataProviderFactories factories = new SpriteDataProviderFactories();
            factories.Init();
            ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            SpriteRect[] previous = provider.GetSpriteRects();
            SpriteRect[] rectangles = new SpriteRect[7];
            List<SpriteNameFileIdPair> namePairs = new List<SpriteNameFileIdPair>(7);
            for (int i = 0; i < rectangles.Length; i++)
            {
                string name = "Horse" + horse + (i == 0 ? "Idle" : "Run" + (i - 1));
                GUID spriteId = GUID.Generate();
                for (int old = 0; old < previous.Length; old++)
                    if (previous[old].name == name) spriteId = previous[old].spriteID;
                int column = i == 0 ? 0 : i - 1;
                int row = i == 0 ? 5 : 13;
                rectangles[i] = new SpriteRect
                {
                    name = name,
                    spriteID = spriteId,
                    rect = new Rect(column * 64, source.height - (row + 1) * 48 - 8, 64, 48),
                    alignment = SpriteAlignment.Custom,
                    pivot = new Vector2(0.5f, 10f / 48f)
                };
                namePairs.Add(new SpriteNameFileIdPair(name, spriteId));
            }
            provider.SetSpriteRects(rectangles);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(namePairs);
            provider.Apply();
            importer.SaveAndReimport();
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            Sprite[] frames = new Sprite[7];
            for (int i = 0; i < frames.Length; i++)
            {
                foreach (UnityEngine.Object asset in assets)
                    if (asset is Sprite sprite && sprite.name == rectangles[i].name) frames[i] = sprite;
                if (frames[i] == null) throw new InvalidOperationException("Missing imported horse frame: " + rectangles[i].name);
            }
            return frames;
        }
    }
}
