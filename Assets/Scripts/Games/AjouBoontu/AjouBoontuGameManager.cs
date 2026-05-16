using AjouFestival.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AjouFestival.Games.AjouBoontu
{
    public sealed class AjouBoontuGameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ChitoRunnerController runner;
        [SerializeField] private RunnerUI ui;

        [Header("Scoring")]
        [SerializeField] private float scorePerSecond = 10f;
        [SerializeField] private int itemScore = 100;

        [Header("Pacing")]
        [SerializeField, Min(0f)] private float initialRunSpeed = 5.8f;
        [SerializeField, Min(0f)] private float speedIncreaseIntervalSeconds = 8f;
        [SerializeField, Min(0f)] private float speedIncreaseAmount = 0.5f;
        [SerializeField, Min(1f)] private float clearDistance = 120f;
        [SerializeField, Min(0f)] private float startDelaySeconds = 3f;
        [SerializeField] private string startGuideMessage = "3\uCD08 \uB4A4 \uC2DC\uC791! SPACE\uB85C \uC2DC\uC791\uD569\uB2C8\uB2E4.";
        [Header("Start Guide UI (Scene Editable)")]
        [SerializeField] private GameObject startGuidePanel;
        [SerializeField] private Text startGuideText;
        [SerializeField] private Text startCountdownText;
        [SerializeField] private Button startConfirmButton;
        [SerializeField] private bool useRuntimeGuideFallback = false;
        [SerializeField] private string startGuidePanelName = "StartGuidePanel";
        [Header("Clear Trophy (Scene Editable)")]
        [SerializeField] private GameObject clearTrophyObject;
        [SerializeField] private SpriteRenderer clearTrophySpriteRenderer;
        [SerializeField] private Graphic clearTrophyGraphic;
        [SerializeField] private Camera clearTrophyCamera;
        [SerializeField] private string clearTrophyObjectName = "\uC6B0\uC2B9\uD2B8\uB85C\uD53C_0";
        [SerializeField] private Vector2 clearTrophyCameraOffset = new Vector2(0f, 0.4f);
        [SerializeField, Min(0.01f)] private float clearTrophyFadeInSeconds = 0.45f;
        [SerializeField, Min(0f)] private float clearTrophyVisibleSeconds = 2.5f;
        [SerializeField, Min(0.01f)] private float clearTrophyFadeOutSeconds = 0.35f;

        private static readonly string[] StartGuideTextNames = { "GuideText", "StartGuideText", "Guide" };
        private static readonly string[] StartCountdownTextNames = { "CountdownText", "CountText", "TimerText" };
        private static readonly string[] StartButtonNames = { "StartButton", "ConfirmButton", "Start" };

        public bool IsGameOver { get; private set; }
        public bool IsGameRunning { get; private set; }
        public int Score { get; private set; }

        private bool isCountdownStarted;
        private float scoreFloat;
        private float elapsedTime;
        private float nextSpeedIncreaseTime;
        private float runStartX;
        private readonly List<Behaviour> hiddenStartGuideBehaviours = new();
        private Coroutine clearSequenceCoroutine;
        private bool clearTrophyInitiallyActive;
        private Vector3 clearTrophyOriginalPosition;

        private void Awake()
        {
            if (runner == null) runner = FindFirstObjectByType<ChitoRunnerController>();
            if (ui == null) ui = FindFirstObjectByType<RunnerUI>();

            if (startGuidePanel == null)
            {
                startGuidePanel = transform.Find(startGuidePanelName)?.gameObject;
                if (startGuidePanel == null && !string.IsNullOrWhiteSpace(startGuidePanelName))
                {
                    startGuidePanel = GameObject.Find(startGuidePanelName);
                }

                if (startGuidePanel == null)
                {
                    startGuidePanel = FindStartGuidePanelByScan();
                }
            }

            if (startGuidePanel != null)
            {
                if (startGuideText == null) startGuideText = FindTextInPanel(startGuidePanel, StartGuideTextNames);
                if (startCountdownText == null) startCountdownText = FindTextInPanel(startGuidePanel, StartCountdownTextNames);
                if (startConfirmButton == null) startConfirmButton = FindButtonInPanel(startGuidePanel, StartButtonNames);
            }
            else if (useRuntimeGuideFallback)
            {
                CreateRuntimeStartGuide();
            }

            if (startGuideText != null && string.IsNullOrWhiteSpace(startGuideText.text) && !string.IsNullOrWhiteSpace(startGuideMessage))
            {
                startGuideText.text = startGuideMessage;
            }

            ResolveClearTrophyReferences();
            clearTrophyInitiallyActive = clearTrophyObject != null && clearTrophyObject.activeSelf;
            if (clearTrophyObject != null)
            {
                clearTrophyOriginalPosition = clearTrophyObject.transform.position;
            }
            SetClearTrophyAlpha(0f);
        }

        private void Start()
        {
            GameSessionManager.Ensure().StartGame(GameType.AjouBoontu, SceneLoader.AjouBoontuScene);

            if (runner != null)
            {
                runner.Initialize(this);
                runner.SetRunSpeed(initialRunSpeed);
                runStartX = runner.transform.position.x;
                runner.SetRunning(false);
            }

            SetGameRuntimeEnabled(false);
            nextSpeedIncreaseTime = speedIncreaseIntervalSeconds > 0f ? speedIncreaseIntervalSeconds : float.PositiveInfinity;

            if (ui != null)
            {
                ui.SetBestScore(ScoreRecordManager.GetBestScore(GameType.AjouBoontu));
                ui.SetScore(Score);
                ui.SetRemainingDistance(clearDistance);
            }

            if (startGuidePanel != null)
            {
                startGuidePanel.SetActive(true);
                SetStartGuideText();
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

        private void OnStartConfirmClicked()
        {
            if (isCountdownStarted)
            {
                return;
            }

            isCountdownStarted = true;
            if (startConfirmButton != null)
            {
                startConfirmButton.interactable = false;
            }

            HideGuidePanelForCountdown();
            StartCoroutine(StartCountdownRoutine());
        }

        private void Update()
        {
            if (!IsGameRunning || IsGameOver)
            {
                return;
            }

            elapsedTime += Time.deltaTime;
            UpdateRunSpeed();

            scoreFloat += scorePerSecond * Time.deltaTime;
            Score = Mathf.FloorToInt(scoreFloat);

            float remainingDistance = GetRemainingDistance();
            if (ui != null)
            {
                ui.SetScore(Score);
                ui.SetRemainingDistance(remainingDistance);
            }

            if (remainingDistance <= 0f)
            {
                ClearRun();
            }
        }

        public void AddItemScore(int amount)
        {
            if (!IsGameRunning || IsGameOver)
            {
                return;
            }

            scoreFloat += amount <= 0 ? itemScore : amount;
            Score = Mathf.FloorToInt(scoreFloat);
            if (ui != null) ui.SetScore(Score);
        }

        public void GameOver(string reason)
        {
            if (!IsGameRunning)
            {
                return;
            }

            if (IsGameOver)
            {
                return;
            }

            IsGameOver = true;
            GameSessionManager.Ensure().SetResult(Score, string.IsNullOrWhiteSpace(reason) ? "Run ended." : reason);
            SceneLoader.LoadResult();
        }

        private void ClearRun()
        {
            if (IsGameOver)
            {
                return;
            }

            IsGameOver = true;
            GameSessionManager.Ensure().SetResult(Score, "Reached the finish line.", $"Score {Score:N0}");
            if (clearSequenceCoroutine != null)
            {
                StopCoroutine(clearSequenceCoroutine);
            }

            clearSequenceCoroutine = StartCoroutine(ClearSequenceRoutine());
        }

        private void UpdateRunSpeed()
        {
            if (runner == null || speedIncreaseIntervalSeconds <= 0f || speedIncreaseAmount <= 0f)
            {
                return;
            }

            while (elapsedTime >= nextSpeedIncreaseTime)
            {
                runner.SetRunSpeed(runner.RunSpeed + speedIncreaseAmount);
                nextSpeedIncreaseTime += speedIncreaseIntervalSeconds;
            }
        }

        private float GetRemainingDistance()
        {
            if (runner == null)
            {
                return clearDistance;
            }

            float travelledDistance = Mathf.Max(0f, runner.transform.position.x - runStartX);
            return Mathf.Max(0f, clearDistance - travelledDistance);
        }

        private IEnumerator StartCountdownRoutine()
        {
            float remaining = Mathf.Max(0f, startDelaySeconds);
            int lastShow = -1;

            while (remaining > 0f)
            {
                int sec = Mathf.CeilToInt(remaining);
                if (sec != lastShow)
                {
                    if (startCountdownText != null)
                    {
                        startCountdownText.text = $"{sec}\uCD08 \uB4A4 \uC2DC\uC791";
                    }

                    lastShow = sec;
                }

                remaining -= Time.deltaTime;
                yield return null;
            }

            if (startCountdownText != null)
            {
                startCountdownText.text = string.Empty;
            }

            StartGame();
        }

        private void StartGame()
        {
            IsGameRunning = true;
            RestoreGuidePanelAfterCountdown();
            if (startGuidePanel != null)
            {
                startGuidePanel.SetActive(false);
            }

            if (runner != null)
            {
                runner.SetRunning(true);
            }

            SetGameRuntimeEnabled(true);
            if (startGuideText != null && startGuideText.text.Contains("\uCD08 \uB4A4 \uC2DC\uC791"))
            {
                startGuideText.text = string.Empty;
            }
        }

        private void SetStartGuideText()
        {
            if (startGuideText == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(startGuideText.text) && !string.IsNullOrWhiteSpace(startGuideMessage))
            {
                startGuideText.text = startGuideMessage;
            }
            if (startCountdownText != null)
            {
                startCountdownText.text = string.Empty;
            }
        }

        private void HideGuidePanelForCountdown()
        {
            if (startGuidePanel == null)
            {
                return;
            }

            hiddenStartGuideBehaviours.Clear();
            Behaviour[] behaviours = startGuidePanel.GetComponentsInChildren<Behaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                Behaviour behaviour = behaviours[i];
                if (behaviour == null || !behaviour.enabled)
                {
                    continue;
                }

                if (startCountdownText != null && (behaviour == startCountdownText || behaviour.transform == startCountdownText.transform))
                {
                    continue;
                }

                hiddenStartGuideBehaviours.Add(behaviour);
                behaviour.enabled = false;
            }

            if (startCountdownText != null)
            {
                startCountdownText.enabled = true;
                startCountdownText.gameObject.SetActive(true);
            }
        }

        private void RestoreGuidePanelAfterCountdown()
        {
            for (int i = 0; i < hiddenStartGuideBehaviours.Count; i++)
            {
                if (hiddenStartGuideBehaviours[i] != null)
                {
                    hiddenStartGuideBehaviours[i].enabled = true;
                }
            }

            hiddenStartGuideBehaviours.Clear();
        }

        private IEnumerator ClearSequenceRoutine()
        {
            IsGameRunning = false;
            SetGameRuntimeEnabled(false);

            if (runner != null)
            {
                runner.SetRunning(false);
            }

            bool hasTrophy = ResolveClearTrophyReferences();
            if (!hasTrophy)
            {
                SceneLoader.LoadResult();
                yield break;
            }

            if (clearTrophyObject != null)
            {
                PositionClearTrophyAtCamera();
                clearTrophyObject.SetActive(true);
            }

            SetClearTrophyAlpha(0f);

            float fadeInDuration = Mathf.Max(0.01f, clearTrophyFadeInSeconds);
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                SetClearTrophyAlpha(Mathf.Clamp01(elapsed / fadeInDuration));
                yield return null;
            }

            SetClearTrophyAlpha(1f);

            if (clearTrophyVisibleSeconds > 0f)
            {
                yield return new WaitForSeconds(clearTrophyVisibleSeconds);
            }

            float fadeOutDuration = Mathf.Max(0.01f, clearTrophyFadeOutSeconds);
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                SetClearTrophyAlpha(1f - Mathf.Clamp01(elapsed / fadeOutDuration));
                yield return null;
            }

            SetClearTrophyAlpha(0f);
            if (clearTrophyObject != null && !clearTrophyInitiallyActive)
            {
                clearTrophyObject.SetActive(false);
            }
            RestoreClearTrophyTransform();
            SceneLoader.LoadResult();
        }

        private bool ResolveClearTrophyReferences()
        {
            if (clearTrophyObject == null && !string.IsNullOrWhiteSpace(clearTrophyObjectName))
            {
                clearTrophyObject = FindSceneObjectIncludingInactive(clearTrophyObjectName);
            }

            if (clearTrophyObject != null)
            {
                if (clearTrophySpriteRenderer == null)
                {
                    clearTrophySpriteRenderer = clearTrophyObject.GetComponent<SpriteRenderer>();
                    if (clearTrophySpriteRenderer == null)
                    {
                        clearTrophySpriteRenderer = clearTrophyObject.GetComponentInChildren<SpriteRenderer>(true);
                    }
                }

                if (clearTrophyGraphic == null)
                {
                    clearTrophyGraphic = clearTrophyObject.GetComponent<Graphic>();
                    if (clearTrophyGraphic == null)
                    {
                        clearTrophyGraphic = clearTrophyObject.GetComponentInChildren<Graphic>(true);
                    }
                }
            }

            return clearTrophySpriteRenderer != null || clearTrophyGraphic != null;
        }

        private void PositionClearTrophyAtCamera()
        {
            if (clearTrophyObject == null || clearTrophyGraphic != null)
            {
                return;
            }

            Camera targetCamera = clearTrophyCamera != null ? clearTrophyCamera : Camera.main;
            if (targetCamera == null)
            {
                return;
            }

            clearTrophyOriginalPosition = clearTrophyObject.transform.position;

            Vector3 cameraPosition = targetCamera.transform.position;
            Vector3 trophyPosition = clearTrophyObject.transform.position;
            trophyPosition.x = cameraPosition.x + clearTrophyCameraOffset.x;
            trophyPosition.y = cameraPosition.y + clearTrophyCameraOffset.y;
            clearTrophyObject.transform.position = trophyPosition;
        }

        private void RestoreClearTrophyTransform()
        {
            if (clearTrophyObject == null || clearTrophyGraphic != null)
            {
                return;
            }

            clearTrophyObject.transform.position = clearTrophyOriginalPosition;
        }

        private static GameObject FindSceneObjectIncludingInactive(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                return null;
            }

            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < allObjects.Length; i++)
            {
                GameObject candidate = allObjects[i];
                if (candidate == null)
                {
                    continue;
                }

                if (candidate.name != objectName)
                {
                    continue;
                }

                if (!candidate.scene.IsValid() || !candidate.scene.isLoaded)
                {
                    continue;
                }

                return candidate;
            }

            return null;
        }

        private void SetClearTrophyAlpha(float alpha)
        {
            alpha = Mathf.Clamp01(alpha);

            if (clearTrophySpriteRenderer != null)
            {
                Color color = clearTrophySpriteRenderer.color;
                color.a = alpha;
                clearTrophySpriteRenderer.color = color;
            }

            if (clearTrophyGraphic != null)
            {
                Color color = clearTrophyGraphic.color;
                color.a = alpha;
                clearTrophyGraphic.color = color;
            }
        }

        private void SetGameRuntimeEnabled(bool enabled)
        {
            RunnerObstacleSpawner obstacleSpawner = FindFirstObjectByType<RunnerObstacleSpawner>();
            RunnerItemSpawner itemSpawner = FindFirstObjectByType<RunnerItemSpawner>();
            RunnerPlatformSpawner platformSpawner = FindFirstObjectByType<RunnerPlatformSpawner>();
            WireActionController wireAction = FindFirstObjectByType<WireActionController>();

            if (obstacleSpawner != null) obstacleSpawner.enabled = enabled;
            if (itemSpawner != null) itemSpawner.enabled = enabled;
            if (platformSpawner != null) platformSpawner.enabled = enabled;

            if (wireAction != null)
            {
                wireAction.enabled = enabled;
                if (enabled)
                {
                    wireAction.ResetForStart();
                }
            }
        }

        private GameObject FindStartGuidePanelByScan()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                return null;
            }

            const string guideKeyword = "Guide";
            const string startKeyword = "Start";
            Transform[] allTransforms = canvas.GetComponentsInChildren<Transform>(true);
            Transform fallbackCandidate = null;

            foreach (Transform child in allTransforms)
            {
                if (child == null || child == canvas.transform)
                {
                    continue;
                }

                string name = child.name ?? string.Empty;
                bool isCandidateByName = name.IndexOf(guideKeyword, System.StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf(startKeyword, System.StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf(startGuidePanelName, System.StringComparison.OrdinalIgnoreCase) >= 0;

                Text[] texts = child.GetComponentsInChildren<Text>(true);
                Button[] buttons = child.GetComponentsInChildren<Button>(true);
                Text guideText = FindTextInPanel(child.gameObject, StartGuideTextNames);
                Text countdownText = FindTextInPanel(child.gameObject, StartCountdownTextNames);
                Button confirmButton = FindButtonInPanel(child.gameObject, StartButtonNames);

                if (isCandidateByName && guideText != null && buttons.Length > 0)
                {
                    return child.gameObject;
                }

                if (fallbackCandidate == null && guideText != null && countdownText != null && confirmButton != null)
                {
                    fallbackCandidate = child;
                }
            }

            return fallbackCandidate != null ? fallbackCandidate.gameObject : null;
        }

        private Text FindTextInPanel(GameObject root, string[] preferredNames)
        {
            Text[] texts = root.GetComponentsInChildren<Text>(true);
            if (texts == null || texts.Length == 0)
            {
                return null;
            }

            foreach (var preferredName in preferredNames)
            {
                for (int i = 0; i < texts.Length; i++)
                {
                    if (texts[i] != null && string.Equals(texts[i].name, preferredName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return texts[i];
                    }
                }
            }

            foreach (var preferredName in preferredNames)
            {
                for (int i = 0; i < texts.Length; i++)
                {
                    if (texts[i] != null && texts[i].name.IndexOf(preferredName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return texts[i];
                    }
                }
            }

            return texts.Length > 0 ? texts[0] : null;
        }

        private Button FindButtonInPanel(GameObject root, string[] preferredNames)
        {
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            if (buttons == null || buttons.Length == 0)
            {
                return null;
            }

            foreach (var preferredName in preferredNames)
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i] != null && string.Equals(buttons[i].name, preferredName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return buttons[i];
                    }
                }
            }

            foreach (var preferredName in preferredNames)
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i] != null && buttons[i].name.IndexOf(preferredName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return buttons[i];
                    }
                }
            }

            return buttons.Length > 0 ? buttons[0] : null;
        }

        private void CreateRuntimeStartGuide()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                return;
            }

            var panel = new GameObject("StartGuidePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
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

            var guideObj = new GameObject("GuideText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            guideObj.transform.SetParent(panel.transform, false);
            RectTransform guideRect = guideObj.GetComponent<RectTransform>();
            guideRect.anchorMin = new Vector2(0.5f, 1f);
            guideRect.anchorMax = new Vector2(0.5f, 1f);
            guideRect.pivot = new Vector2(0.5f, 1f);
            guideRect.anchoredPosition = new Vector2(0f, -120f);
            guideRect.sizeDelta = new Vector2(760f, 220f);
            Text guideText = guideObj.GetComponent<Text>();
            if (uiFont != null) guideText.font = uiFont;
            guideText.fontSize = 48;
            guideText.alignment = TextAnchor.MiddleCenter;
            guideText.color = Color.white;
            guideText.text = startGuideMessage;
            startGuideText = guideText;

            var countdownObj = new GameObject("CountdownText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            countdownObj.transform.SetParent(panel.transform, false);
            RectTransform countdownRect = countdownObj.GetComponent<RectTransform>();
            countdownRect.anchorMin = new Vector2(0.5f, 0.5f);
            countdownRect.anchorMax = new Vector2(0.5f, 0.5f);
            countdownRect.pivot = new Vector2(0.5f, 0.5f);
            countdownRect.anchoredPosition = new Vector2(0f, -190f);
            countdownRect.sizeDelta = new Vector2(220f, 100f);
            Text countdownText = countdownObj.GetComponent<Text>();
            if (uiFont != null) countdownText.font = uiFont;
            countdownText.fontSize = 56;
            countdownText.fontStyle = FontStyle.Bold;
            countdownText.alignment = TextAnchor.MiddleCenter;
            countdownText.color = new Color(1f, 1f, 0.8f, 1f);
            startCountdownText = countdownText;

            var btnObj = new GameObject("StartButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(panel.transform, false);
            RectTransform buttonRect = btnObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = new Vector2(0f, 170f);
            buttonRect.sizeDelta = new Vector2(200f, 60f);
            Image buttonImage = btnObj.GetComponent<Image>();
            buttonImage.color = new Color(0.1f, 0.65f, 0.2f, 0.98f);

            var btnTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            btnTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;

            Text btnText = btnTextObj.GetComponent<Text>();
            if (uiFont != null) btnText.font = uiFont;
            btnText.text = "\uD655\uC778";
            btnText.fontStyle = FontStyle.Bold;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;

            startConfirmButton = btnObj.GetComponent<Button>();
            startConfirmButton.targetGraphic = buttonImage;
            isCountdownStarted = false;
        }
    }
}



