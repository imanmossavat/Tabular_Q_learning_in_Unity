using System;
using System.Collections.Generic;

namespace GridLearn
{
    public class Level
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int StartState { get; private set; } = -1;
        public int GoalState { get; private set; } = -1;
        public Tile[] Tiles { get; private set; }

        public static Level Parse(string text)
        {
            string[] rows = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (rows.Length == 0)
                throw new ArgumentException("Level is empty.", nameof(text));

            int width = rows[0].Length;
            for (int i = 1; i < rows.Length; i++)
            {
                if (rows[i].Length != width)
                    throw new ArgumentException($"Row {i} has {rows[i].Length} columns, expected {width}.", nameof(text));
            }

            Level level = new Level
            {
                Width = width,
                Height = rows.Length,
                Tiles = new Tile[width * rows.Length]
            };

            for (int y = 0; y < rows.Length; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Tile tile = rows[y][x] switch
                    {
                        '.' => Tile.Empty,
                        '#' => Tile.Wall,
                        'S' => Tile.Start,
                        'G' => Tile.Goal,
                        'H' => Tile.Hazard,
                        'm' => Tile.Mud,
                        char c => throw new ArgumentException($"Unknown tile '{c}' at ({x},{y}).", nameof(text))
                    };

                    int state = y * width + x;
                    level.Tiles[state] = tile;

                    if (tile == Tile.Start)
                    {
                        if (level.StartState != -1)
                            throw new ArgumentException("Level has more than one start.", nameof(text));
                        level.StartState = state;
                    }

                    if (tile == Tile.Goal)
                    {
                        if (level.GoalState != -1)
                            throw new ArgumentException("Level has more than one goal.", nameof(text));
                        level.GoalState = state;
                    }
                }
            }

            if (level.StartState == -1)
                throw new ArgumentException("Level has no start.", nameof(text));
            if (level.GoalState == -1)
                throw new ArgumentException("Level has no goal.", nameof(text));
            if (!HasPath(level))
                throw new ArgumentException("No path from start to goal.", nameof(text));

            return level;
        }

        static bool HasPath(Level level)
        {
            int total = level.Width * level.Height;
            bool[] visited = new bool[total];
            Queue<int> queue = new Queue<int>();
            visited[level.StartState] = true;
            queue.Enqueue(level.StartState);

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            while (queue.Count > 0)
            {
                int state = queue.Dequeue();
                if (state == level.GoalState)
                    return true;

                int x = state % level.Width;
                int y = state / level.Width;

                for (int i = 0; i < Moves.Count; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];
                    if (nx < 0 || nx >= level.Width || ny < 0 || ny >= level.Height)
                        continue;

                    int next = ny * level.Width + nx;
                    if (visited[next] || level.Tiles[next] == Tile.Wall)
                        continue;

                    visited[next] = true;
                    queue.Enqueue(next);
                }
            }

            return false;
        }
    }
}
