using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing.Editor
{
    internal static class RuralUIWidgets
    {
        internal static void Place(Transform target, Vector2 anchor, Vector2 position, Vector2 size)
        {
            RectTransform r = (RectTransform)target;
            r.anchorMin = r.anchorMax = anchor; r.pivot = new Vector2(.5f, .5f);
            r.anchoredPosition = position; r.sizeDelta = size;
        }
        internal static void Skin(Transform target, string surface)
        {
            Image image = target.GetComponent<Image>(); image.material = null;
            image.sprite = RuralUIAssets.Sprite(surface); image.type = UnityEngine.UI.Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = .5f; image.color = Color.white;
        }
        internal static Image Image(Transform parent, string name, Sprite sprite, Vector2 pos, Vector2 size)
        {
            Transform old = parent.Find(name);
            Image image = old != null ? old.GetComponent<Image>() : RaceUIBuilder.Panel(parent, name, pos, size, Color.white);
            Place(image.transform, new Vector2(.5f, .5f), pos, size);
            image.sprite = sprite; image.color = Color.white; image.raycastTarget = false; return image;
        }
        internal static TMP_Text Text(Transform parent, string name, string value, Vector2 pos, Vector2 size, int font, bool muted = false)
        {
            Transform old = parent.Find(name);
            TMP_Text text = old != null ? old.GetComponent<TMP_Text>() : RaceUIBuilder.Label(parent, name, pos, size, font);
            Place(text.transform, new Vector2(.5f, .5f), pos, size);
            text.text = value; text.fontSize = font; text.color = muted ? RuralUIAssets.Theme.Muted : RuralUIAssets.Theme.Ink;
            text.textWrappingMode = TextWrappingModes.NoWrap; text.overflowMode = TextOverflowModes.Truncate;
            return text;
        }
        internal static void Button(Button button, string surface = "SageButton")
        {
            Skin(button.transform, surface);
            ColorBlock c = button.colors; c.normalColor = Color.white;
            c.highlightedColor = new Color(1.12f, 1.1f, 1.03f); c.selectedColor = c.highlightedColor;
            c.pressedColor = new Color(.82f, .84f, .76f); c.disabledColor = new Color(.94f, .92f, .86f);
            c.fadeDuration = .08f; button.colors = c;
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            text.color = surface == "GoldButton" ? RuralUIAssets.Theme.Ink : new Color32(255, 241, 200, 255);
            text.textWrappingMode = TextWrappingModes.NoWrap; text.overflowMode = TextOverflowModes.Truncate;
        }
        internal static void Inspect(GameObject target, UIDetailPanel panel, DetailKind kind, int id, Sprite icon)
        {
            UIDetailTarget detail = target.GetComponent<UIDetailTarget>();
            if (detail == null) detail = target.AddComponent<UIDetailTarget>();
            detail.enabled = true;
            RaceSceneBaker.SetReference(detail, "_panel", panel); RaceSceneBaker.SetReference(detail, "_icon", icon);
            UnityEditor.SerializedObject data = new UnityEditor.SerializedObject(detail);
            data.FindProperty("_kind").intValue = (int)kind; data.FindProperty("_id").intValue = id;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
