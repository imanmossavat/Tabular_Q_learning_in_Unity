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
        [SerializeField] Button csvButton;
        [SerializeField] Button helpButton;

        [Header("Help")]
        [SerializeField] GameObject helpPanel;

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
            EnsureHelpPanel();
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
            trainButton.onClick.AddListener(() => { DeselectUi(); StartTrain(); });
            watchButton.onClick.AddListener(() => { DeselectUi(); StartWatch(); });
            playButton.onClick.AddListener(() => { DeselectUi(); StartPlay(); });
            resetButton.onClick.AddListener(() => { DeselectUi(); ResetAll(); });
            fastButton.onClick.AddListener(() => { DeselectUi(); FastTrain(); });
        }

        public void StartTrain()
        {
            SetMode(ControlMode.Train);
            runningRoutine = StartCoroutine(TrainOneEpisode());
        }

        public void StartWatch()
        {
            SetMode(ControlMode.Watch);
            runningRoutine = StartCoroutine(WatchOneEpisode());
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
            gridView.ShowPolicy(agent, world);
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

        IEnumerator TrainOneEpisode()
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

            runningRoutine = null;
            SetMode(ControlMode.Play);
        }

        IEnumerator WatchOneEpisode()
        {
            float totalReward = 0f;

            while (mode == ControlMode.Watch && !world.IsTerminal)
            {
                Move action = agent.BestValidAction(world.State, world.GetValidMoves(world.State));
                var (_, reward, terminal, timeout) = world.Step(action);
                totalReward += reward;
                lastReward = reward;
                gridView.SetAgentPosition(world.AgentX, world.AgentY);
                UpdateHUD();
                if (terminal || timeout)
                    break;
                yield return new WaitForSeconds(stepInterval);
            }

            lastReward = totalReward;
            UpdateHUD();

            runningRoutine = null;
            SetMode(ControlMode.Play);
        }

        void HandlePlayInput()
        {
            if (world == null)
                return;

            if (world.IsTerminal)
            {
                ResetToStart();
                return;
            }

            Move? move = null;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.upArrowKey.wasPressedThisFrame) move = Move.Up;
                else if (keyboard.downArrowKey.wasPressedThisFrame) move = Move.Down;
                else if (keyboard.leftArrowKey.wasPressedThisFrame) move = Move.Left;
                else if (keyboard.rightArrowKey.wasPressedThisFrame) move = Move.Right;
            }

            if (!move.HasValue)
                return;

            var (_, reward, _, _) = world.Step(move.Value);
            lastReward = reward;
            gridView.SetAgentPosition(world.AgentX, world.AgentY);
            UpdateHUD();
        }

        void ResetToStart()
        {
            world.Reset();
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

        void DeselectUi()
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
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
            if (policyButton != null && csvButton != null && helpButton != null)
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

            if (policyButton == null)
            {
                policyButton = Instantiate(template, panel);
                SetupExtraButton(policyButton, "PolicyButton", new Vector2(-120f, -135f), "Show Policy");
            }

            if (csvButton == null)
            {
                csvButton = Instantiate(template, panel);
                SetupExtraButton(csvButton, "CsvButton", new Vector2(0f, -135f), "CSV");
            }

            if (helpButton == null)
            {
                helpButton = Instantiate(template, panel);
                SetupExtraButton(helpButton, "HelpButton", new Vector2(120f, -135f), "Help");
            }

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
            policyButton.onClick.AddListener(() => { DeselectUi(); TogglePolicy(); });
            csvButton.onClick.AddListener(() => { DeselectUi(); ExportCsv(); });
            helpButton.onClick.AddListener(() => { DeselectUi(); ToggleHelp(); });
        }

        void TogglePolicy()
        {
            showingPolicy = !showingPolicy;
            if (showingPolicy)
                gridView.ShowPolicy(agent, world);
            else
                gridView.ClearPolicy();
            UpdatePolicyButtonText();
        }

        void UpdatePolicyButtonText()
        {
            SetButtonLabel(policyButton, showingPolicy ? "Hide Policy" : "Show Policy");
        }

        void EnsureHelpPanel()
        {
            if (helpPanel != null)
                return;

            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
                return;

            helpPanel = new GameObject("HelpPanel");
            helpPanel.transform.SetParent(canvas.transform, false);

            RectTransform rt = helpPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(800f, 520f);

            Image bg = helpPanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            GameObject textGo = new GameObject("HelpText");
            textGo.transform.SetParent(helpPanel.transform, false);
            RectTransform textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(20f, 20f);
            textRt.offsetMax = new Vector2(-20f, -20f);

            Text text = textGo.AddComponent<Text>();
            text.font = episodeText != null && episodeText.font != null
                ? episodeText.font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 22;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.text =
                "GridLearn controls\n\n" +
                "Train – run one training episode using epsilon-greedy exploration.\n" +
                "Watch – run one episode using the current best policy (no exploration).\n" +
                "Play  – move the agent with the arrow keys.\n" +
                "Reset – clear the Q-table and episode history.\n" +
                "Fast  – train " + fastEpisodes + " episodes instantly.\n" +
                "Show/Hide Policy – toggle arrows showing the best action for each cell.\n" +
                "CSV   – export episode results to GridLearnEpisodes.csv.";

            helpPanel.SetActive(false);
        }

        void ToggleHelp()
        {
            if (helpPanel == null)
                return;
            helpPanel.SetActive(!helpPanel.activeSelf);
        }

        void SetButtonLabel(Button button, string label)
        {
            if (button == null)
                return;
            Text text = button.GetComponentInChildren<Text>();
            if (text != null)
                text.text = label;
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
    }
}
