using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Horsecremental.Racing.Editor
{
    internal static class PixelFontAssets
    {
        internal const string FONT_PATH = "Assets/Racing/Resources/Fonts/m6x11 SDF.asset";
        private static TMP_FontAsset _font;
        internal static TMP_FontAsset Font => _font != null ? _font : Ensure();

        internal static TMP_FontAsset Ensure()
        {
            _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);
            if (_font == null)
            {
                Font source = AssetDatabase.LoadAssetAtPath<Font>("Assets/m6x11 (1).ttf");
                if (source == null) throw new InvalidOperationException("The project's m6x11 font is missing.");
                System.IO.Directory.CreateDirectory("Assets/Racing/Resources/Fonts");
                AssetDatabase.Refresh();
                _font = TMP_FontAsset.CreateFontAsset(source, 66, 8, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
                if (_font == null) throw new InvalidOperationException("Could not create the m6x11 TMP font asset.");
                _font.name = "m6x11 SDF";
                AssetDatabase.CreateAsset(_font, FONT_PATH);
                foreach (Texture2D atlas in _font.atlasTextures)
                {
                    atlas.name = "m6x11 Atlas";
                    AssetDatabase.AddObjectToAsset(atlas, _font);
                }
                _font.material.name = "m6x11 Material";
                AssetDatabase.AddObjectToAsset(_font.material, _font);
                _font.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 !\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~", out string missing);
                if (!string.IsNullOrEmpty(missing)) Debug.LogWarning("m6x11 has no glyphs for: " + missing);
                EditorUtility.SetDirty(_font);
            }
            TMP_Settings.defaultFontAsset = _font;
            EditorUtility.SetDirty(TMP_Settings.instance);
            AssetDatabase.SaveAssets();
            return _font;
        }

        internal static void Configure(TMP_Text text, float size, TextAlignmentOptions alignment)
        {
            text.font = Font;
            text.fontSize = size;
            text.fontStyle = FontStyles.Normal;
            text.alignment = alignment;
            text.enableAutoSizing = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = false;
        }

        internal static string ReadableText(string text) => text.Replace('\u2192', '>').Replace('\u2022', '|')
            .Replace('\u2013', '-').Replace('\u2014', '-').Replace('\u2212', '-')
            .Replace('\u015e', 'S').Replace('\u015f', 's').Replace('\u0131', 'i')
            .Replace('\u0130', 'I').Replace('\u011e', 'G').Replace('\u011f', 'g')
            .Replace('\u00c7', 'C').Replace('\u00e7', 'c').Replace('\u00d6', 'O')
            .Replace('\u00f6', 'o').Replace('\u00dc', 'U').Replace('\u00fc', 'u');
    }
}
