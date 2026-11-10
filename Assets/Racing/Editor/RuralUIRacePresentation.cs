using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Horsecremental.Racing.Editor.RuralUIWidgets;

namespace Horsecremental.Racing.Editor
{
    internal static class RuralUIRacePresentation
    {
        private static readonly Vector2 CENTRE = new Vector2(.5f, .5f);
        internal static void Apply(GameplayWorld world, UIPresentationSettings icons, UIDetailPanel details)
        {
            Selection(world, icons, details);
            Race(world);
        }
        private static void Selection(GameplayWorld world, UIPresentationSettings icons, UIDetailPanel details)
        {
            Transform panel = world.transform.Find("Training Selection UI/Training Selection");
            Place(panel, CENTRE, new Vector2(0, 0), new Vector2(1240, 666)); Skin(panel, "Parchment");
            Text(panel, "Title", "RACE DAY", new Vector2(0, 272), new Vector2(800, 60), 46);
            Text(panel, "Hint", "Win a race to open the next course.", new Vector2(0, 221), new Vector2(1000, 36), 27, true);
            for (int i = 0; i < world.TrainingLadder.Count + 1; i++)
            {
                bool main = i == world.TrainingLadder.Count;
                Transform card = panel.Find(main ? "Main Race Card" : "Training Card " + (i + 1));
                Place(card, CENTRE, new Vector2((i - 1.5f) * 294, -15), new Vector2(278, 408)); Skin(card, "Inset");
                Image(card, "Course Illustration", RuralUIAssets.Sprite("Course" + Mathf.Min(i, 3)), new Vector2(0, 119), new Vector2(254, 127));
                Text(card, "Name", "", new Vector2(0, 28), new Vector2(254, 40), 34);
                Text(card, "Status", "", new Vector2(0, -7), new Vector2(250, 28), 22);
                Text(card, "Best", "", new Vector2(0, -37), new Vector2(252, 28), 23, true);
                TMP_Text reward = Text(card, "Rewards", "", new Vector2(19, -86), new Vector2(206, 58), 25);
                reward.alignment = TextAlignmentOptions.Left;
                if (main)
                {
                    ProgressionSettings settings = UnityEditor.AssetDatabase.LoadAssetAtPath<ProgressionSettings>(Milestone2SceneSetup.SETTINGS_PATH);
                    reward.text = settings.RewardForPlace(4) + "-" + settings.RewardForPlace(1) + " coins\n" +
                        settings.TokensForPlace(4) + "-" + settings.TokensForPlace(1) + " race tokens";
                }
                Image(card, "Coin Reward Icon", icons.Icon(14), new Vector2(-98, -72), new Vector2(24, 24));
                Image(card, "Token Reward Icon", icons.Icon(15), new Vector2(-98, -100), new Vector2(24, 24));
                Text(card, "Requirement", "", new Vector2(0, -130), new Vector2(252, 28), 22, true);
                Button start = card.GetComponentInChildren<Button>(true);
                Place(start.transform, CENTRE, new Vector2(0, -169), new Vector2(248, 48)); Button(start, main ? "GoldButton" : "SageButton");
                TMP_Text label = start.GetComponentInChildren<TMP_Text>(); label.fontSize = 27;
                Place(label.transform, CENTRE, Vector2.zero, new Vector2(230, 44));
                RaceSceneBaker.SetReference(card.GetComponent<TrainingTierCardUI>(), "_theme", RuralUIAssets.Theme);
                Inspect(start.gameObject, details, DetailKind.RaceCourse, main ? -1 : i, icons.Icon(5));
            }
            Transform close = panel.Find("Close Training");
            Place(close, CENTRE, new Vector2(0, -279), new Vector2(270, 48)); Button(close.GetComponent<Button>(), "Wood");
            TMP_Text back = close.GetComponentInChildren<TMP_Text>(); back.text = "BACK TO STABLE"; back.fontSize = 26;
            Place(back.transform, CENTRE, Vector2.zero, new Vector2(254, 44));
        }
        private static void Race(GameplayWorld world)
        {
            Transform race = world.transform.Find("Generated Race/Race UI");
            Transform result = race.Find("Race result"); Skin(result, "Parchment");
            foreach (TMP_Text t in result.GetComponentsInChildren<TMP_Text>(true)) t.color = RuralUIAssets.Theme.Ink;
            TMP_Text title = result.Find("Result").GetComponent<TMP_Text>(); title.fontSize = 40;
            Place(title.transform, CENTRE, new Vector2(0, 254), new Vector2(744, 68));
            TMP_Text reward = result.Find("Race Reward").GetComponent<TMP_Text>(); reward.fontSize = 28;
            reward.color = RuralUIAssets.Theme.Positive;
            result.Find("Column place").GetComponent<TMP_Text>().text = "PLACE";
            result.Find("Column name").GetComponent<TMP_Text>().text = "HORSE";
            result.Find("Column time").GetComponent<TMP_Text>().text = "TIME";
            foreach (Button b in result.GetComponentsInChildren<Button>(true)) Button(b, b.name == "Restart Race" ? "SageButton" : "Wood");
            Transform entry = race.Find("Race entry"); Skin(entry, "Parchment");
            foreach (TMP_Text t in entry.GetComponentsInChildren<TMP_Text>(true)) t.color = RuralUIAssets.Theme.Ink;
            Button(entry.Find("Start Race").GetComponent<Button>());
            // Backplates share the text/bar's visibility without adding another runtime controller.
            TMP_Text position = race.Find("Position").GetComponent<TMP_Text>(); position.fontSize = 34;
            position.alignment = TextAlignmentOptions.Center; position.color = new Color32(255, 239, 198, 255);
            Place(position.transform, new Vector2(1, 1), new Vector2(-138, -54), new Vector2(224, 56));
            Image plaque = Image(race, "Rural Position Plaque", null, Vector2.zero, new Vector2(238, 70));
            Skin(plaque.transform, "Wood"); Place(plaque.transform, new Vector2(1, 1), new Vector2(-138, -54), new Vector2(238, 70));
            plaque.transform.SetAsFirstSibling();
            Transform bar = race.Find("Boost bar"); Skin(bar, "Wood");
            Place(bar, new Vector2(0, 1), new Vector2(204, -154), new Vector2(344, 32));
            Transform fill = bar.Find("Fill"); RectTransform fillRect = (RectTransform)fill;
            fillRect.anchoredPosition = new Vector2(12, 0); fillRect.sizeDelta = new Vector2(0, 12);
            TMP_Text boost = race.Find("Boost").GetComponent<TMP_Text>(); boost.fontSize = 26;
            boost.color = new Color32(255, 241, 204, 255);
            Place(boost.transform, new Vector2(0, 1), new Vector2(204, -113), new Vector2(320, 34));
            TMP_Text controls = race.Find("Controls").GetComponent<TMP_Text>(); controls.fontSize = 27;
            controls.text = "SPACE  Boost     E  Jump";
            Place(controls.transform, new Vector2(.5f, 0), new Vector2(0, 130), new Vector2(440, 44));
            Image controlPlaque = Image(race, "Rural Controls Plaque", null, Vector2.zero, new Vector2(480, 62));
            Skin(controlPlaque.transform, "Wood"); Place(controlPlaque.transform, new Vector2(.5f, 0), new Vector2(0, 130), new Vector2(480, 62));
            controlPlaque.transform.SetSiblingIndex(0);
            RaceSceneBaker.SetArray(race.GetComponent<RaceUI>(), "_hudElements", new GameObject[] {
                position.gameObject, plaque.gameObject, boost.gameObject, bar.gameObject,
                controls.gameObject, controlPlaque.gameObject
            });
        }
    }
}
