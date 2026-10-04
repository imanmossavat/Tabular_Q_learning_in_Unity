using System.Collections;
using GridLearn;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GridLearn.Unity
{
    public class SceneTests
    {
        [UnityTest]
        public IEnumerator Scene_LoadsAndFastTrains()
        {
            SceneManager.LoadScene("GridLearn", LoadSceneMode.Single);
            yield return null;

            GameController controller = Object.FindFirstObjectByType<GameController>();
            Assert.IsNotNull(controller, "GameController not found in scene.");

            GridView view = Object.FindFirstObjectByType<GridView>();
            Assert.IsNotNull(view, "GridView not found in scene.");

            int expected = controller.FastEpisodes;
            controller.FastTrain();
            yield return null;

            Assert.AreEqual(expected, controller.EpisodeCount, "FastTrain did not run the expected number of episodes.");
            Assert.Greater(controller.Records.Count, 0, "No episode records were produced.");
            Assert.Greater(controller.Records[controller.Records.Count - 1].episode, 0, "Last recorded episode number is invalid.");
        }
    }
}
