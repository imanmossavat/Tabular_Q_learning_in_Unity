using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GridLearn;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

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

        [Header("Extras")]
        [SerializeField] Button policyButton;
        [SerializeField] Button saveButton;
        [SerializeField] Button loadButton;
        [SerializeField] Button csvButton;

        bool showingPolicy;

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
            agent = new QAgent(states, Moves.Count, config, seed);

            gridView.Build(level);
            WireButtons();
            UpdateHUD();
            EnsureEventSystem();
            ConfigureHud();
            EnsureExtraButtons();
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
            agent = new QAgent(states, Moves.Count, config, seed);
            records.Clear();
            episodeCount = 0;
            lastReward = 0f;
            showingPolicy = false;
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
            showingPolicy = true;
            UpdatePolicyButtonText();
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
        void EnsureEventSystem()
        {
            if (EventSystem.current == null)
            {
                GameObject go = new GameObject("EventSystem");
                go.AddComponent<EventSystem>();
                go.AddComponent<InputSystemUIInputModule>();
            }
        }

        void ConfigureHud()
        {
            CanvasScaler scaler = FindAnyObjectByType<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0f;
            }

            RectTransform panel = episodeText != null ? episodeText.transform.parent as RectTransform : null;
            if (panel != null)
                panel.sizeDelta = new Vector2(0f, 150f);

            Text[] labels = { episodeText, epsilonText, rewardText, successText, speedText };
            foreach (Text label in labels)
            {
                if (label != null)
                    label.fontSize = 20;
            }

            Button[] buttons = { trainButton, watchButton, playButton, resetButton, fastButton };
            foreach (Button button in buttons)
            {
                if (button == null)
                    continue;

                RectTransform rt = button.GetComponent<RectTransform>();
                if (rt != null)
                    rt.sizeDelta = new Vector2(110f, 44f);

                Text label = button.GetComponentInChildren<Text>();
                if (label != null)
                    label.fontSize = 20;
            }
        }

        void EnsureExtraButtons()
        {
            if (policyButton != null && saveButton != null && loadButton != null && csvButton != null)
            {
                WireExtraButtons();
                return;
            }

            Transform panel = episodeText != null ? episodeText.transform.parent : null;
            if (panel == null)
                return;

            RectTransform panelRt = panel as RectTransform;
            if (panelRt != null)
                panelRt.sizeDelta = new Vector2(0f, 190f);

            Button template = fastButton ?? trainButton;
            if (template == null)
                return;

            policyButton = Instantiate(template, panel);
            SetupExtraButton(policyButton, "PolicyButton", new Vector2(360f, -90f), "Show Policy");

            saveButton = Instantiate(template, panel);
            SetupExtraButton(saveButton, "SaveButton", new Vector2(-120f, -135f), "Save");

            loadButton = Instantiate(template, panel);
            SetupExtraButton(loadButton, "LoadButton", new Vector2(0f, -135f), "Load");

            csvButton = Instantiate(template, panel);
            SetupExtraButton(csvButton, "CsvButton", new Vector2(120f, -135f), "CSV");

            WireExtraButtons();
            UpdatePolicyButtonText();
        }

        void SetupExtraButton(Button button, string name, Vector2 position, string label)
        {
            button.name = name;
            RectTransform rt = button.GetComponent<RectTransform>();
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(110f, 44f);
            SetButtonLabel(button, label);
            button.onClick.RemoveAllListeners();
        }

        void WireExtraButtons()
        {
            policyButton.onClick.AddListener(TogglePolicy);
            saveButton.onClick.AddListener(SaveQTable);
            loadButton.onClick.AddListener(LoadQTable);
            csvButton.onClick.AddListener(ExportCsv);
        }

        void TogglePolicy()
        {
            showingPolicy = !showingPolicy;
            if (showingPolicy)
                gridView.ShowPolicy(agent);
            else
                gridView.ClearPolicy();
            UpdatePolicyButtonText();
        }

        void UpdatePolicyButtonText()
        {
            SetButtonLabel(policyButton, showingPolicy ? "Hide Policy" : "Show Policy");
        }

        void SetButtonLabel(Button button, string label)
        {
            if (button == null)
                return;
            Text text = button.GetComponentInChildren<Text>();
            if (text != null)
                text.text = label;
        }

        void SaveQTable()
        {
            float[] values = new float[agent.StateCount * agent.ActionCount];
            for (int s = 0; s < agent.StateCount; s++)
                for (int a = 0; a < agent.ActionCount; a++)
                    values[s * agent.ActionCount + a] = agent.Q[s, a];

            QTableData data = new QTableData
            {
                states = agent.StateCount,
                actions = agent.ActionCount,
                values = values,
                episode = episodeCount
            };

            string path = Path.Combine(Application.persistentDataPath, "GridLearnQTable.json");
            File.WriteAllText(path, JsonUtility.ToJson(data));
            Debug.Log($"Saved Q-table to {path}");
        }

        void LoadQTable()
        {
            string path = Path.Combine(Application.persistentDataPath, "GridLearnQTable.json");
            if (!File.Exists(path))
            {
                Debug.LogWarning($"No saved Q-table found at {path}");
                return;
            }

            QTableData data = JsonUtility.FromJson<QTableData>(File.ReadAllText(path));
            if (data == null || data.states != agent.StateCount || data.actions != agent.ActionCount || data.values == null || data.values.Length != agent.StateCount * agent.ActionCount)
            {
                Debug.LogError("Saved Q-table does not match the current level.");
                return;
            }

            StopRoutine();
            for (int s = 0; s < agent.StateCount; s++)
                for (int a = 0; a < agent.ActionCount; a++)
                    agent.Q[s, a] = data.values[s * agent.ActionCount + a];

            episodeCount = data.episode;
            world.Reset();
            int startX = level.StartState % level.Width;
            int startY = level.StartState / level.Width;
            gridView.SetAgentPosition(startX, startY);
            if (showingPolicy)
                gridView.ShowPolicy(agent);
            UpdateHUD();
            Debug.Log($"Loaded Q-table from {path}");
        }

        void ExportCsv()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Episode,Steps,TotalReward,ReachedGoal,Epsilon");
            foreach (EpisodeRecord r in records)
            {
                sb.AppendLine($"{r.episode},{r.steps},{r.totalReward:F4},{(r.reachedGoal ? 1 : 0)},{r.epsilon:F4}");
            }

            string path = Path.Combine(Application.persistentDataPath, "GridLearnEpisodes.csv");
            File.WriteAllText(path, sb.ToString());
            Debug.Log($"Exported episode data to {path}");
        }

        [Serializable]
        public class QTableData
        {
            public int states;
            public int actions;
            public float[] values;
            public int episode;
        }
    }
}
