using NUnit.Framework;

namespace GridLearn
{
    public class WorldTests
    {
        Config config = new Config();

        [Test]
        public void WallBump_StaysAndCostsStep()
        {
            Level level = Level.Parse("S.#\n..G");
            GridWorld world = new GridWorld(level, config);
            world.Reset();

            var (s1, r1, t1, time1) = world.Step(Move.Right);
            Assert.AreEqual(1, s1);
            Assert.AreEqual(-0.04f, r1, 0.0001);
            Assert.IsFalse(t1);

            var (s2, r2, t2, time2) = world.Step(Move.Right);
            Assert.AreEqual(1, s2);
            Assert.AreEqual(-0.04f, r2, 0.0001);
            Assert.IsFalse(t2);
        }

        [Test]
        public void EdgeBump_StaysAndCostsStep()
        {
            Level level = Level.Parse("S..G");
            GridWorld world = new GridWorld(level, config);
            world.Reset();

            var (s, r, t, time) = world.Step(Move.Left);
            Assert.AreEqual(0, s);
            Assert.AreEqual(-0.04f, r, 0.0001);
            Assert.IsFalse(t);
        }

        [Test]
        public void Goal_EndsEpisodeWithAdditiveReward()
        {
            Level level = Level.Parse("SG");
            GridWorld world = new GridWorld(level, config);
            world.Reset();

            var (s, r, t, time) = world.Step(Move.Right);
            Assert.AreEqual(1, s);
            Assert.AreEqual(0.96f, r, 0.0001);
            Assert.IsTrue(t);
            Assert.IsFalse(time);
        }

        [Test]
        public void Hazard_EndsEpisodeWithAdditiveReward()
        {
            Level level = Level.Parse("SHG");
            GridWorld world = new GridWorld(level, config);
            world.Reset();

            var (s, r, t, time) = world.Step(Move.Right);
            Assert.AreEqual(1, s);
            Assert.AreEqual(-1.04f, r, 0.0001);
            Assert.IsTrue(t);
            Assert.IsFalse(time);
        }

        [Test]
        public void Mud_AddsExtraPenaltyAndContinues()
        {
            Level level = Level.Parse("SmG");
            GridWorld world = new GridWorld(level, config);
            world.Reset();

            var (s1, r1, t1, time1) = world.Step(Move.Right);
            Assert.AreEqual(1, s1);
            Assert.AreEqual(-0.34f, r1, 0.0001);
            Assert.IsFalse(t1);

            var (s2, r2, t2, time2) = world.Step(Move.Right);
            Assert.AreEqual(2, s2);
            Assert.AreEqual(0.96f, r2, 0.0001);
            Assert.IsTrue(t2);
        }

        [Test]
        public void Timeout_IsNotTerminal()
        {
            Config shortConfig = new Config { maxSteps = 2 };
            Level level = Level.Parse("S..G");
            GridWorld world = new GridWorld(level, shortConfig);
            world.Reset();

            world.Step(Move.Right);
            var (s, r, t, time) = world.Step(Move.Right);
            Assert.AreEqual(2, s);
            Assert.IsFalse(t);
            Assert.IsTrue(time);
        }

        [Test]
        public void GetValidMoves_ExcludesWallsAndEdges()
        {
            Level level = Level.Parse("S.#\n..G");
            GridWorld world = new GridWorld(level, config);

            Move[] valid = world.GetValidMoves(0);
            CollectionAssert.AreEquivalent(new[] { Move.Right, Move.Down }, valid);

            Move[] middle = world.GetValidMoves(1);
            CollectionAssert.AreEquivalent(new[] { Move.Left, Move.Down }, middle);
        }
    }
}
