using System;

namespace GridLearn
{
    [Serializable]
    public class Config
    {
        public float stepReward = -0.04f;
        public float goalReward = 1.0f;
        public float hazardReward = -1.0f;
        public float mudReward = -0.30f;
        public int maxSteps = 200;
        public float alpha = 0.3f;
        public float gamma = 0.95f;
        public float epsilonStart = 1.0f;
        public float epsilonEnd = 0.05f;
        public int epsilonDecayEpisodes = 400;
    }
}
