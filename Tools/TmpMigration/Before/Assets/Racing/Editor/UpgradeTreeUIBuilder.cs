using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Horsecremental.Racing.Editor
{
    internal static class UpgradeTreeUIBuilder
    {
        internal static void Build(Transform parent, GameplayWorld world, ProgressionSettings settings)
        {
            UIPresentationSettings presentation = UIIconAssets.Ensure();
            Transform old = parent.Find("Upgrade Tree UI");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            GameObject root = new GameObject("Upgrade Tree UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(parent, false);
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            root.GetComponent<Canvas>().sortingOrder = 10;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            CanvasGroup group = root.AddComponent<CanvasGroup>(); group.alpha = 0;
            group.interactable = group.blocksRaycasts = false;
            AreaPanel area = root.AddComponent<AreaPanel>();
            RaceSceneBaker.SetReference(area, "_world", world);
            SetEnum(area, "_area", (int)ManagementArea.UpgradeTree);
            UpgradeTreeUI ui = root.AddComponent<UpgradeTreeUI>();
            RaceSceneBaker.SetReference(ui, "_world", world); RaceSceneBaker.SetReference(ui, "_settings", settings);
            Image panel = RaceUIBuilder.Panel(root.transform, "Tree Panel", new Vector2(0, -5), new Vector2(1760, 800), new Color(0.025f, 0.06f, 0.085f, 0.98f));
            RaceUIBuilder.Label(panel.transform, "Heading", new Vector2(0, 345), new Vector2(1650, 50), 32).text = string.Empty;
            Text summary = RaceUIBuilder.Label(panel.transform, "Tree Resources", new Vector2(0, 295), new Vector2(1650, 65), 21);
            RaceSceneBaker.SetReference(ui, "_summary", summary);
            UpgradeTreeRules rules = settings.UpgradeTree.CreateRules();
            Image[] lines = new Image[UpgradeTreeRules.NODE_COUNT];
            for (int i = 0; i < lines.Length; i++)
            {
                UpgradeNode parentNode = rules.Rule((UpgradeNode)i).Prerequisite;
                if (parentNode == UpgradeNode.None) continue;
                Vector2 start = presentation.TreePosition(parentNode); Vector2 end = presentation.TreePosition((UpgradeNode)i);
                Vector2 direction = end - start;
                Vector2 unit = direction.normalized;
                float inset = 40f / Mathf.Max(Mathf.Abs(unit.x), Mathf.Abs(unit.y));
                lines[i] = RaceUIBuilder.Panel(panel.transform, "Connection " + (UpgradeNode)i, (start + end) * 0.5f,
                    new Vector2(Mathf.Max(0, direction.magnitude - 2 * inset), 4), new Color(0.3f, 0.34f, 0.4f));
                lines[i].rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                lines[i].raycastTarget = false;
            }
            for (int i = 0; i < lines.Length; i++)
            {
                Button button = RaceUIBuilder.Button(panel.transform, "Unlock " + (UpgradeNode)i, "", presentation.TreePosition((UpgradeNode)i));
                ColorBlock colors = button.colors;
                colors.disabledColor = new Color(0.72f, 0.72f, 0.72f, 1);
                button.colors = colors;
                button.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
                Text label = button.GetComponentInChildren<Text>();
                label.fontSize = 20; label.rectTransform.sizeDelta = new Vector2(320, 108);
                label.text = string.Empty;
                UpgradeTreeNodeUI node = button.gameObject.AddComponent<UpgradeTreeNodeUI>();
                RaceSceneBaker.SetReference(node, "_treeUI", ui); RaceSceneBaker.SetReference(node, "_settings", settings);
                RaceSceneBaker.SetReference(node, "_label", label); RaceSceneBaker.SetReference(node, "_connection", lines[i]);
                SetEnum(node, "_node", i + 1); // None is the first serialized enum entry.
            }
            RaceUIBuilder.Label(panel.transform, "Tree Help", new Vector2(0, -270), new Vector2(1640, 75), 23).text = string.Empty;
            Text feedback = RaceUIBuilder.Label(panel.transform, "Tree Feedback", new Vector2(0, -345), new Vector2(1630, 65), 25);
            feedback.color = new Color(0.9f, 0.8f, 1);
            feedback.text = string.Empty;
            RaceSceneBaker.SetReference(ui, "_feedback", feedback);
        }
        private static void SetEnum(Object target, string field, int index)
        {
            SerializedObject data = new SerializedObject(target);
            data.FindProperty(field).enumValueIndex = index;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
