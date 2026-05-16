using AjouFestival.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AjouFestival.Games.Soccer
{
    public sealed class SoccerGameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SoccerPlayerController player1;
        [SerializeField] private SoccerPlayerController player2;
        [SerializeField] private SoccerBallController ball;
        [SerializeField] private SoccerUI ui;

        [Header("Match")]
        [SerializeField] private float matchDuration = 60f;

        [Header("Start Guide UI (Scene Editable)")]
        [SerializeField] private GameObject startGuidePanel;
        [SerializeField] private Text startGuideText;
        [SerializeField] private Button startConfirmButton;
        [SerializeField] private bool useRuntimeGuideFallback = true;
        [SerializeField] private string startGuidePanelName = "StartGuidePanel";
        [SerializeField] private string startGuideMessage = "\uACF5\uC744 \uCC28\uC11C \uB354 \uB9CE\uC774 \uB123\uC73C\uBA74 \uC2B9\uB9AC!\n1\uB3001 \uB610\uB294 AI \uB300\uC804 \uBAA8\uB4DC\uB97C \uACE0\uB974\uACE0 \uACBD\uAE30\uB97C \uC2DC\uC791\uD558\uC138\uC694.";
        [SerializeField] private string startConfirmButtonText = "\uD655\uC778";
        [SerializeField] private float countdownDuration = 3f;
        [SerializeField] private string countdownStartMessage = "\uC2DC\uC791!";

        [Header("Arena")]
        [SerializeField] private Vector2 player1Spawn = new Vector2(-4.6f, -3.2f);
        [SerializeField] private Vector2 player2Spawn = new Vector2(4.6f, -3.2f);
        [SerializeField] private Vector2 ballSpawn = new Vector2(0f, -2.55f);
        [SerializeField] private float goalYPosition = -3.25f;
        [SerializeField] private Vector2 goalTriggerSize = new Vector2(0.9f, 2f);
        [SerializeField] private Vector2 cameraCenter = new Vector2(0f, -0.35f);
        [SerializeField] private float cameraSize = 5f;
        [SerializeField] private float sideWallOffsetFromGoal = 0.7f;
        [SerializeField] private float sideWallHeight = 8.2f;
        [SerializeField] private float topWallY = 4.2f;
        [SerializeField] private float bottomWallY = -4.2f;
        [SerializeField] private float horizontalWallThickness = 0.35f;
        [SerializeField] private float verticalWallThickness = 0.3f;

        public int Player1Score { get; private set; }
        public int Player2Score { get; private set; }
        public float TimeRemaining { get; private set; }
        public bool IsFinished { get; private set; }
        public bool IsMatchActive { get; private set; }
        public SoccerMatchMode CurrentMatchMode { get; private set; } = SoccerMatchMode.OneVsOne;
        public SoccerAIDifficulty CurrentAIDifficulty { get; private set; } = SoccerAIDifficulty.Medium;

        private static readonly string[] StartGuideTextNames = { "GuideText", "StartGuideText", "Guide" };
        private static readonly string[] StartButtonNames = { "StartButton", "ConfirmButton", "Start" };

        private Vector3 player1Start;
        private Vector3 player2Start;
        private Vector3 ballStart;
        private bool isCountdownStarted;

        private void Awake()
        {
            if (player1 == null || player2 == null)
            {
                SoccerPlayerController[] players = FindObjectsByType<SoccerPlayerController>(FindObjectsSortMode.None);
                foreach (SoccerPlayerController player in players)
                {
                    if (player.PlayerIndex == 1) player1 = player;
                    if (player.PlayerIndex == 2) player2 = player;
                }
            }

            if (ball == null) ball = FindFirstObjectByType<SoccerBallController>();
            if (ui == null) ui = FindFirstObjectByType<SoccerUI>();

            ResolveStartGuideReferences();
            ApplyStartGuideText();
        }

        private void Start()
        {
            GameSessionManager.Ensure().StartGame(GameType.Soccer, SceneLoader.SoccerScene);
            ConfigureArena();

            if (player1 != null) player1.Initialize(this, ball);
            if (player2 != null) player2.Initialize(this, ball);

            ResetMatchState();
            ui?.HideCountdown();
            ResolveMatchSelection();
            UpdateUI();
        }

        private void Update()
        {
            if (!IsMatchActive || IsFinished)
            {
                return;
            }

            TimeRemaining -= Time.deltaTime;
            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                FinishMatch();
            }

            UpdateUI();
        }

        public void AddGoal(int scoringPlayer)
        {
            if (!IsMatchActive || IsFinished)
            {
                return;
            }

            if (scoringPlayer == 1) Player1Score++;
            if (scoringPlayer == 2) Player2Score++;

            ResetPositions();
            UpdateUI();
        }

        private void ResolveMatchSelection()
        {
            GameSessionManager session = GameSessionManager.Ensure();
            if (session.HasSoccerMatchSelection)
            {
                ApplyMatchSelection(session.SoccerMatchMode, session.SoccerAIDifficulty);
                return;
            }

            ShowModeSelection();
        }

        private void ShowModeSelection()
        {
            IsMatchActive = false;
            isCountdownStarted = false;
            ResetMatchState();

            if (player1 != null) player1.SetHumanControl();
            if (player2 != null) player2.SetHumanControl();

            if (startGuidePanel != null)
            {
                startGuidePanel.SetActive(false);
            }

            ui?.HideCountdown();
            ui?.ShowModeSelection(ApplyMatchSelection);
            ui?.SetModeHint(null, null);
        }

        private void ApplyMatchSelection(SoccerMatchMode mode, SoccerAIDifficulty difficulty)
        {
            CurrentMatchMode = mode;
            CurrentAIDifficulty = mode == SoccerMatchMode.VersusAI ? difficulty : SoccerAIDifficulty.Medium;

            GameSessionManager.Ensure().SetSoccerMatchSelection(CurrentMatchMode, CurrentAIDifficulty);

            if (player1 != null) player1.SetHumanControl();
            if (player2 != null)
            {
                if (CurrentMatchMode == SoccerMatchMode.VersusAI)
                {
                    player2.SetAIControl(CurrentAIDifficulty);
                }
                else
                {
                    player2.SetHumanControl();
                }
            }

            ResetMatchState();
            IsMatchActive = false;
            isCountdownStarted = false;

            ui?.HideModeSelection();
            ui?.HideCountdown();
            ui?.SetModeHint(CurrentMatchMode, CurrentAIDifficulty);
            UpdateUI();

            ShowStartGuide();
        }

        private void ShowStartGuide()
        {
            ResolveStartGuideReferences();
            ApplyStartGuideText();

            if (startGuidePanel != null)
            {
                startGuidePanel.SetActive(true);
            }

            if (startConfirmButton != null)
            {
                startConfirmButton.onClick.RemoveAllListeners();
                startConfirmButton.onClick.AddListener(OnStartConfirmClicked);
                startConfirmButton.interactable = true;
            }
            else
            {
                StartCoroutine(StartCountdownRoutine());
            }
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
            float remaining = Mathf.Max(0f, countdownDuration);
            int lastShown = -1;

            while (remaining > 0f)
            {
                int current = Mathf.CeilToInt(remaining);
                if (current != lastShown)
                {
                    ui?.SetCountdown(current.ToString());
                    lastShown = current;
                }

                remaining -= Time.deltaTime;
                yield return null;
            }

            ui?.SetCountdown(countdownStartMessage, true);
            BeginMatch();
        }

        private void BeginMatch()
        {
            ResetMatchState();
            IsMatchActive = true;
            UpdateUI();
        }

        private void ResetMatchState()
        {
            Player1Score = 0;
            Player2Score = 0;
            TimeRemaining = matchDuration;
            IsFinished = false;
            ResetPositions();
        }

        private void ResetPositions()
        {
            if (player1 != null) player1.ResetPosition(player1Start);
            if (player2 != null) player2.ResetPosition(player2Start);
            if (ball != null) ball.ResetPosition(ballStart);
        }

        private void FinishMatch()
        {
            IsFinished = true;
            IsMatchActive = false;

            string player2Name = CurrentMatchMode == SoccerMatchMode.VersusAI
                ? $"AI {GetDifficultyLabel(CurrentAIDifficulty)}"
                : "Player 2";

            string result = Player1Score == Player2Score
                ? "Draw!"
                : Player1Score > Player2Score ? "Player 1 Wins!" : $"{player2Name} Wins!";

            int winnerScore = Mathf.Max(Player1Score, Player2Score);
            GameSessionManager.Ensure().SetResult(winnerScore, result, $"P1 {Player1Score} : {Player2Score} P2");
            SceneLoader.LoadResult();
        }

        private void UpdateUI()
        {
            if (ui != null)
            {
                ui.SetMatch(TimeRemaining, Player1Score, Player2Score);
            }
        }

        private void ConfigureArena()
        {
            player1Start = new Vector3(player1Spawn.x, player1Spawn.y, 0f);
            player2Start = new Vector3(player2Spawn.x, player2Spawn.y, 0f);
            ballStart = new Vector3(ballSpawn.x, ballSpawn.y, 0f);

            Transform leftGoal = ConfigureGoal("SoccerGoalLeft");
            Transform rightGoal = ConfigureGoal("SoccerGoalRight");
            ConfigureBoundaryWalls(leftGoal, rightGoal);

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = cameraSize;
                mainCamera.transform.position = new Vector3(cameraCenter.x, cameraCenter.y, mainCamera.transform.position.z);
            }
        }

        private Transform ConfigureGoal(string goalName)
        {
            GameObject goalObject = GameObject.Find(goalName);
            if (goalObject == null)
            {
                return null;
            }

            Vector3 goalPosition = goalObject.transform.position;
            goalPosition.y = goalYPosition;
            goalObject.transform.position = goalPosition;

            BoxCollider2D goalCollider = goalObject.GetComponent<BoxCollider2D>();
            if (goalCollider != null)
            {
                goalCollider.isTrigger = true;
                goalCollider.offset = Vector2.zero;
                goalCollider.size = goalTriggerSize;
            }

            return goalObject.transform;
        }

        private void ConfigureBoundaryWalls(Transform leftGoal, Transform rightGoal)
        {
            if (leftGoal == null || rightGoal == null)
            {
                return;
            }

            float leftWallX = leftGoal.position.x - sideWallOffsetFromGoal;
            float rightWallX = rightGoal.position.x + sideWallOffsetFromGoal;
            float wallCenterX = (leftWallX + rightWallX) * 0.5f;
            float wallWidth = Mathf.Abs(rightWallX - leftWallX) + verticalWallThickness;

            ConfigureWall("LeftBackWall", new Vector2(leftWallX, 0f), new Vector2(verticalWallThickness, sideWallHeight));
            ConfigureWall("RightBackWall", new Vector2(rightWallX, 0f), new Vector2(verticalWallThickness, sideWallHeight));
            ConfigureWall("TopWall", new Vector2(wallCenterX, topWallY), new Vector2(wallWidth, horizontalWallThickness));
            ConfigureWall("BottomWall", new Vector2(wallCenterX, bottomWallY), new Vector2(wallWidth, horizontalWallThickness));
        }

        private static void ConfigureWall(string wallName, Vector2 position, Vector2 size)
        {
            GameObject wallObject = GameObject.Find(wallName);
            if (wallObject == null)
            {
                return;
            }

            wallObject.transform.position = new Vector3(position.x, position.y, wallObject.transform.position.z);

            BoxCollider2D wallCollider = wallObject.GetComponent<BoxCollider2D>();
            if (wallCollider != null)
            {
                wallCollider.offset = Vector2.zero;
                wallCollider.size = size;
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
            guideRect.sizeDelta = new Vector2(780f, 220f);

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
            buttonRect.anchoredPosition = new Vector2(0f, -118f);
            buttonRect.sizeDelta = new Vector2(220f, 64f);

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(1f, 1f, 1f, 0.95f);

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
            buttonText.color = Color.black;
            buttonText.text = startConfirmButtonText;

            startConfirmButton = buttonObject.GetComponent<Button>();
            startConfirmButton.targetGraphic = buttonImage;
        }

        private static string GetDifficultyLabel(SoccerAIDifficulty difficulty)
        {
            return difficulty switch
            {
                SoccerAIDifficulty.Easy => "초급",
                SoccerAIDifficulty.Medium => "중급",
                SoccerAIDifficulty.Hard => "상급",
                _ => "중급"
            };
        }
    }
}
