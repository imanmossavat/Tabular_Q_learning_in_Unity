using NUnit.Framework;

namespace GridLearn
{
    public class AgentTests
    {
        Config config = new Config();

        [Test]
        public void TerminalUpdate_MatchesHandCalculation()
        {
            Config cfg = new Config { alpha = 0.5f, gamma = 0.9f };
            QAgent agent = new QAgent(2, Moves.Count, cfg, 0);
            agent.Learn(0, Move.Right, 1.0f, 1, true);
            Assert.AreEqual(0.5f, agent.Q[0, (int)Move.Right], 0.0001);
        }

        [Test]
        public void NonTerminalUpdate_MatchesHandCalculation()
        {
            Config cfg = new Config { alpha = 0.5f, gamma = 0.9f };
            QAgent agent = new QAgent(2, Moves.Count, cfg, 0);
            agent.Q[1, 0] = 2f;
            agent.Q[1, 1] = 3f;
            agent.Q[1, 2] = 1f;
            agent.Q[1, 3] = 0f;

            agent.Learn(0, Move.Up, 0.5f, 1, false);

            float target = 0.5f + 0.9f * 3f;
            float expected = 0.5f * target;
            Assert.AreEqual(expected, agent.Q[0, (int)Move.Up], 0.0001);
        }

        [Test]
        public void TimeoutUpdate_BootstrapsFromNextState()
        {
            Config cfg = new Config { alpha = 0.5f, gamma = 0.9f };
            QAgent agent = new QAgent(2, Moves.Count, cfg, 0);
            agent.Q[1, 0] = 5f;

            agent.Learn(0, Move.Down, -0.04f, 1, false);

            float target = -0.04f + 0.9f * 5f;
            float expected = 0.5f * target;
            Assert.AreEqual(expected, agent.Q[0, (int)Move.Down], 0.0001);
        }

        [Test]
        public void Exploration_CoversEveryAction()
        {
            QAgent agent = new QAgent(1, Moves.Count, config, 7);
            bool[] chosen = new bool[Moves.Count];
            for (int i = 0; i < 400; i++)
                chosen[(int)agent.ChooseAction(0, 1f)] = true;

            for (int a = 0; a < Moves.Count; a++)
                Assert.IsTrue(chosen[a], $"Action {a} was never chosen.");
        }

        [Test]
        public void BestAction_IsDeterministicWithTies()
        {
            QAgent agent = new QAgent(1, Moves.Count, config, 99);
            for (int i = 0; i < 50; i++)
                Assert.AreEqual(Move.Up, agent.BestAction(0));
        }
    }
}
