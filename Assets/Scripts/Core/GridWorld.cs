using System;

namespace GridLearn
{
    public class GridWorld
    {
        public Level Level { get; }
        public Config Config { get; }
        public int State { get; private set; }
        public int Steps { get; private set; }
        public bool IsTerminal { get; private set; }
        public int AgentX => State % Level.Width;
        public int AgentY => State / Level.Width;

        public GridWorld(Level level, Config config)
        {
            Level = level ?? throw new ArgumentNullException(nameof(level));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Reset();
        }

        public void Reset()
        {
            State = Level.StartState;
            Steps = 0;
            IsTerminal = false;
        }

        public (int nextState, float reward, bool isTerminal, bool isTimeout) Step(Move action)
        {
            if (IsTerminal)
                return (State, 0f, true, false);

            int x = AgentX;
            int y = AgentY;
            int nx = x;
            int ny = y;

            switch (action)
            {
                case Move.Up: ny--; break;
                case Move.Down: ny++; break;
                case Move.Left: nx--; break;
                case Move.Right: nx++; break;
            }

            if (nx < 0 || nx >= Level.Width || ny < 0 || ny >= Level.Height)
            {
                nx = x;
                ny = y;
            }
            else if (Level.Tiles[ny * Level.Width + nx] == Tile.Wall)
            {
                nx = x;
                ny = y;
            }

            int nextState = ny * Level.Width + nx;
            Tile tile = Level.Tiles[nextState];
            float reward = Config.stepReward;
            bool terminal = false;

            if (tile == Tile.Goal)
            {
                reward += Config.goalReward;
                terminal = true;
            }
            else if (tile == Tile.Hazard)
            {
                reward += Config.hazardReward;
                terminal = true;
            }
            else if (tile == Tile.Mud)
            {
                reward += Config.mudReward;
            }

            Steps++;
            bool timeout = !terminal && Steps >= Config.maxSteps;
            State = nextState;
            IsTerminal = terminal;

            return (nextState, reward, terminal, timeout);
        }
    }
}
