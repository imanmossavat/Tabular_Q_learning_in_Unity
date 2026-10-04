using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace GridLearn
{
    public class LearningTests
    {
        const int EpisodeLimit = 600;
        const int PathLimit = 1000;

        static Level LoadLevel()
        {
            return Level.Parse(File.ReadAllText("Assets/Levels/level01.txt"));
        }

        static int GreedyPathLength(Level level, Config config, QAgent agent)
        {
            GridWorld world = new GridWorld(level, config);
            world.Reset();
            HashSet<int> visited = new HashSet<int>();
            int steps = 0;

            while (steps < PathLimit)
            {
                if (world.State == level.GoalState)
                    return steps;
                if (!visited.Add(world.State))
                    return -1;

                Move action = agent.BestAction(world.State);
                world.Step(action);

                if (world.IsTerminal && world.State != level.GoalState)
                    return -1;

                steps++;
            }

            return -1;
        }

        [Test]
        public void Level01_LearnsOptimalPathWithinEpisodeLimit()
        {
            Level level = LoadLevel();
            Config config = new Config();
            int stateCount = level.Width * level.Height;

            for (int seed = 0; seed < 10; seed++)
            {
                QAgent agent = new QAgent(stateCount, 4, config, seed);
                GridWorld world = new GridWorld(level, config);
                Trainer trainer = new Trainer(world, agent);

                bool solved = false;
                for (int ep = 0; ep < EpisodeLimit; ep++)
                {
                    trainer.RunEpisode();
                    if (GreedyPathLength(level, config, agent) == 18)
                    {
                        solved = true;
                        break;
                    }
                }

                Assert.IsTrue(solved, $"Seed {seed} did not find the 18-step path within {EpisodeLimit} episodes.");
            }
        }

        [Test]
        public void SameSeed_GivesSameResult()
        {
            Level level = LoadLevel();
            Config config = new Config();
            int stateCount = level.Width * level.Height;
            int seed = 42;
            int episodes = 100;

            float[,] first = TrainAndCopy(level, config, stateCount, seed, episodes);
            float[,] second = TrainAndCopy(level, config, stateCount, seed, episodes);

            for (int s = 0; s < stateCount; s++)
            {
                for (int a = 0; a < 4; a++)
                {
                    Assert.AreEqual(first[s, a], second[s, a], 0.0001f,
                        $"Q-table differs at state {s}, action {a}.");
                }
            }
        }

        float[,] TrainAndCopy(Level level, Config config, int stateCount, int seed, int episodes)
        {
            QAgent agent = new QAgent(stateCount, 4, config, seed);
            GridWorld world = new GridWorld(level, config);
            Trainer trainer = new Trainer(world, agent);
            trainer.Run(episodes);

            float[,] copy = new float[stateCount, 4];
            for (int s = 0; s < stateCount; s++)
                for (int a = 0; a < 4; a++)
                    copy[s, a] = agent.Q[s, a];
            return copy;
        }
    }
}
