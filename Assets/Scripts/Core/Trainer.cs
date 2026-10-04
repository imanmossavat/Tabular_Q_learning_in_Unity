using System.Collections.Generic;

namespace GridLearn
{
    public struct EpisodeRecord
    {
        public int episode;
        public float totalReward;
        public int steps;
        public bool reachedGoal;
        public float epsilon;
    }

    public class Trainer
    {
        public GridWorld World { get; }
        public QAgent Agent { get; }
        public List<EpisodeRecord> Records { get; } = new List<EpisodeRecord>();
        public int Episode { get; private set; }

        public Trainer(GridWorld world, QAgent agent)
        {
            World = world;
            Agent = agent;
        }

        public EpisodeRecord RunEpisode()
        {
            World.Reset();
            float totalReward = 0f;
            int steps = 0;

            while (true)
            {
                float epsilon = Agent.Epsilon(Episode + 1);
                Move action = Agent.ChooseAction(World.State, epsilon);
                int state = World.State;
                var (nextState, reward, terminal, timeout) = World.Step(action);
                Agent.Learn(state, action, reward, nextState, terminal);
                totalReward += reward;
                steps++;
                if (terminal || timeout)
                    break;
            }

            Episode++;
            EpisodeRecord record = new EpisodeRecord
            {
                episode = Episode,
                totalReward = totalReward,
                steps = steps,
                reachedGoal = World.State == World.Level.GoalState,
                epsilon = Agent.Epsilon(Episode)
            };
            Records.Add(record);
            return record;
        }

        public void Run(int episodes)
        {
            for (int i = 0; i < episodes; i++)
                RunEpisode();
        }
    }
}
