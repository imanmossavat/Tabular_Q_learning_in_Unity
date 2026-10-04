using System.Collections.Generic;
using GridLearn;
using UnityEngine;

namespace GridLearn.Unity
{
    public class GridView : MonoBehaviour
    {
        [SerializeField] Theme theme;
        [SerializeField] float cellSize = 1f;

        Level level;
        SpriteRenderer agentRenderer;
        readonly List<GameObject> tiles = new List<GameObject>();
        readonly List<GameObject> arrows = new List<GameObject>();

        public void Build(Level level)
        {
            this.level = level;
            Clear();

            for (int y = 0; y < level.Height; y++)
            {
                for (int x = 0; x < level.Width; x++)
                {
                    Tile tile = level.Tiles[y * level.Width + x];
                    GameObject go = new GameObject(tile.ToString());
                    go.transform.SetParent(transform, false);
                    go.transform.localPosition = CellPosition(x, y);
                    SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = GetTileSprite();
                    sr.color = theme != null ? theme.ColorFor(tile) : Color.white;
                    sr.sortingOrder = 0;
                    tiles.Add(go);
                }
            }

            GameObject agentGo = new GameObject("Agent");
            agentGo.transform.SetParent(transform, false);
            agentRenderer = agentGo.AddComponent<SpriteRenderer>();
            agentRenderer.sprite = GetTileSprite();
            agentRenderer.color = theme != null ? theme.agentColor : Color.blue;
            agentRenderer.sortingOrder = 1;

            int startX = level.StartState % level.Width;
            int startY = level.StartState / level.Width;
            SetAgentPosition(startX, startY);
        }

        public void SetAgentPosition(int x, int y)
        {
            if (agentRenderer == null)
                return;
            agentRenderer.transform.localPosition = CellPosition(x, y);
        }

        public void ShowPolicy(QAgent agent)
        {
            ClearArrows();
            if (level == null || agent == null)
                return;

            for (int y = 0; y < level.Height; y++)
            {
                for (int x = 0; x < level.Width; x++)
                {
                    int state = y * level.Width + x;
                    Tile tile = level.Tiles[state];
                    if (tile == Tile.Wall || tile == Tile.Hazard || tile == Tile.Goal)
                        continue;

                    Move best = agent.BestAction(state);
                    GameObject arrow = new GameObject("Arrow");
                    arrow.transform.SetParent(transform, false);
                    arrow.transform.localPosition = CellPosition(x, y);
                    SpriteRenderer sr = arrow.AddComponent<SpriteRenderer>();
                    sr.sprite = GetArrowSprite();
                    sr.color = new Color(0f, 0f, 0f, 0.5f);
                    sr.sortingOrder = 2;
                    arrow.transform.localRotation = RotationFor(best);
                    arrows.Add(arrow);
                }
            }
        }

        public void ClearPolicy()
        {
            ClearArrows();
        }

        Vector3 CellPosition(int x, int y)
        {
            return new Vector3(x * cellSize, -y * cellSize, 0f);
        }

        Quaternion RotationFor(Move move)
        {
            return move switch
            {
                Move.Up => Quaternion.identity,
                Move.Down => Quaternion.Euler(0f, 0f, 180f),
                Move.Left => Quaternion.Euler(0f, 0f, 90f),
                Move.Right => Quaternion.Euler(0f, 0f, -90f),
                _ => Quaternion.identity,
            };
        }

        Sprite GetTileSprite()
        {
            if (theme != null && theme.tileSprite != null)
                return theme.tileSprite;

            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }

        Sprite GetArrowSprite()
        {
            if (theme != null && theme.arrowSprite != null)
                return theme.arrowSprite;

            const int size = 64;
            Texture2D tex = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.clear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inShaft = x >= 26 && x <= 38 && y >= 8 && y <= 40;
                    bool inHead = false;
                    if (y > 40 && y <= 56)
                    {
                        int halfWidth = (56 - y) * 14 / 16;
                        if (halfWidth < 0) halfWidth = 0;
                        inHead = x >= 32 - halfWidth && x <= 32 + halfWidth;
                    }
                    if (inShaft || inHead)
                        pixels[y * size + x] = Color.white;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        void Clear()
        {
            foreach (GameObject go in tiles)
                Destroy(go);
            tiles.Clear();

            if (agentRenderer != null)
                Destroy(agentRenderer.gameObject);
            agentRenderer = null;

            ClearArrows();
        }

        void ClearArrows()
        {
            foreach (GameObject go in arrows)
                Destroy(go);
            arrows.Clear();
        }
    }
}
