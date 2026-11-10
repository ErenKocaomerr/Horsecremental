using System.IO;
using UnityEditor;
using UnityEngine;

namespace Horsecremental.Racing.Editor
{
    // Small authored pixel surfaces. Nine-slicing keeps bevels crisp at every panel size.
    internal static class RuralUIAssets
    {
        internal const string DIRECTORY = "Assets/Racing/Art/UI/Rural";
        internal const string THEME_PATH = "Assets/Racing/Config/RuralUITheme.asset";
        internal static RuralUITheme Theme => AssetDatabase.LoadAssetAtPath<RuralUITheme>(THEME_PATH);
        internal static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(DIRECTORY + "/" + name + ".png");

        internal static void Ensure()
        {
            Directory.CreateDirectory(DIRECTORY);
            Surface("Parchment", "503828", "9C653D", "D6AA69", "F3E4BD", "FFF2D0");
            Surface("Inset", "8F704B", "BA9664", "D8BA83", "E5D2A7", "F5E4BD");
            Surface("Wood", "382A23", "6D4630", "B78249", "865C3B", "C6965B");
            Surface("SageButton", "344634", "536B45", "8EAA69", "687F50", "A2B778");
            Surface("GoldButton", "68412C", "AF773D", "E6B765", "CA964C", "F4D58C");
            for (int i = 0; i < 4; i++) Save("Course" + i, Landscape(i), 0);
            if (Theme == null) AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<RuralUITheme>(), THEME_PATH);
            AssetDatabase.SaveAssets();
        }

        private static void Surface(string name, string outline, string edge, string bevel, string fill, string light)
        {
            Texture2D t = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[1024];
            Color[] palette = { Hex(outline), Hex(edge), Hex(bevel), Hex(fill), Hex(light) };
            for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
            {
                int d = Mathf.Min(x, y, 31 - x, 31 - y);
                Color c = d < 1 ? palette[0] : d < 3 ? palette[1] : d < 4 ? palette[2] : palette[3];
                if (d == 4 && y > 16) c = palette[4];
                if ((x < 2 || x > 29) && (y < 2 || y > 29)) c = Color.clear;
                // Restrained grain lives in the fixed corners, never across the stretchable centre.
                if ((x == 5 || x == 26) && (y == 5 || y == 26)) c = palette[1];
                pixels[y * 32 + x] = c;
            }
            t.SetPixels(pixels); t.Apply(); Save(name, t, 8);
        }

        private static Texture2D Landscape(int course)
        {
            Texture2D t = new Texture2D(128, 64, TextureFormat.RGBA32, false);
            Color sky = Hex(course == 3 ? "D9B67D" : "C8D9B1");
            Color distant = Hex(course == 2 ? "697C5C" : "84956B");
            Color green = Hex(course == 3 ? "88834A" : "94A361");
            for (int y = 0; y < 64; y++) for (int x = 0; x < 128; x++)
            {
                int horizon = 35 + Mathf.RoundToInt(Mathf.Sin(x * .055f + course) * 6);
                t.SetPixel(x, y, y > 43 ? sky : y > horizon ? distant : green);
            }
            Rect(t, 15, 51, 19, 3, Hex("EAE8C8")); Rect(t, 21, 54, 10, 3, Hex("EAE8C8"));
            for (int i = 0; i < 7; i++)
            {
                int x = i * 23 - 8 + course * 3; int y = 30 + i % 3 * 3;
                Rect(t, x + 7, y - 9, 3, 15, Hex("806143"));
                Rect(t, x + 1, y, 17, 10, Hex("5C734B")); Rect(t, x + 4, y + 8, 11, 7, Hex("6E8251"));
                Rect(t, x, y + 1, 8, 5, Hex("A7AF6C"));
            }
            Rect(t, 0, 9, 128, 11, Hex("BB9565")); Rect(t, 0, 18, 128, 2, Hex("DBC08B"));
            for (int x = 4; x < 128; x += 17)
            {
                Rect(t, x, 21, 3, 9, Hex("8E6945")); Rect(t, x, 28, 3, 2, Hex("DCC08A"));
                Rect(t, x, 23, 17, 2, Hex("C3A16A"));
            }
            // A tiny chestnut horse and the course's distinctive obstacle count.
            Rect(t, 30, 13, 16, 7, Hex("75412C")); Rect(t, 42, 18, 6, 7, Hex("75412C"));
            Rect(t, 46, 19, 5, 3, Hex("75412C")); Rect(t, 30, 8, 3, 6, Hex("513828"));
            Rect(t, 40, 8, 3, 6, Hex("513828")); Rect(t, 27, 15, 3, 6, Hex("513828"));
            for (int i = 0; i <= course; i++)
            { int x = 68 + i * 14; Rect(t, x, 10, 2, 10, Hex("6F5338")); Rect(t, x + 8, 10, 2, 10, Hex("6F5338")); Rect(t, x, 17, 10, 2, Hex("E8C68A")); }
            if (course == 3) for (int y = 0; y < 7; y++) for (int x = 0; x < 10; x++)
                Rect(t, 113 + x, 37 + y, 1, 1, (x / 2 + y / 2) % 2 == 0 ? Hex("F0DBAE") : Hex("654A35"));
            t.Apply(); return t;
        }

        private static void Rect(Texture2D t, int x, int y, int w, int h, Color c)
        { for (int yy = Mathf.Max(0, y); yy < Mathf.Min(t.height, y + h); yy++) for (int xx = Mathf.Max(0, x); xx < Mathf.Min(t.width, x + w); xx++) t.SetPixel(xx, yy, c); }
        private static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out Color c); return c; }
        private static void Save(string name, Texture2D texture, int border)
        {
            string path = DIRECTORY + "/" + name + ".png";
            File.WriteAllBytes(path, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100; importer.spriteBorder = Vector4.one * border;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
        }
    }
}
