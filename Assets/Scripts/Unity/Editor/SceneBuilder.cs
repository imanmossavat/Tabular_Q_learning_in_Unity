using System.Collections.Generic;
using System.IO;
using System.Linq;
using GridLearn;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GridLearn.Unity.Editor
{
    public static class SceneBuilder
    {
        const string ScenePath = "Assets/Scenes/GridLearn.unity";
        const string LevelPath = "Assets/Levels/level01.txt";
        const string ThemePath = "Assets/Themes/DefaultTheme.asset";

        [MenuItem("GridLearn/Build Scene")]
        public static void BuildScene()
        {
            EnsureDirectories();
            Theme theme = CreateTheme();
            TextAsset levelAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(LevelPath);
            if (levelAsset == null)
            {
                Debug.LogError($"Level asset not found at {LevelPath}");
                return;
            }

            Level level = Level.Parse(levelAsset.text);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera camera = CreateCamera(level);
            GameObject gridGo = CreateGrid(theme);
            GameObject controllerGo = CreateController(levelAsset, gridGo.GetComponent<GridView>());
            CreateHUD(controllerGo.GetComponent<GameController>());

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);

            Debug.Log($"GridLearn scene saved to {ScenePath}");
        }

        static void EnsureDirectories()
        {
            if (!Directory.Exists("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            if (!Directory.Exists("Assets/Themes"))
                AssetDatabase.CreateFolder("Assets", "Themes");
        }

        static Theme CreateTheme()
        {
            Theme existing = AssetDatabase.LoadAssetAtPath<Theme>(ThemePath);
            if (existing != null)
                return existing;

            Theme theme = ScriptableObject.CreateInstance<Theme>();
            AssetDatabase.CreateAsset(theme, ThemePath);
            AssetDatabase.SaveAssets();
            return theme;
        }

        static Camera CreateCamera(Level level)
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            Camera camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.backgroundColor = new Color(0.15f, 0.15f, 0.15f);

            float centerX = (level.Width - 1) * 0.5f;
            float centerY = -(level.Height - 1) * 0.5f;
            go.transform.position = new Vector3(centerX, centerY, -10f);
            camera.orthographicSize = Mathf.Max(level.Width, level.Height) * 0.5f + 1f;
            return camera;
        }

        static GameObject CreateGrid(Theme theme)
        {
            GameObject go = new GameObject("Grid");
            GridView grid = go.AddComponent<GridView>();
            SerializedObject so = new SerializedObject(grid);
            so.FindProperty("theme").objectReferenceValue = theme;
            so.FindProperty("cellSize").floatValue = 1f;
            so.ApplyModifiedProperties();
            return go;
        }

        static GameObject CreateController(TextAsset levelAsset, GridView gridView)
        {
            GameObject go = new GameObject("GameController");
            GameController controller = go.AddComponent<GameController>();
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("levelText").objectReferenceValue = levelAsset;
            so.FindProperty("gridView").objectReferenceValue = gridView;
            so.FindProperty("seed").intValue = 0;
            so.FindProperty("stepInterval").floatValue = 0.15f;
            so.FindProperty("fastEpisodes").intValue = 100;
            so.ApplyModifiedProperties();
            return go;
        }

        static void CreateHUD(GameController controller)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Font.CreateDynamicFontFromOSFont("Arial", 16);

            GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            GameObject panel = new GameObject("Panel", typeof(Image));
            panel.transform.SetParent(canvasGo.transform, false);
            RectTransform panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0f, 1f);
            panelRt.anchorMax = new Vector2(1f, 1f);
            panelRt.pivot = new Vector2(0.5f, 1f);
            panelRt.sizeDelta = new Vector2(0f, 120f);
            panel.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);

            Text episodeText = CreateText(panel.transform, "EpisodeText", "Episode: 0", new Vector2(-400f, -20f), new Vector2(180f, 30f), font);
            Text epsilonText = CreateText(panel.transform, "EpsilonText", "Epsilon: 1.000", new Vector2(-200f, -20f), new Vector2(180f, 30f), font);
            Text rewardText = CreateText(panel.transform, "RewardText", "Last reward: 0.00", new Vector2(0f, -20f), new Vector2(180f, 30f), font);
            Text successText = CreateText(panel.transform, "SuccessText", "Success: 0%", new Vector2(200f, -20f), new Vector2(180f, 30f), font);
            Text speedText = CreateText(panel.transform, "SpeedText", "Mode: Play", new Vector2(400f, -20f), new Vector2(180f, 30f), font);

            Button trainButton = CreateButton(panel.transform, "Train", new Vector2(-240f, -80f), new Vector2(100f, 36f), font);
            Button watchButton = CreateButton(panel.transform, "Watch", new Vector2(-120f, -80f), new Vector2(100f, 36f), font);
            Button playButton = CreateButton(panel.transform, "Play", new Vector2(0f, -80f), new Vector2(100f, 36f), font);
            Button resetButton = CreateButton(panel.transform, "Reset", new Vector2(120f, -80f), new Vector2(100f, 36f), font);
            Button fastButton = CreateButton(panel.transform, "Fast", new Vector2(240f, -80f), new Vector2(100f, 36f), font);

            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("episodeText").objectReferenceValue = episodeText;
            so.FindProperty("epsilonText").objectReferenceValue = epsilonText;
            so.FindProperty("rewardText").objectReferenceValue = rewardText;
            so.FindProperty("successText").objectReferenceValue = successText;
            so.FindProperty("speedText").objectReferenceValue = speedText;
            so.FindProperty("trainButton").objectReferenceValue = trainButton;
            so.FindProperty("watchButton").objectReferenceValue = watchButton;
            so.FindProperty("playButton").objectReferenceValue = playButton;
            so.FindProperty("resetButton").objectReferenceValue = resetButton;
            so.FindProperty("fastButton").objectReferenceValue = fastButton;
            so.ApplyModifiedProperties();
        }

        static Text CreateText(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size, Font font)
        {
            GameObject go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = size;
            Text text = go.GetComponent<Text>();
            text.text = label;
            text.font = font;
            text.fontSize = 16;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleCenter;
            return text;
        }

        static Button CreateButton(Transform parent, string label, Vector2 anchoredPosition, Vector2 size, Font font)
        {
            GameObject go = new GameObject(label + "Button", typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = size;
            Image image = go.GetComponent<Image>();
            image.color = new Color(0.85f, 0.85f, 0.85f);
            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;

            Text text = CreateText(go.transform, "Text", label, Vector2.zero, size, font);
            RectTransform textRt = text.rectTransform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.anchoredPosition = Vector2.zero;
            textRt.sizeDelta = Vector2.zero;

            return button;
        }

        static void AddToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            for (int i = scenes.Count - 1; i >= 0; i--)
            {
                if (!File.Exists(scenes[i].path))
                    scenes.RemoveAt(i);
            }

            if (!scenes.Any(s => s.path == scenePath))
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
