using AjouFestival.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AjouFestival.UI
{
    [ExecuteAlways]
    public sealed class ResultUI : MonoBehaviour
    {
        [Header("Result")]
        [SerializeField] private Text gameNameText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Text resultMessageText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button gameSelectButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button recordBoardButton;

        [Header("Score Record UI")]
        [SerializeField] private bool showRecordPromptOnStart = true;
        [SerializeField] private bool createRecordFallbackUI = true;
        [SerializeField] private GameObject recordPromptPanel;
        [SerializeField] private Text recordPromptText;
        [SerializeField] private Button recordYesButton;
        [SerializeField] private Button recordNoButton;
        [SerializeField] private GameObject nameInputPanel;
        [SerializeField] private Text nameInputTitleText;
        [SerializeField] private InputField playerNameInput;
        [SerializeField] private Button recordSaveButton;
        [SerializeField] private Button recordCancelButton;
        [SerializeField] private Text recordSavedText;

        [Header("Record Text")]
        [SerializeField] private string recordPromptMessage = "\uC810\uC218\uB97C \uAE30\uB85D\uD558\uC2DC\uACA0\uC2B5\uB2C8\uAE4C?";
        [SerializeField] private string nameInputMessage = "\uC774\uB984\uC744 \uC785\uB825\uD574\uC8FC\uC138\uC694";
        [SerializeField] private string recordSavedMessage = "\uAE30\uB85D\uC774 \uC800\uC7A5\uB418\uC5C8\uC2B5\uB2C8\uB2E4.";

        private GameSessionManager session;
        private bool recordSaved;

        private void OnEnable()
        {
            if (!Application.isPlaying && createRecordFallbackUI)
            {
                ResolveReferences();
                EnsureSceneBgmObject();
                EnsureRecordFallbackUI();
                EnsureButtonAudioComponents();
            }
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            if (gameNameText == null) gameNameText = transform.Find("GameNameText")?.GetComponent<Text>();
            if (scoreText == null) scoreText = transform.Find("ScoreText")?.GetComponent<Text>();
            if (bestScoreText == null) bestScoreText = transform.Find("BestScoreText")?.GetComponent<Text>();
            if (resultMessageText == null) resultMessageText = transform.Find("ResultMessageText")?.GetComponent<Text>();
            if (retryButton == null) retryButton = transform.Find("RetryButton")?.GetComponent<Button>();
            if (gameSelectButton == null) gameSelectButton = transform.Find("GameSelectButton")?.GetComponent<Button>();
            if (mainMenuButton == null) mainMenuButton = transform.Find("MainMenuButton")?.GetComponent<Button>();
            if (recordBoardButton == null) recordBoardButton = transform.Find("RecordBoardButton")?.GetComponent<Button>();
        }

        private void Start()
        {
            EnsureSceneBgmObject();
            AudioManager.Ensure();
            session = GameSessionManager.Ensure();
            GameType type = session.CurrentGameType;

            if (gameNameText != null) gameNameText.text = GetGameName(type);
            if (scoreText != null) scoreText.text = $"\uCD5C\uC885 \uC810\uC218: {session.LastScoreText}";
            if (bestScoreText != null) bestScoreText.text = $"\uCD5C\uACE0 \uC810\uC218: {ScoreRecordManager.GetBestScore(type):N0}";
            if (resultMessageText != null) resultMessageText.text = string.IsNullOrWhiteSpace(session.LastResultMessage) ? "\uD50C\uB808\uC774 \uACB0\uACFC" : session.LastResultMessage;

            if (retryButton != null) retryButton.onClick.AddListener(SceneLoader.RestartLastGame);
            if (gameSelectButton != null) gameSelectButton.onClick.AddListener(SceneLoader.LoadGameSelect);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(SceneLoader.LoadMainMenu);

            if (createRecordFallbackUI)
            {
                EnsureRecordFallbackUI();
            }

            BindRecordUI();
            EnsureButtonAudioComponents();
            if (showRecordPromptOnStart && type != GameType.None)
            {
                ShowRecordPrompt();
            }
            else
            {
                HideRecordPanels();
            }
        }

        private void BindRecordUI()
        {
            if (recordPromptText != null) recordPromptText.text = recordPromptMessage;
            if (nameInputTitleText != null) nameInputTitleText.text = nameInputMessage;
            if (recordSavedText != null)
            {
                recordSavedText.text = string.Empty;
                recordSavedText.gameObject.SetActive(false);
            }

            if (recordYesButton != null)
            {
                recordYesButton.onClick.RemoveAllListeners();
                recordYesButton.onClick.AddListener(ShowNameInput);
            }

            if (recordNoButton != null)
            {
                recordNoButton.onClick.RemoveAllListeners();
                recordNoButton.onClick.AddListener(HideRecordPanels);
            }

            if (recordSaveButton != null)
            {
                recordSaveButton.onClick.RemoveAllListeners();
                recordSaveButton.onClick.AddListener(SaveNamedRecord);
            }

            if (recordCancelButton != null)
            {
                recordCancelButton.onClick.RemoveAllListeners();
                recordCancelButton.onClick.AddListener(HideRecordPanels);
            }

            if (recordBoardButton != null)
            {
                recordBoardButton.onClick.RemoveAllListeners();
                recordBoardButton.onClick.AddListener(SceneLoader.LoadRecordBoard);
            }
        }

        private void EnsureButtonAudioComponents()
        {
            EnsureButtonAudioComponent(retryButton);
            EnsureButtonAudioComponent(gameSelectButton);
            EnsureButtonAudioComponent(mainMenuButton);
            EnsureButtonAudioComponent(recordBoardButton);
            EnsureButtonAudioComponent(recordYesButton);
            EnsureButtonAudioComponent(recordNoButton);
            EnsureButtonAudioComponent(recordSaveButton);
            EnsureButtonAudioComponent(recordCancelButton);
        }

        private static void EnsureButtonAudioComponent(Button button)
        {
            if (button == null || button.GetComponent<CommonButtonUI>() != null)
            {
                return;
            }

            button.gameObject.AddComponent<CommonButtonUI>();
        }

        private static void EnsureSceneBgmObject()
        {
            GameObject sceneBgm = GameObject.Find("SceneBGM");
            if (sceneBgm == null)
            {
                sceneBgm = new GameObject("SceneBGM");
            }

            AudioSource source = sceneBgm.GetComponent<AudioSource>();
            if (source == null)
            {
                source = sceneBgm.AddComponent<AudioSource>();
            }

            source.playOnAwake = true;
            source.loop = true;
            source.spatialBlend = 0f;
        }

        private void ShowRecordPrompt()
        {
            if (recordSaved)
            {
                return;
            }

            if (recordPromptPanel != null) recordPromptPanel.SetActive(true);
            if (nameInputPanel != null) nameInputPanel.SetActive(false);
        }

        private void ShowNameInput()
        {
            if (recordPromptPanel != null) recordPromptPanel.SetActive(false);
            if (nameInputPanel != null) nameInputPanel.SetActive(true);
            if (playerNameInput != null)
            {
                playerNameInput.text = string.Empty;
                playerNameInput.ActivateInputField();
            }
        }

        private void SaveNamedRecord()
        {
            if (recordSaved || session == null || session.CurrentGameType == GameType.None)
            {
                return;
            }

            string playerName = playerNameInput != null ? playerNameInput.text : string.Empty;
            ScoreHistoryManager.AddRecord(session.CurrentGameType, playerName, session.LastScore, session.LastScoreText, session.LastResultMessage);
            recordSaved = true;

            if (recordSavedText != null)
            {
                recordSavedText.text = recordSavedMessage;
                recordSavedText.gameObject.SetActive(true);
            }

            if (recordPromptPanel != null) recordPromptPanel.SetActive(false);
            if (nameInputPanel != null) nameInputPanel.SetActive(false);
        }

        private void HideRecordPanels()
        {
            if (recordPromptPanel != null) recordPromptPanel.SetActive(false);
            if (nameInputPanel != null) nameInputPanel.SetActive(false);
        }

        private void EnsureRecordFallbackUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            }

            EnsureEventSystem();

            if (recordBoardButton == null)
            {
                recordBoardButton = FindOrCreateButton(canvas.transform, "RecordBoardButton", "\uAE30\uB85D \uBCF4\uAE30", new Vector2(0f, -260f), new Vector2(180f, 48f));
            }

            if (recordPromptPanel == null)
            {
                recordPromptPanel = FindOrCreatePanel(canvas.transform, "RecordPromptPanel", new Vector2(0f, 0f), new Vector2(430f, 220f));
                recordPromptText = FindOrCreateText(recordPromptPanel.transform, "PromptText", recordPromptMessage, new Vector2(0f, 48f), new Vector2(380f, 60f), 28);
                recordYesButton = FindOrCreateButton(recordPromptPanel.transform, "YesButton", "\uC608", new Vector2(-92f, -54f), new Vector2(130f, 46f));
                recordNoButton = FindOrCreateButton(recordPromptPanel.transform, "NoButton", "\uC544\uB2C8\uC624", new Vector2(92f, -54f), new Vector2(130f, 46f));
            }

            if (nameInputPanel == null)
            {
                nameInputPanel = FindOrCreatePanel(canvas.transform, "NameInputPanel", new Vector2(0f, 0f), new Vector2(460f, 260f));
                nameInputTitleText = FindOrCreateText(nameInputPanel.transform, "TitleText", nameInputMessage, new Vector2(0f, 76f), new Vector2(400f, 48f), 26);
                playerNameInput = FindOrCreateInputField(nameInputPanel.transform, "PlayerNameInput", new Vector2(0f, 14f), new Vector2(320f, 46f));
                recordSaveButton = FindOrCreateButton(nameInputPanel.transform, "SaveButton", "\uC800\uC7A5", new Vector2(-92f, -72f), new Vector2(130f, 46f));
                recordCancelButton = FindOrCreateButton(nameInputPanel.transform, "CancelButton", "\uCDE8\uC18C", new Vector2(92f, -72f), new Vector2(130f, 46f));
            }

            if (recordSavedText == null)
            {
                recordSavedText = FindOrCreateText(canvas.transform, "RecordSavedText", string.Empty, new Vector2(0f, 220f), new Vector2(520f, 48f), 24);
            }
        }

        private static GameObject FindOrCreatePanel(Transform parent, string name, Vector2 position, Vector2 size)
        {
            GameObject existing = FindUniqueDirectChild(parent, name);
            return existing != null ? existing : CreatePanel(parent, name, position, size);
        }

        private static Text FindOrCreateText(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize)
        {
            GameObject existing = FindUniqueDirectChild(parent, name);
            Text text = existing != null ? existing.GetComponent<Text>() : null;
            return text != null ? text : CreateText(parent, name, value, position, size, fontSize);
        }

        private static Button FindOrCreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size)
        {
            GameObject existing = FindUniqueDirectChild(parent, name);
            Button button = existing != null ? existing.GetComponent<Button>() : null;
            return button != null ? button : CreateButton(parent, name, label, position, size);
        }

        private static InputField FindOrCreateInputField(Transform parent, string name, Vector2 position, Vector2 size)
        {
            GameObject existing = FindUniqueDirectChild(parent, name);
            InputField input = existing != null ? existing.GetComponent<InputField>() : null;
            return input != null ? input : CreateInputField(parent, name, position, size);
        }

        private static GameObject FindUniqueDirectChild(Transform parent, string childName)
        {
            GameObject first = null;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child == null || child.name != childName)
                {
                    continue;
                }

                if (first == null)
                {
                    first = child.gameObject;
                    continue;
                }

                DestroyObject(child.gameObject);
            }

            return first;
        }

        private static void DestroyObject(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(obj);
            }
            else
            {
                DestroyImmediate(obj);
            }
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 position, Vector2 size)
        {
            GameObject obj = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            obj.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.09f, 0.94f);
            return obj;
        }

        private static Text CreateText(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize)
        {
            GameObject obj = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            obj.transform.SetParent(parent, false);
            Text text = obj.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size)
        {
            GameObject obj = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(CommonButtonUI));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            obj.GetComponent<Image>().color = new Color(0.08f, 0.42f, 0.86f, 0.95f);

            Text text = CreateText(obj.transform, "Text", label, Vector2.zero, size, 22);
            text.raycastTarget = false;

            Button button = obj.GetComponent<Button>();
            button.targetGraphic = obj.GetComponent<Image>();
            return button;
        }

        private static InputField CreateInputField(Transform parent, string name, Vector2 position, Vector2 size)
        {
            GameObject obj = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(InputField));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            obj.GetComponent<Image>().color = Color.white;

            Text text = CreateText(obj.transform, "Text", string.Empty, Vector2.zero, size - new Vector2(24f, 0f), 22);
            text.alignment = TextAnchor.MiddleLeft;
            text.color = Color.black;

            Text placeholder = CreateText(obj.transform, "Placeholder", "\uC774\uB984", Vector2.zero, size - new Vector2(24f, 0f), 22);
            placeholder.alignment = TextAnchor.MiddleLeft;
            placeholder.color = new Color(0.45f, 0.45f, 0.45f, 1f);

            InputField input = obj.GetComponent<InputField>();
            input.textComponent = text;
            input.placeholder = placeholder;
            return input;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static string GetGameName(GameType type)
        {
            return type switch
            {
                GameType.AjouBoontu => "\uC544\uC8FC\uBD84\uD22C",
                GameType.BalanceWalk => "\uCE58\uD1A0 \uADE0\uD615\uAC77\uAE30",
                GameType.Soccer => "\uC544\uC8FC 1\uB3001 \uCD95\uAD6C",
                _ => "\uBBF8\uB2C8\uAC8C\uC784"
            };
        }
    }
}
