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
            int tieCount = 1;

            for (int a = 1; a < ActionCount; a++)
            {
                if (Q[state, a] > best)
                {
                    best = Q[state, a];
                    tieCount = 1;
                }
                else if (Q[state, a] == best)
                {
                    tieCount++;
                }
            }

            int pick = tieCount == 1 ? 0 : Random.Next(tieCount);
            int seen = 0;
            for (int a = 0; a < ActionCount; a++)
            {
                if (Q[state, a] == best)
                {
                    if (seen == pick)
                        return (Move)a;
                    seen++;
                }
            }

            return Move.Up;
        }


        public Move BestValidAction(int state, Move[] validMoves)
        {
            if (validMoves == null || validMoves.Length == 0)
                return BestAction(state);

            float best = Q[state, (int)validMoves[0]];
            int tieCount = 1;

            for (int i = 1; i < validMoves.Length; i++)
            {
                float q = Q[state, (int)validMoves[i]];
                if (q > best)
                {
                    best = q;
                    tieCount = 1;
                }
                else if (q == best)
                {
                    tieCount++;
                }
            }

            int pick = tieCount == 1 ? 0 : Random.Next(tieCount);
            int seen = 0;
            for (int i = 0; i < validMoves.Length; i++)
            {
                if (Q[state, (int)validMoves[i]] == best)
                {
                    if (seen == pick)
                        return validMoves[i];
                    seen++;
                }
            }

            return validMoves[0];
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
