using System.Collections;
using System.Collections.Generic;
using GridLearn;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GridLearn.Unity
{
    public enum ControlMode { Play, Train, Watch }

    public class GameController : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] TextAsset levelText;
        [SerializeField] Config config;
        [SerializeField] GridView gridView;
        [SerializeField] int seed = 0;

        [Header("Timing")]
        [SerializeField] float stepInterval = 0.15f;
        [SerializeField] int fastEpisodes = 100;

        [Header("HUD")]
        [SerializeField] Text episodeText;
        [SerializeField] Text epsilonText;
        [SerializeField] Text rewardText;
        [SerializeField] Text successText;
        [SerializeField] Text speedText;

        [Header("Controls")]
        [SerializeField] Button trainButton;
        [SerializeField] Button watchButton;
        [SerializeField] Button playButton;
        [SerializeField] Button resetButton;
        [SerializeField] Button fastButton;

        Level level;
        GridWorld world;
        QAgent agent;
        readonly List<EpisodeRecord> records = new List<EpisodeRecord>();

        ControlMode mode = ControlMode.Play;
        Coroutine runningRoutine;
        int episodeCount;
        float lastReward;

        public int EpisodeCount => episodeCount;
        public int FastEpisodes => fastEpisodes;
        public IReadOnlyList<EpisodeRecord> Records => records;
        public GridWorld World => world;
        public QAgent Agent => agent;

        void Start()
        {
            if (levelText == null)
                throw new System.InvalidOperationException("Level TextAsset is not assigned.");
            if (config == null)
                config = new Config();

            level = Level.Parse(levelText.text);
            world = new GridWorld(level, config);
            int states = level.Width * level.Height;
            agent = new QAgent(states, 4, config, seed);

            gridView.Build(level);
            WireButtons();
            UpdateHUD();
        }

        void Update()
        {
            if (mode == ControlMode.Play)
                HandlePlayInput();
        }

        void OnDestroy()
        {
            StopRoutine();
        }

        void WireButtons()
        {
            trainButton.onClick.AddListener(StartTrain);
            watchButton.onClick.AddListener(StartWatch);
            playButton.onClick.AddListener(StartPlay);
            resetButton.onClick.AddListener(ResetAll);
            fastButton.onClick.AddListener(FastTrain);
        }

        public void StartTrain()
        {
            SetMode(ControlMode.Train);
            runningRoutine = StartCoroutine(TrainLoop());
        }

        public void StartWatch()
        {
            SetMode(ControlMode.Watch);
            runningRoutine = StartCoroutine(WatchLoop());
        }

        public void StartPlay()
        {
            SetMode(ControlMode.Play);
        }

        public void ResetAll()
        {
            StopRoutine();
            world.Reset();
            int states = level.Width * level.Height;
            agent = new QAgent(states, 4, config, seed);
            records.Clear();
            episodeCount = 0;
            lastReward = 0f;
            gridView.ClearPolicy();
            gridView.SetAgentPosition(world.AgentX, world.AgentY);
            UpdateHUD();
        }

        public void FastTrain()
        {
            StopRoutine();
            SetMode(ControlMode.Train);
            for (int i = 0; i < fastEpisodes; i++)
                RunEpisode();
            world.Reset();
            gridView.ShowPolicy(agent);
            gridView.SetAgentPosition(world.AgentX, world.AgentY);
            if (records.Count > 0)
                lastReward = records[records.Count - 1].totalReward;
            UpdateHUD();
        }

        EpisodeRecord RunEpisode()
        {
            world.Reset();
            float totalReward = 0f;
            int steps = 0;

            while (true)
            {
                float epsilon = agent.Epsilon(episodeCount + 1);
                Move action = agent.ChooseAction(world.State, epsilon);
                int state = world.State;
                var (nextState, reward, terminal, timeout) = world.Step(action);
                agent.Learn(state, action, reward, nextState, terminal);
                totalReward += reward;
                steps++;
                if (terminal || timeout)
                    break;
            }

            episodeCount++;
            EpisodeRecord record = new EpisodeRecord
            {
                episode = episodeCount,
                totalReward = totalReward,
                steps = steps,
                reachedGoal = world.State == level.GoalState,
                epsilon = agent.Epsilon(episodeCount)
            };
            records.Add(record);
            return record;
        }

        IEnumerator TrainLoop()
        {
            while (mode == ControlMode.Train)
            {
                world.Reset();
                float totalReward = 0f;
                int steps = 0;

                while (mode == ControlMode.Train)
                {
                    float epsilon = agent.Epsilon(episodeCount + 1);
                    Move action = agent.ChooseAction(world.State, epsilon);
                    int state = world.State;
                    var (nextState, reward, terminal, timeout) = world.Step(action);
                    agent.Learn(state, action, reward, nextState, terminal);
                    totalReward += reward;
                    steps++;
                    lastReward = reward;
                    gridView.SetAgentPosition(world.AgentX, world.AgentY);
                    UpdateHUD();
                    if (terminal || timeout)
                        break;
                    yield return new WaitForSeconds(stepInterval);
                }

                if (mode != ControlMode.Train)
                    yield break;

                episodeCount++;
                EpisodeRecord record = new EpisodeRecord
                {
                    episode = episodeCount,
                    totalReward = totalReward,
                    steps = steps,
                    reachedGoal = world.State == level.GoalState,
                    epsilon = agent.Epsilon(episodeCount)
                };
                records.Add(record);
                lastReward = totalReward;
                UpdateHUD();
                yield return new WaitForSeconds(stepInterval);
            }
        }

        IEnumerator WatchLoop()
        {
            while (mode == ControlMode.Watch)
            {
                world.Reset();
                while (mode == ControlMode.Watch && !world.IsTerminal)
                {
                    Move action = agent.BestAction(world.State);
                    var (_, reward, terminal, timeout) = world.Step(action);
                    lastReward = reward;
                    gridView.SetAgentPosition(world.AgentX, world.AgentY);
                    UpdateHUD();
                    if (terminal || timeout)
                        break;
                    yield return new WaitForSeconds(stepInterval);
                }
                yield return new WaitForSeconds(stepInterval * 2f);
            }
        }

        void HandlePlayInput()
        {
            if (world == null || world.IsTerminal)
                return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            Move? move = null;
            if (keyboard.upArrowKey.wasPressedThisFrame) move = Move.Up;
            else if (keyboard.downArrowKey.wasPressedThisFrame) move = Move.Down;
            else if (keyboard.leftArrowKey.wasPressedThisFrame) move = Move.Left;
            else if (keyboard.rightArrowKey.wasPressedThisFrame) move = Move.Right;

            if (!move.HasValue)
                return;

            var (_, reward, _, _) = world.Step(move.Value);
            lastReward = reward;
            gridView.SetAgentPosition(world.AgentX, world.AgentY);
            UpdateHUD();
        }

        void SetMode(ControlMode newMode)
        {
            StopRoutine();
            mode = newMode;
            UpdateHUD();
        }

        void StopRoutine()
        {
            if (runningRoutine != null)
            {
                StopCoroutine(runningRoutine);
                runningRoutine = null;
            }
        }

        void UpdateHUD()
        {
            if (episodeText != null)
                episodeText.text = $"Episode: {episodeCount}";
            if (epsilonText != null)
            {
                int episodeForEpsilon = Mathf.Max(1, episodeCount + 1);
                epsilonText.text = $"Epsilon: {agent.Epsilon(episodeForEpsilon):F3}";
            }
            if (rewardText != null)
                rewardText.text = $"Last reward: {lastReward:F2}";
            if (successText != null)
                successText.text = $"Success: {SuccessRate():P0}";
            if (speedText != null)
                speedText.text = $"Mode: {mode} ({stepInterval:F2}s/step)";
        }

        float SuccessRate()
        {
            const int window = 50;
            int count = Mathf.Min(window, records.Count);
            if (count == 0)
                return 0f;

            int goals = 0;
            for (int i = records.Count - count; i < records.Count; i++)
            {
                if (records[i].reachedGoal)
                    goals++;
            }
            return (float)goals / count;
        }
    }
}
