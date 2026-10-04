using System;

namespace GridLearn
{
    public class QAgent
    {
        public float[,] Q { get; }
        public Config Config { get; }
        public Random Random { get; }
        public int StateCount { get; }
        public int ActionCount { get; }

        public QAgent(int stateCount, int actionCount, Config config, int seed)
        {
            StateCount = stateCount;
            ActionCount = actionCount;
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Q = new float[stateCount, actionCount];
            Random = new Random(seed);
        }

        public float Epsilon(int episode)
        {
            if (episode <= 1) return Config.epsilonStart;
            if (episode >= Config.epsilonDecayEpisodes) return Config.epsilonEnd;
            float t = (episode - 1) / (float)(Config.epsilonDecayEpisodes - 1);
            return Config.epsilonStart + (Config.epsilonEnd - Config.epsilonStart) * t;
        }

        public Move ChooseAction(int state, float epsilon)
        {
            if (Random.NextDouble() < epsilon)
                return (Move)Random.Next(ActionCount);
            return BestAction(state);
        }

        public Move BestAction(int state)
        {
            float best = Q[state, 0];
            int bestIndex = 0;
            for (int a = 1; a < ActionCount; a++)
            {
                if (Q[state, a] > best)
                {
                    best = Q[state, a];
                    bestIndex = a;
                }
            }
            return (Move)bestIndex;
        }

        public void Learn(int state, Move action, float reward, int nextState, bool terminal)
        {
            int a = (int)action;
            float target = terminal
                ? reward
                : reward + Config.gamma * MaxQ(nextState);
            Q[state, a] += Config.alpha * (target - Q[state, a]);
        }

        float MaxQ(int state)
        {
            float max = Q[state, 0];
            for (int a = 1; a < ActionCount; a++)
            {
                if (Q[state, a] > max)
                    max = Q[state, a];
            }
            return max;
        }
    }
}
