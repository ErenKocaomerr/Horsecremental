using UnityEngine;

namespace Horsecremental.Racing
{
    public static class RaceTrackBuilder
    {
        public const float FIRST_LANE_Y = -2.8f;
        public const float LANE_SPACING = 1.7f;

        public static void Build(Transform parent, Sprite square, float length, RaceObstacle[] obstacles, int lanes)
        {
            Rectangle(parent, square, "Grass", new Vector2(length * 0.5f, 0f),
                new Vector2(length + 80f, 30f), new Color(0.16f, 0.32f, 0.23f), -30);
            Rectangle(parent, square, "Track", new Vector2(length * 0.5f, 0f),
                new Vector2(length + 35f, lanes * LANE_SPACING + 1f), new Color(0.42f, 0.3f, 0.19f), -20);

            for (int lane = 0; lane < lanes; lane++)
            {
                float laneY = FIRST_LANE_Y + lane * LANE_SPACING;
                Rectangle(parent, square, "Lane edge", new Vector2(length * 0.5f, laneY - 0.25f),
                    new Vector2(length + 25f, 0.05f), new Color(0.73f, 0.59f, 0.36f), -10);
                for (int i = 0; i < obstacles.Length; i++)
                {
                    RaceObstacle obstacle = obstacles[i];
                    Rectangle(parent, square, "Hurdle " + (i + 1) + " lane " + (lane + 1),
                        new Vector2(obstacle.Distance, laneY + obstacle.Clearance * 0.5f),
                        new Vector2(obstacle.Width, obstacle.Clearance), new Color(0.95f, 0.4f, 0.14f), 10 - lane * 2);
                    Rectangle(parent, square, "Jump approach", new Vector2(obstacle.Distance - 3f, laneY - 0.08f),
                        new Vector2(0.14f, 0.35f), new Color(1f, 0.84f, 0.2f), -5);
                }
            }

            Rectangle(parent, square, "Start line", new Vector2(0f, 0.4f),
                new Vector2(0.15f, lanes * LANE_SPACING), Color.white, -5);
            for (int i = 0; i < lanes * 4; i++)
            {
                float y = FIRST_LANE_Y - 0.2f + i * LANE_SPACING / 4f;
                Rectangle(parent, square, "Finish checker", new Vector2(length, y),
                    new Vector2(0.4f, LANE_SPACING / 4f), i % 2 == 0 ? Color.white : Color.black, -5);
            }
        }

        public static SpriteRenderer Rectangle(Transform parent, Sprite square, string name,
            Vector2 position, Vector2 size, Color color, int order)
        {
            GameObject rectangle = new GameObject(name);
            rectangle.transform.SetParent(parent, false);
            rectangle.transform.localPosition = position;
            rectangle.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer renderer = rectangle.AddComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }
    }
}
