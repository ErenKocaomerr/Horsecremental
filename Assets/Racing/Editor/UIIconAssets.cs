using System.IO;
using UnityEditor;
using UnityEngine;

namespace Horsecremental.Racing.Editor
{
    internal static class UIIconAssets
    {
        private const string DIRECTORY = "Assets/Racing/Generated/UI";
        private const string SETTINGS_PATH = "Assets/Racing/Config/UIPresentation.asset";
        internal static UIPresentationSettings Ensure()
        {
            Directory.CreateDirectory(DIRECTORY);
            Sprite[] icons = new Sprite[21];
            for (int i = 0; i < icons.Length; i++)
            {
                string path = DIRECTORY + "/Icon" + i + ".png";
                if (!File.Exists(path))
                {
                    Texture2D texture = Draw(i);
                    File.WriteAllBytes(path, texture.EncodeToPNG());
                    Object.DestroyImmediate(texture);
                    AssetDatabase.ImportAsset(path);
                    TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
                    importer.textureType = TextureImporterType.Sprite;
                    importer.filterMode = FilterMode.Point;
                    importer.mipmapEnabled = false;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.spritePixelsPerUnit = 16;
                    importer.SaveAndReimport();
                }
                icons[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            UIPresentationSettings settings = AssetDatabase.LoadAssetAtPath<UIPresentationSettings>(SETTINGS_PATH);
            bool created = settings == null;
            if (created)
            {
                settings = ScriptableObject.CreateInstance<UIPresentationSettings>();
                AssetDatabase.CreateAsset(settings, SETTINGS_PATH);
            }
            SerializedObject data = new SerializedObject(settings);
            SerializedProperty sprites = data.FindProperty("_icons");
            sprites.arraySize = icons.Length;
            for (int i = 0; i < icons.Length; i++) sprites.GetArrayElementAtIndex(i).objectReferenceValue = icons[i];
            SerializedProperty positions = data.FindProperty("_treePositions");
            if (created || positions.arraySize != UpgradeTreeRules.NODE_COUNT)
            {
                Vector2[] defaults = {
                    new Vector2(-660, -30), new Vector2(-400, -30), new Vector2(-140, -30),
                    new Vector2(-400, 150), new Vector2(-140, 150), new Vector2(-400, -240),
                    new Vector2(-140, -240), new Vector2(-660, 330), new Vector2(-400, 330),
                    new Vector2(120, 150), new Vector2(380, 150)
                };
                positions.arraySize = defaults.Length;
                for (int i = 0; i < defaults.Length; i++) positions.GetArrayElementAtIndex(i).vector2Value = defaults[i];
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            return settings;
        }
        private static Texture2D Draw(int icon)
        {
            Texture2D t = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            t.SetPixels(new Color[256]);
            Color gold = new Color(1, 0.82f, 0.27f), green = new Color(0.46f, 0.93f, 0.38f);
            Color blue = new Color(0.6f, 0.76f, 1), purple = new Color(0.79f, 0.52f, 1);
            switch (icon)
            {
                case 0: Polygon(t, gold, 9,15, 3,7, 7,7, 5,0, 13,10, 9,10); break;
                case 1: Line(t,2,3,7,8,blue); Line(t,7,8,2,13,blue); Line(t,8,3,13,8,blue); Line(t,13,8,8,13,blue); break;
                case 2: Polygon(t,new Color(1,0.4f,0.26f), 8,15, 5,10, 4,12, 2,6, 4,2, 8,0, 13,3, 14,8, 10,12, 10,7); Polygon(t,gold, 8,9,5,4,8,2,11,4); break;
                case 3: Rect(t,2,2,12,6,new Color(0.68f,0.45f,0.27f)); Rect(t,3,10,3,2,green); Rect(t,10,10,3,2,green); Line(t,4,5,4,13,green); Line(t,11,5,11,13,green); break;
                case 4: Polygon(t,blue, 1,12,15,12,12,8,9,8,9,4,13,2,2,2,6,4,6,8,3,8); break;
                case 5: Circle(t,8,7,6,blue); Rect(t,6,14,4,2,blue); Line(t,8,7,8,11,gold); Line(t,8,7,11,5,gold); break;
                case 6: Circle(t,7,7,5,gold); Rect(t,6,4,2,7,new Color(0.55f,0.34f,0.13f)); Line(t,11,11,15,15,green); break;
                case 7: Rect(t,1,1,14,2,new Color(0.68f,0.45f,0.27f)); Line(t,8,3,8,11,green); Polygon(t,green, 8,7,3,8,2,13,6,12); Polygon(t,green, 8,10,11,14,15,14,13,10); break;
                case 8: Rect(t,2,2,12,12,new Color(0.76f,0.55f,0.31f)); Rect(t,4,4,8,8,new Color(0.4f,0.28f,0.16f)); Line(t,3,3,12,12,gold); Line(t,3,12,12,3,gold); break;
                case 9: Line(t,3,1,11,11,gold); Polygon(t,blue, 5,12,9,16,15,10,11,6); break;
                case 10: Polygon(t,blue, 2,4,13,4,15,8,12,11,5,11,1,7); Line(t,7,14,13,14,green); Line(t,13,14,11,12,green); break;
                case 11: Line(t,3,13,3,5,blue); Line(t,3,5,6,2,blue); Line(t,6,2,10,2,blue); Line(t,10,2,13,5,blue); Line(t,13,5,13,13,blue); Rect(t,2,11,3,3,gold); Rect(t,11,11,3,3,gold); break;
                case 12: Polygon(t,new Color(0.78f,0.45f,0.28f), 2,12,6,10,10,10,14,12,13,4,3,4); Rect(t,6,6,4,3,gold); Line(t,3,4,3,1,gold); Line(t,13,4,13,1,gold); break;
                case 13: Polygon(t,purple, 5,14,11,14,13,10,12,5,9,2,4,3,3,7); Rect(t,7,5,5,2,gold); Line(t,5,14,5,4,gold); break;
                case 14: Circle(t,8,8,7,gold); Rect(t,6,4,4,8,new Color(0.65f,0.4f,0.1f)); break;
                case 15: Polygon(t,purple, 3,14,13,14,12,6,8,1,4,6); Polygon(t,blue, 6,11,10,11,10,7,8,4,6,7); break;
                case 16: Polygon(t,blue, 1,4,12,4,15,8,12,12,5,12,1,8); Line(t,2,8,14,8,Color.white); break;
                case 17: Polygon(t,new Color(1,0.55f,0.2f), 3,10,12,12,8,1); Line(t,9,11,8,15,green); Line(t,9,11,14,14,green); break;
                case 18: Rect(t,3,1,10,8,Color.white); Line(t,4,8,4,12,Color.white); Line(t,4,12,6,14,Color.white); Line(t,6,14,10,14,Color.white); Line(t,10,14,12,12,Color.white); Line(t,12,12,12,8,Color.white); Rect(t,7,3,2,4,Color.black); break;
                case 19: Line(t,2,7,6,3,Color.white); Line(t,6,3,14,12,Color.white); break;
                case 20: Rect(t,0,0,16,1,Color.white); Rect(t,0,15,16,1,Color.white); Rect(t,0,0,1,16,Color.white); Rect(t,15,0,1,16,Color.white); break;
            }
            t.Apply();
            return t;
        }
        private static void Rect(Texture2D t, int x, int y, int width, int height, Color c)
        { for (int yy = y; yy < y + height; yy++) for (int xx = x; xx < x + width; xx++) Pixel(t, xx, yy, c); }
        private static void Pixel(Texture2D t, int x, int y, Color c) { if (x >= 0 && x < 16 && y >= 0 && y < 16) t.SetPixel(x, y, c); }
        private static void Line(Texture2D t, int x, int y, int endX, int endY, Color c)
        {
            int steps = Mathf.Max(Mathf.Abs(endX - x), Mathf.Abs(endY - y));
            for (int i = 0; i <= steps; i++)
            { float u = steps == 0 ? 0 : (float)i / steps; Rect(t, Mathf.RoundToInt(Mathf.Lerp(x,endX,u)), Mathf.RoundToInt(Mathf.Lerp(y,endY,u)), 2, 2, c); }
        }
        private static void Circle(Texture2D t, int x, int y, int radius, Color c)
        { for (int yy = 0; yy < 16; yy++) for (int xx = 0; xx < 16; xx++) if ((xx-x)*(xx-x)+(yy-y)*(yy-y) <= radius*radius) Pixel(t,xx,yy,c); }
        private static void Polygon(Texture2D t, Color c, params int[] points)
        {
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++)
            {
                bool inside = false;
                for (int i = 0, j = points.Length-2; i < points.Length; j = i, i += 2)
                    if ((points[i+1] > y) != (points[j+1] > y) && x < (float)(points[j]-points[i])*(y-points[i+1])/(points[j+1]-points[i+1])+points[i]) inside = !inside;
                if (inside) Pixel(t,x,y,c);
            }
        }
    }
}
