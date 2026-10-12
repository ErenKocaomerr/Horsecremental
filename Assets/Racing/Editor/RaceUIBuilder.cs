using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing.Editor
{
    internal static class RaceUIBuilder
    {
        internal static RaceUI Build(Transform parent, int racers)
        {
            GameObject root = new GameObject("Race UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(RaceUI));
            root.transform.SetParent(parent, false);
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            RaceUI ui = root.GetComponent<RaceUI>();
            Text position = Label(root.transform, "Position", new Vector2(220, -38), new Vector2(420, 48), 27, TextAnchor.MiddleLeft, true);
            position.text = "Sıra: 1 / " + racers;
            RaceSceneBaker.SetReference(ui, "_position", position);
            Text boost = Label(root.transform, "Boost", new Vector2(220, -84), new Vector2(420, 40), 23, TextAnchor.MiddleLeft, true);
            boost.text = "SPACE - TEKRAR BASARAK BOOST";
            RaceSceneBaker.SetReference(ui, "_boost", boost);
            Image bar = Panel(root.transform, "Boost bar", new Vector2(170, -118), new Vector2(320, 12), new Color(0.08f, 0.1f, 0.1f), true);
            Image fill = Panel(bar.transform, "Fill", Vector2.zero, new Vector2(0, 12), new Color(1f, 0.78f, 0.2f));
            fill.rectTransform.anchorMin = fill.rectTransform.anchorMax = new Vector2(0, 0.5f);
            fill.rectTransform.pivot = new Vector2(0, 0.5f);
            RaceSceneBaker.SetReference(ui, "_boostFill", fill);
            Text controls = Label(root.transform, "Controls", new Vector2(0, -490), new Vector2(1700, 64), 26);
            controls.text = "SPACE: tekrar basarak boost   |   E: zıpla   |   Sen: alt şeritteki altın renkli at\nTuruncu engeller yavaşlatır. Sarı işaretin yakınında zıpla.";
            RaceSceneBaker.SetReference(ui, "_countdown", Label(root.transform, "Countdown", new Vector2(0, 80), new Vector2(400, 150), 80));
            RaceSceneBaker.SetReference(ui, "_feedback", Label(root.transform, "Feedback", new Vector2(0, 205), new Vector2(1000, 50), 28));

            Image entry = Panel(root.transform, "Race entry", Vector2.zero, new Vector2(560, 280), new Color(0.05f, 0.1f, 0.08f, 0.96f));
            Label(entry.transform, "Title", new Vector2(0, 70), new Vector2(530, 60), 32).text = "AT YARIŞI";
            Label(entry.transform, "Description", new Vector2(0, 15), new Vector2(520, 60), 23).text = "Dört at. Beş engel. Tek yarış.";
            RaceSceneBaker.SetReference(ui, "_entryPanel", entry.gameObject);
            RaceSceneBaker.SetReference(ui, "_start", Button(entry.transform, "Start Race", "Yarışı Başlat", new Vector2(0, -75)));

            Image result = Panel(root.transform, "Race result", Vector2.zero, new Vector2(800, 640), new Color(0.05f, 0.1f, 0.08f, 0.97f));
            Text title = Label(result.transform, "Result", new Vector2(0, 260), new Vector2(760, 60), 30);
            title.text = "YARIŞ TAMAMLANDI";
            RaceSceneBaker.SetReference(ui, "_result", title);
            Label(result.transform, "Column place", new Vector2(-260, 103), new Vector2(90, 35), 22).text = "Sıra";
            Label(result.transform, "Column name", new Vector2(-75, 103), new Vector2(250, 35), 22, TextAnchor.MiddleLeft).text = "At";
            Label(result.transform, "Column time", new Vector2(200, 103), new Vector2(220, 35), 22).text = "Bitiş Süresi";
            RaceResultRow[] rows = new RaceResultRow[racers];
            for (int i = 0; i < racers; i++)
            {
                Image highlight = Panel(result.transform, "Result Row " + i, new Vector2(0, 58 - i * 44), new Vector2(620, 40), new Color(1f, 1f, 1f, 0.05f));
                RaceResultRow row = highlight.gameObject.AddComponent<RaceResultRow>();
                RaceSceneBaker.SetReference(row, "_highlight", highlight);
                RaceSceneBaker.SetReference(row, "_place", Label(highlight.transform, "Place", new Vector2(-260, 0), new Vector2(90, 38), 24));
                RaceSceneBaker.SetReference(row, "_name", Label(highlight.transform, "Horse Name", new Vector2(-75, 0), new Vector2(250, 38), 24, TextAnchor.MiddleLeft));
                RaceSceneBaker.SetReference(row, "_time", Label(highlight.transform, "Finish Time", new Vector2(200, 0), new Vector2(220, 38), 24));
                rows[i] = row;
            }
            RaceSceneBaker.SetArray(ui, "_resultRows", rows);
            RaceSceneBaker.SetReference(ui, "_resultPanel", result.gameObject);
            Text reward = Label(result.transform, "Race Reward", new Vector2(0, 187), new Vector2(760, 80), 28);
            RaceSceneBaker.SetReference(ui, "_reward", reward);
            Button returnButton = Button(result.transform, "Return to Stable", "AHIRA DÖN", new Vector2(0, -170));
            returnButton.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 64);
            RaceSceneBaker.SetReference(ui, "_returnToStable", returnButton);
            RaceSceneBaker.SetReference(ui, "_restart", Button(result.transform, "Restart Race", "Tekrar Yarış (Debug)", new Vector2(0, -250)));
            result.gameObject.SetActive(false);
            return ui;
        }

        internal static Text Label(Transform parent, string name, Vector2 position, Vector2 size, int fontSize,
            TextAnchor alignment = TextAnchor.MiddleCenter, bool topLeft = false)
        {
            RectTransform rect = Rect(parent, name, position, size, topLeft);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        internal static Image Panel(Transform parent, string name, Vector2 position, Vector2 size, Color color, bool topLeft = false)
        {
            Image image = Rect(parent, name, position, size, topLeft).gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        internal static Button Button(Transform parent, string name, string title, Vector2 position)
        {
            Image image = Panel(parent, name, position, new Vector2(270, 58), new Color(0.18f, 0.43f, 0.25f));
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            Label(image.transform, "Label", Vector2.zero, new Vector2(260, 54), 26).text = title;
            return button;
        }

        private static RectTransform Rect(Transform parent, string name, Vector2 position, Vector2 size, bool topLeft)
        {
            RectTransform rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = topLeft ? new Vector2(0, 1) : new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }
    }
}
