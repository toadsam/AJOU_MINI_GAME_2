using System;
using AjouFestival.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AjouFestival.Games.Soccer
{
    public sealed class SoccerUI : MonoBehaviour
    {
        [SerializeField] private Text timeText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text hintText;
        [SerializeField] private Button exitButton;

        private Action<SoccerMatchMode, SoccerAIDifficulty> modeSelectionCallback;
        private GameObject modeSelectionPanel;

        private void Awake()
        {
            if (timeText == null) timeText = transform.Find("TimeText")?.GetComponent<Text>();
            if (scoreText == null) scoreText = transform.Find("ScoreText")?.GetComponent<Text>();
            if (hintText == null) hintText = transform.Find("HintText")?.GetComponent<Text>();
            if (exitButton == null) exitButton = transform.Find("ExitButton")?.GetComponent<Button>();
        }

        private void Start()
        {
            if (exitButton != null) exitButton.onClick.AddListener(SceneLoader.LoadGameSelect);
        }

        public void SetMatch(float timeRemaining, int p1Score, int p2Score)
        {
            if (timeText != null) timeText.text = $"Time {Mathf.CeilToInt(timeRemaining)}";
            if (scoreText != null) scoreText.text = $"P1 {p1Score} : {p2Score} P2";
        }

        public void ShowModeSelection(Action<SoccerMatchMode, SoccerAIDifficulty> onModeSelected)
        {
            modeSelectionCallback = onModeSelected;
            EnsureModeSelectionPanel();

            if (modeSelectionPanel != null)
            {
                modeSelectionPanel.SetActive(true);
                modeSelectionPanel.transform.SetAsLastSibling();
            }
        }

        public void HideModeSelection()
        {
            if (modeSelectionPanel != null)
            {
                modeSelectionPanel.SetActive(false);
            }
        }

        public void SetModeHint(SoccerMatchMode? mode, SoccerAIDifficulty? difficulty)
        {
            if (hintText == null)
            {
                return;
            }

            if (mode == null)
            {
                hintText.text = "Choose a mode to start. 1 vs 1 or AI Easy / Medium / Hard";
                return;
            }

            if (mode == SoccerMatchMode.OneVsOne)
            {
                hintText.text = "1 vs 1  P1 A/D move, W jump, Space or S kick   P2 Left/Right move, Up jump, Enter, Ctrl or Down kick   R restart   ESC menu";
                return;
            }

            string difficultyLabel = GetDifficultyLabel(difficulty ?? SoccerAIDifficulty.Medium);
            hintText.text = $"VS AI {difficultyLabel}  P1 A/D move, W jump, Space or S kick   R restart   ESC menu";
        }

        private void EnsureModeSelectionPanel()
        {
            if (modeSelectionPanel != null)
            {
                return;
            }

            modeSelectionPanel = CreateRectObject("ModeSelectionPanel", transform).gameObject;
            RectTransform panelRect = modeSelectionPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image overlay = modeSelectionPanel.AddComponent<Image>();
            overlay.color = new Color(0.04f, 0.08f, 0.12f, 0.78f);

            RectTransform cardRect = CreateRectObject("ModeCard", modeSelectionPanel.transform);
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(420f, 350f);
            cardRect.anchoredPosition = Vector2.zero;

            Image cardImage = cardRect.gameObject.AddComponent<Image>();
            cardImage.color = new Color(0.92f, 0.97f, 1f, 0.96f);

            Text titleText = CreateText("ModeTitle", cardRect, new Vector2(0f, 125f), new Vector2(340f, 40f), 28, Color.black);
            titleText.text = "Choose Match Mode";

            Text subtitleText = CreateText("ModeSubtitle", cardRect, new Vector2(0f, 86f), new Vector2(340f, 32f), 18, new Color(0.08f, 0.24f, 0.44f));
            subtitleText.text = "Pick how Player 2 is controlled";

            CreateModeButton(cardRect, "1 vs 1", new Vector2(0f, 20f), () => SelectMode(SoccerMatchMode.OneVsOne, SoccerAIDifficulty.Medium), new Color(0.14f, 0.4f, 0.71f));
            CreateModeButton(cardRect, "AI Easy", new Vector2(0f, -38f), () => SelectMode(SoccerMatchMode.VersusAI, SoccerAIDifficulty.Easy), new Color(0.18f, 0.58f, 0.38f));
            CreateModeButton(cardRect, "AI Medium", new Vector2(0f, -96f), () => SelectMode(SoccerMatchMode.VersusAI, SoccerAIDifficulty.Medium), new Color(0.84f, 0.56f, 0.14f));
            CreateModeButton(cardRect, "AI Hard", new Vector2(0f, -154f), () => SelectMode(SoccerMatchMode.VersusAI, SoccerAIDifficulty.Hard), new Color(0.78f, 0.22f, 0.22f));

            modeSelectionPanel.SetActive(false);
        }

        private void SelectMode(SoccerMatchMode mode, SoccerAIDifficulty difficulty)
        {
            modeSelectionCallback?.Invoke(mode, difficulty);
        }

        private void CreateModeButton(RectTransform parent, string label, Vector2 anchoredPosition, Action onClick, Color buttonColor)
        {
            RectTransform buttonRect = CreateRectObject(label, parent);
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = anchoredPosition;
            buttonRect.sizeDelta = new Vector2(260f, 44f);

            Image buttonImage = buttonRect.gameObject.AddComponent<Image>();
            buttonImage.color = buttonColor;

            Button button = buttonRect.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = buttonColor;
            colors.highlightedColor = buttonColor * 1.08f;
            colors.pressedColor = buttonColor * 0.92f;
            colors.selectedColor = buttonColor;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            button.colors = colors;
            button.onClick.AddListener(() => onClick());

            Text buttonText = CreateText("Label", buttonRect, Vector2.zero, new Vector2(220f, 32f), 20, Color.white);
            buttonText.text = label;
        }

        private static RectTransform CreateRectObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj.GetComponent<RectTransform>();
        }

        private static Text CreateText(string name, RectTransform parent, Vector2 anchoredPosition, Vector2 size, int fontSize, Color color)
        {
            RectTransform textRect = CreateRectObject(name, parent);
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = anchoredPosition;
            textRect.sizeDelta = size;

            Text text = textRect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.raycastTarget = false;

            return text;
        }

        private static string GetDifficultyLabel(SoccerAIDifficulty difficulty)
        {
            return difficulty switch
            {
                SoccerAIDifficulty.Easy => "Easy",
                SoccerAIDifficulty.Medium => "Medium",
                SoccerAIDifficulty.Hard => "Hard",
                _ => "Medium"
            };
        }
    }
}
