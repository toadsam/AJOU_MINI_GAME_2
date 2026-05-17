using System.Collections.Generic;
using AjouFestival.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AjouFestival.UI
{
    [ExecuteAlways]
    public sealed class RecordBoardUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text recordsText;
        [SerializeField] private Text emptyText;
        [SerializeField] private Button allButton;
        [SerializeField] private Button ajouBoontuButton;
        [SerializeField] private Button balanceWalkButton;
        [SerializeField] private Button soccerButton;
        [SerializeField] private Button previousPageButton;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private Text pageText;
        [SerializeField] private Button gameSelectButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Runtime Fallback")]
        [SerializeField] private bool createFallbackUI = true;
        [SerializeField, Min(1)] private int recordsPerPage = 20;

        private GameType currentFilter = GameType.None;
        private int currentPage;

        private void OnEnable()
        {
            if (!Application.isPlaying && createFallbackUI)
            {
                EnsureSceneBgmObject();
                EnsureFallbackUI();
                EnsureButtonAudioComponents();
            }
        }

        private void Start()
        {
            EnsureSceneBgmObject();
            AudioManager.Ensure();
            if (createFallbackUI)
            {
                EnsureFallbackUI();
            }

            BindButtons();
            EnsureButtonAudioComponents();
            ShowRecords(GameType.None);
        }

        private void BindButtons()
        {
            BindFilterButton(allButton, GameType.None);
            BindFilterButton(ajouBoontuButton, GameType.AjouBoontu);
            BindFilterButton(balanceWalkButton, GameType.BalanceWalk);
            BindFilterButton(soccerButton, GameType.Soccer);

            if (gameSelectButton != null)
            {
                gameSelectButton.onClick.RemoveAllListeners();
                gameSelectButton.onClick.AddListener(SceneLoader.LoadGameSelect);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(SceneLoader.LoadMainMenu);
            }

            if (previousPageButton != null)
            {
                previousPageButton.onClick.RemoveAllListeners();
                previousPageButton.onClick.AddListener(ShowPreviousPage);
            }

            if (nextPageButton != null)
            {
                nextPageButton.onClick.RemoveAllListeners();
                nextPageButton.onClick.AddListener(ShowNextPage);
            }
        }

        private void EnsureButtonAudioComponents()
        {
            EnsureButtonAudioComponent(allButton);
            EnsureButtonAudioComponent(ajouBoontuButton);
            EnsureButtonAudioComponent(balanceWalkButton);
            EnsureButtonAudioComponent(soccerButton);
            EnsureButtonAudioComponent(previousPageButton);
            EnsureButtonAudioComponent(nextPageButton);
            EnsureButtonAudioComponent(gameSelectButton);
            EnsureButtonAudioComponent(mainMenuButton);
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

        private void BindFilterButton(Button button, GameType type)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                currentPage = 0;
                ShowRecords(type);
            });
        }

        private void ShowPreviousPage()
        {
            if (currentPage <= 0)
            {
                return;
            }

            currentPage--;
            ShowRecords(currentFilter);
        }

        private void ShowNextPage()
        {
            List<ScoreHistoryManager.ScoreHistoryRecord> records = ScoreHistoryManager.GetRecords(currentFilter);
            int totalPages = GetTotalPages(records.Count);
            if (currentPage >= totalPages - 1)
            {
                return;
            }

            currentPage++;
            ShowRecords(currentFilter);
        }

        private void ShowRecords(GameType type)
        {
            currentFilter = type;
            if (titleText != null)
            {
                titleText.text = type == GameType.None
                    ? "\uC804\uCCB4 \uAE30\uB85D"
                    : $"{GetGameName(type)} \uAE30\uB85D";
            }

            List<ScoreHistoryManager.ScoreHistoryRecord> records = ScoreHistoryManager.GetRecords(type);
            bool hasRecords = records.Count > 0;
            int totalPages = GetTotalPages(records.Count);
            currentPage = Mathf.Clamp(currentPage, 0, Mathf.Max(0, totalPages - 1));

            if (emptyText != null)
            {
                emptyText.gameObject.SetActive(!hasRecords);
                emptyText.text = "\uC800\uC7A5\uB41C \uAE30\uB85D\uC774 \uC5C6\uC2B5\uB2C8\uB2E4.";
            }

            if (pageText != null)
            {
                pageText.gameObject.SetActive(hasRecords);
                pageText.text = hasRecords ? $"{currentPage + 1} / {totalPages}" : string.Empty;
            }

            if (previousPageButton != null)
            {
                previousPageButton.interactable = hasRecords && currentPage > 0;
            }

            if (nextPageButton != null)
            {
                nextPageButton.interactable = hasRecords && currentPage < totalPages - 1;
            }

            if (recordsText == null)
            {
                return;
            }

            recordsText.gameObject.SetActive(hasRecords);
            if (!hasRecords)
            {
                recordsText.text = string.Empty;
                return;
            }

            int pageSize = Mathf.Max(1, recordsPerPage);
            int startIndex = currentPage * pageSize;
            int endIndex = Mathf.Min(records.Count, startIndex + pageSize);
            var lines = new List<string>(endIndex - startIndex);
            for (int i = startIndex; i < endIndex; i++)
            {
                ScoreHistoryManager.ScoreHistoryRecord record = records[i];
                lines.Add($"{i + 1}. {GetGameName(record.gameType)}  |  {record.playerName}  |  {record.scoreText}  |  {record.recordedAt}");
            }

            recordsText.text = string.Join("\n", lines);
        }

        private int GetTotalPages(int recordCount)
        {
            if (recordCount <= 0)
            {
                return 1;
            }

            return Mathf.CeilToInt(recordCount / (float)Mathf.Max(1, recordsPerPage));
        }

        private void EnsureFallbackUI()
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

            if (titleText == null)
            {
                titleText = CreateText(canvas.transform, "TitleText", "\uAE30\uB85D", new Vector2(0f, 280f), new Vector2(760f, 62f), 38, TextAnchor.MiddleCenter);
            }

            if (allButton == null) allButton = CreateButton(canvas.transform, "AllButton", "\uC804\uCCB4", new Vector2(-270f, 214f), new Vector2(126f, 42f));
            if (ajouBoontuButton == null) ajouBoontuButton = CreateButton(canvas.transform, "AjouBoontuButton", "\uC544\uC8FC\uBD84\uD22C", new Vector2(-90f, 214f), new Vector2(150f, 42f));
            if (balanceWalkButton == null) balanceWalkButton = CreateButton(canvas.transform, "BalanceWalkButton", "\uADE0\uD615\uAC77\uAE30", new Vector2(90f, 214f), new Vector2(150f, 42f));
            if (soccerButton == null) soccerButton = CreateButton(canvas.transform, "SoccerButton", "\uCD95\uAD6C", new Vector2(270f, 214f), new Vector2(126f, 42f));

            if (recordsText == null)
            {
                recordsText = CreateText(canvas.transform, "RecordsText", string.Empty, new Vector2(0f, 20f), new Vector2(860f, 320f), 22, TextAnchor.UpperLeft);
                recordsText.horizontalOverflow = HorizontalWrapMode.Wrap;
                recordsText.verticalOverflow = VerticalWrapMode.Overflow;
            }

            if (emptyText == null)
            {
                emptyText = CreateText(canvas.transform, "EmptyText", string.Empty, new Vector2(0f, 30f), new Vector2(600f, 70f), 26, TextAnchor.MiddleCenter);
            }

            if (previousPageButton == null) previousPageButton = CreateButton(canvas.transform, "PreviousPageButton", "<", new Vector2(-140f, -220f), new Vector2(76f, 42f));
            if (pageText == null) pageText = CreateText(canvas.transform, "PageText", "1 / 1", new Vector2(0f, -220f), new Vector2(130f, 42f), 22, TextAnchor.MiddleCenter);
            if (nextPageButton == null) nextPageButton = CreateButton(canvas.transform, "NextPageButton", ">", new Vector2(140f, -220f), new Vector2(76f, 42f));

            if (gameSelectButton == null) gameSelectButton = CreateButton(canvas.transform, "GameSelectButton", "\uAC8C\uC784 \uC120\uD0DD", new Vector2(-100f, -282f), new Vector2(170f, 48f));
            if (mainMenuButton == null) mainMenuButton = CreateButton(canvas.transform, "MainMenuButton", "\uBA54\uC778", new Vector2(100f, -282f), new Vector2(170f, 48f));
        }

        private static Text CreateText(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
        {
            GameObject obj = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            obj.transform.SetParent(parent, false);
            Text text = obj.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
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
            obj.GetComponent<Image>().color = new Color(0.08f, 0.42f, 0.86f, 0.96f);

            Text text = CreateText(obj.transform, "Text", label, Vector2.zero, size, 20, TextAnchor.MiddleCenter);
            text.raycastTarget = false;

            Button button = obj.GetComponent<Button>();
            button.targetGraphic = obj.GetComponent<Image>();
            return button;
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
                GameType.Soccer => "\uCD95\uAD6C",
                _ => "\uC804\uCCB4"
            };
        }
    }
}
