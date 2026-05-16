using AjouFestival.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AjouFestival.Games.BalanceWalk
{
    public sealed class BalanceWalkGameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BalancePlayerController player;
        [SerializeField] private BalanceUI ui;

        [Header("Flow")]
        [SerializeField] private float countdownDuration = 3f;

        [Header("Scoring")]
        [SerializeField] private float scorePerSecond = 10f;

        [Header("Start Guide UI (Scene Editable)")]
        [SerializeField] private GameObject startGuidePanel;
        [SerializeField] private Text startGuideText;
        [SerializeField] private Button startConfirmButton;
        [SerializeField] private bool useRuntimeGuideFallback = true;
        [SerializeField] private string startGuidePanelName = "StartGuidePanel";
        [SerializeField] private string startGuideMessage = "\uADE0\uD615\uC744 \uC7A1\uACE0 \uC624\uB798 \uBC84\uD2F0\uBA74 \uC2B9\uB9AC!\nA/D \uB610\uB294 \uBC29\uD5A5\uD0A4\uB85C \uAE30\uC6B8\uAE30\uB97C \uBC84\uD2F0\uC138\uC694.";
        [SerializeField] private string startConfirmButtonText = "\uD655\uC778";

        [Header("Text")]
        [SerializeField] private string countdownStartMessage = "\uC2DC\uC791!";
        [SerializeField] private string resultMessageFormat = "\uADE0\uD615\uC744 \uC783\uC5C8\uC2B5\uB2C8\uB2E4. \uCD5C\uC885 \uAC01\uB3C4 {0:0}\uB3C4";
        [SerializeField] private string resultScoreTextFormat = "{0:0.0}\uCD08 \uBC84\uD300";

        public bool IsGameOver { get; private set; }
        public bool HasStarted { get; private set; }
        public float CountdownRemaining { get; private set; }
        public float ElapsedTime { get; private set; }
        public float DistanceMeters { get; private set; }
        public float Difficulty { get; private set; } = 1f;

        private static readonly string[] StartGuideTextNames = { "GuideText", "StartGuideText", "Guide" };
        private static readonly string[] StartButtonNames = { "StartButton", "ConfirmButton", "Start" };

        private float startX;
        private bool isCountdownStarted;

        private void Awake()
        {
            if (player == null) player = FindFirstObjectByType<BalancePlayerController>();
            if (ui == null) ui = FindFirstObjectByType<BalanceUI>();

            ResolveStartGuideReferences();
            ApplyStartGuideText();
        }

        private void Start()
        {
            GameSessionManager.Ensure().StartGame(GameType.BalanceWalk, SceneLoader.BalanceWalkScene);
            CountdownRemaining = countdownDuration;
            HasStarted = false;
            startX = player != null ? player.transform.position.x : 0f;

            if (player != null) player.Initialize(this);
            if (ui != null)
            {
                ui.SetBestTime(ScoreRecordManager.GetBestScore(GameType.BalanceWalk), scorePerSecond);
                ui.SetSurvivalTime(0f);
                ui.HideCountdown();
            }

            SetGameRuntimeEnabled(false);

            if (startGuidePanel != null)
            {
                startGuidePanel.SetActive(true);
            }

            if (startConfirmButton != null)
            {
                startConfirmButton.onClick.RemoveAllListeners();
                startConfirmButton.onClick.AddListener(OnStartConfirmClicked);
            }
            else
            {
                StartCoroutine(StartCountdownRoutine());
            }
        }

        private void Update()
        {
            if (IsGameOver || !HasStarted)
            {
                return;
            }

            ElapsedTime += Time.deltaTime;
            DistanceMeters = player != null ? Mathf.Max(0f, player.transform.position.x - startX) : 0f;
            Difficulty = 1f + DistanceMeters * 0.015f;
            ui?.SetSurvivalTime(ElapsedTime);
        }

        public void GameOver(float angle)
        {
            if (IsGameOver)
            {
                return;
            }

            IsGameOver = true;
            int score = Mathf.FloorToInt(ElapsedTime * scorePerSecond);
            string resultMessage = string.Format(resultMessageFormat, angle);
            string resultScoreText = string.Format(resultScoreTextFormat, ElapsedTime);
            GameSessionManager.Ensure().SetResult(score, resultMessage, resultScoreText);
            SceneLoader.LoadResult();
        }

        private void OnStartConfirmClicked()
        {
            if (isCountdownStarted)
            {
                return;
            }

            if (startGuidePanel != null)
            {
                startGuidePanel.SetActive(false);
            }

            isCountdownStarted = true;
            if (startConfirmButton != null)
            {
                startConfirmButton.interactable = false;
            }

            StartCoroutine(StartCountdownRoutine());
        }

        private IEnumerator StartCountdownRoutine()
        {
            CountdownRemaining = countdownDuration;
            int lastShown = -1;

            while (CountdownRemaining > 0f)
            {
                int current = Mathf.CeilToInt(CountdownRemaining);
                if (current != lastShown)
                {
                    ui?.SetCountdown(current.ToString());
                    lastShown = current;
                }

                CountdownRemaining -= Time.deltaTime;
                yield return null;
            }

            ui?.SetCountdown(countdownStartMessage, true);
            StartGame();
        }

        private void StartGame()
        {
            HasStarted = true;
            SetGameRuntimeEnabled(true);

            if (player != null)
            {
                player.Initialize(this);
            }
        }

        private void SetGameRuntimeEnabled(bool enabled)
        {
            BalanceObstacleSpawner obstacleSpawner = FindFirstObjectByType<BalanceObstacleSpawner>();
            if (obstacleSpawner != null)
            {
                obstacleSpawner.enabled = enabled;
            }
        }

        private void ResolveStartGuideReferences()
        {
            if (startGuidePanel == null)
            {
                if (!string.IsNullOrWhiteSpace(startGuidePanelName))
                {
                    startGuidePanel = GameObject.Find(startGuidePanelName);
                }

                if (startGuidePanel == null && useRuntimeGuideFallback)
                {
                    CreateRuntimeStartGuide();
                }
            }

            if (startGuidePanel != null)
            {
                if (startGuideText == null) startGuideText = FindTextInPanel(startGuidePanel, StartGuideTextNames);
                if (startConfirmButton == null) startConfirmButton = FindButtonInPanel(startGuidePanel, StartButtonNames);
            }
        }

        private void ApplyStartGuideText()
        {
            if (startGuideText != null)
            {
                startGuideText.text = startGuideMessage;
            }

            if (startConfirmButton != null)
            {
                Text buttonLabel = startConfirmButton.GetComponentInChildren<Text>(true);
                if (buttonLabel != null)
                {
                    buttonLabel.text = startConfirmButtonText;
                }
            }
        }

        private static Text FindTextInPanel(GameObject root, string[] preferredNames)
        {
            Text[] texts = root.GetComponentsInChildren<Text>(true);
            if (texts == null || texts.Length == 0)
            {
                return null;
            }

            for (int nameIndex = 0; nameIndex < preferredNames.Length; nameIndex++)
            {
                for (int i = 0; i < texts.Length; i++)
                {
                    if (texts[i] != null && string.Equals(texts[i].name, preferredNames[nameIndex], System.StringComparison.OrdinalIgnoreCase))
                    {
                        return texts[i];
                    }
                }
            }

            return texts[0];
        }

        private static Button FindButtonInPanel(GameObject root, string[] preferredNames)
        {
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            if (buttons == null || buttons.Length == 0)
            {
                return null;
            }

            for (int nameIndex = 0; nameIndex < preferredNames.Length; nameIndex++)
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i] != null && string.Equals(buttons[i].name, preferredNames[nameIndex], System.StringComparison.OrdinalIgnoreCase))
                    {
                        return buttons[i];
                    }
                }
            }

            return buttons[0];
        }

        private void CreateRuntimeStartGuide()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                return;
            }

            GameObject panel = new("StartGuidePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            startGuidePanel = panel;
            panel.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.8f);

            Font uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject guideObject = new("GuideText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            guideObject.transform.SetParent(panel.transform, false);
            RectTransform guideRect = guideObject.GetComponent<RectTransform>();
            guideRect.anchorMin = new Vector2(0.5f, 0.5f);
            guideRect.anchorMax = new Vector2(0.5f, 0.5f);
            guideRect.pivot = new Vector2(0.5f, 0.5f);
            guideRect.anchoredPosition = new Vector2(0f, 50f);
            guideRect.sizeDelta = new Vector2(760f, 220f);

            startGuideText = guideObject.GetComponent<Text>();
            if (uiFont != null) startGuideText.font = uiFont;
            startGuideText.fontSize = 42;
            startGuideText.alignment = TextAnchor.MiddleCenter;
            startGuideText.color = Color.white;
            startGuideText.text = startGuideMessage;

            GameObject buttonObject = new("ConfirmButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(panel.transform, false);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = new Vector2(0f, -120f);
            buttonRect.sizeDelta = new Vector2(220f, 64f);

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.12f, 0.6f, 0.2f, 0.98f);

            GameObject buttonTextObject = new("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            buttonTextObject.transform.SetParent(buttonObject.transform, false);
            RectTransform buttonTextRect = buttonTextObject.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;

            Text buttonText = buttonTextObject.GetComponent<Text>();
            if (uiFont != null) buttonText.font = uiFont;
            buttonText.fontSize = 28;
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;
            buttonText.text = startConfirmButtonText;

            startConfirmButton = buttonObject.GetComponent<Button>();
            startConfirmButton.targetGraphic = buttonImage;
        }
    }
}
