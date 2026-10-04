using NUnit.Framework;
using System;

namespace GridLearn
{
    public class LevelTests
    {
        [Test]
        public void ValidLevel_Parses()
        {
            Level level = Level.Parse("S.G");
            Assert.AreEqual(3, level.Width);
            Assert.AreEqual(1, level.Height);
            Assert.AreEqual(0, level.StartState);
            Assert.AreEqual(2, level.GoalState);
        }

        [Test]
        public void UnevenRows_Throws()
        {
            Assert.Throws<ArgumentException>(() => Level.Parse("S..\n.G"));
        }

        [Test]
        public void NoStart_Throws()
        {
            Assert.Throws<ArgumentException>(() => Level.Parse(".G"));
        }

        [Test]
        public void NoGoal_Throws()
        {
            Assert.Throws<ArgumentException>(() => Level.Parse("S."));
        }

        [Test]
        public void MultipleStarts_Throws()
        {
            Assert.Throws<ArgumentException>(() => Level.Parse("SS\n.G"));
        }

        [Test]
        public void MultipleGoals_Throws()
        {
            Assert.Throws<ArgumentException>(() => Level.Parse("SG\n.G"));
        }

        [Test]
        public void UnknownTile_Throws()
        {
            Assert.Throws<ArgumentException>(() => Level.Parse("X"));
        }

        [Test]
        public void NoPath_Throws()
        {
            Assert.Throws<ArgumentException>(() => Level.Parse("S#G"));
        }

        [Test]
        public void HazardDoesNotBlockPath()
        {
            Level level = Level.Parse("SH\n.G");
            Assert.AreEqual(2, level.Width);
            Assert.AreEqual(2, level.Height);
        }
    }
}
