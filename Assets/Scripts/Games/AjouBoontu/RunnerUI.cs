using AjouFestival.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AjouFestival.Games.AjouBoontu
{
    public sealed class RunnerUI : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text distanceText;
        [SerializeField] private Button exitButton;

        private void Awake()
        {
            if (scoreText == null) scoreText = transform.Find("ScoreText")?.GetComponent<Text>();
            if (bestScoreText == null) bestScoreText = transform.Find("BestScoreText")?.GetComponent<Text>();
            if (hintText == null) hintText = transform.Find("HintText")?.GetComponent<Text>();
            if (distanceText == null) distanceText = transform.Find("DistanceText")?.GetComponent<Text>();
            if (exitButton == null) exitButton = transform.Find("ExitButton")?.GetComponent<Button>();
        }

        private void Start()
        {
            EnsureDistanceText();

            if (hintText != null) hintText.text = "Space/Click: Jump, Double Jump   R: Restart   ESC: Select";
            if (distanceText != null && string.IsNullOrWhiteSpace(distanceText.text)) distanceText.text = "Left 0 m";
            if (exitButton != null) exitButton.onClick.AddListener(SceneLoader.LoadGameSelect);
        }

        public void SetScore(int score)
        {
            if (scoreText != null) scoreText.text = $"Score {score:N0}";
        }

        public void SetBestScore(int bestScore)
        {
            if (bestScoreText != null) bestScoreText.text = $"Best {bestScore:N0}";
        }

        public void SetRemainingDistance(float remainingDistance)
        {
            Text label = EnsureDistanceText();
            if (label != null)
            {
                label.text = $"Left {Mathf.CeilToInt(Mathf.Max(0f, remainingDistance)):N0} m";
            }
        }

        private Text EnsureDistanceText()
        {
            if (distanceText != null)
            {
                return distanceText;
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                return null;
            }

            GameObject obj = new GameObject("DistanceText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.SetParent(transform, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -80f);
            rect.sizeDelta = new Vector2(280f, 36f);

            distanceText = obj.GetComponent<Text>();
            distanceText.font = font;
            distanceText.fontSize = 20;
            distanceText.alignment = TextAnchor.MiddleCenter;
            distanceText.horizontalOverflow = HorizontalWrapMode.Overflow;
            distanceText.verticalOverflow = VerticalWrapMode.Overflow;
            distanceText.color = Color.white;
            distanceText.raycastTarget = false;
            distanceText.text = "Left 0 m";
            return distanceText;
        }
    }
}
