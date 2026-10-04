using GridLearn;
using UnityEngine;

namespace GridLearn.Unity
{
    [CreateAssetMenu(fileName = "GridTheme", menuName = "GridLearn/Theme")]
    public class Theme : ScriptableObject
    {
        [Header("Sprites (optional)")]
        public Sprite tileSprite;
        public Sprite arrowSprite;

        [Header("Colors")]
        public Color emptyColor = new Color(0.9f, 0.9f, 0.9f);
        public Color wallColor = new Color(0.35f, 0.35f, 0.35f);
        public Color startColor = new Color(0.5f, 1f, 0.5f);
        public Color goalColor = new Color(1f, 0.85f, 0.2f);
        public Color hazardColor = new Color(1f, 0.35f, 0.35f);
        public Color mudColor = new Color(0.55f, 0.4f, 0.25f);
        public Color agentColor = new Color(0.25f, 0.5f, 1f);

        public Color ColorFor(Tile tile)
        {
            return tile switch
            {
                Tile.Wall => wallColor,
                Tile.Start => startColor,
                Tile.Goal => goalColor,
                Tile.Hazard => hazardColor,
                Tile.Mud => mudColor,
                _ => emptyColor,
            };
        }
    }
}
