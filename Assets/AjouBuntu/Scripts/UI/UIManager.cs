using System.Collections;
using AjouBuntu.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AjouBuntu.UI
{
    public sealed class UIManager : MonoBehaviour
    {
        private Text scoreText;
        private Text aplusText;
        private Text highScoreText;
        private Text distanceText;
        private Image distanceFill;
        private Text difficultyText;
        private Coroutine difficultyRoutine;

        public void BuildHud(Canvas canvas)
        {
            Font font = UiFactory.GetDefaultFont();

            Image topPanel = UiFactory.CreatePanel(canvas.transform, "HudTopPanel", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(860f, 62f), new Vector2(0f, -18f));
            topPanel.color = new Color(0.05f, 0.19f, 0.34f, 0.62f);

            scoreText = UiFactory.CreateText(topPanel.transform, "ScoreText", font, 24, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(260f, 42f), new Vector2(32f, 0f));
            aplusText = UiFactory.CreateText(topPanel.transform, "APlusText", font, 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(240f, 42f), Vector2.zero);
            highScoreText = UiFactory.CreateText(topPanel.transform, "HighScoreText", font, 24, TextAnchor.MiddleRight, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(260f, 42f), new Vector2(-32f, 0f));

            Image gauge = UiFactory.CreatePanel(canvas.transform, "DistanceGauge", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(520f, 30f), new Vector2(0f, 42f));
            gauge.color = new Color(0.02f, 0.1f, 0.18f, 0.56f);
            Image fillRoot = UiFactory.CreatePanel(gauge.transform, "DistanceFill", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(500f, 14f), new Vector2(10f, 0f));
            fillRoot.color = new Color(0.14f, 0.95f, 1f, 0.88f);
            distanceFill = fillRoot;

            distanceText = UiFactory.CreateText(canvas.transform, "DistanceText", font, 18, TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(360f, 26f), new Vector2(0f, 76f));
            difficultyText = UiFactory.CreateText(canvas.transform, "DifficultyPopup", font, 38, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(620f, 74f), Vector2.zero);
            difficultyText.color = new Color(0.8f, 1f, 1f, 0f);
        }

        public void SetScore(int score, int highScore, int aplus)
        {
            if (scoreText == null)
            {
                return;
            }

            scoreText.text = $"점수 {score:N0}";
            aplusText.text = $"A+ {aplus}";
            highScoreText.text = $"최고 {highScore:N0}";
        }

        public void SetDistance(float remaining, float total, float progress)
        {
            if (distanceText == null)
            {
                return;
            }

            distanceText.text = $"남은 거리 {Mathf.CeilToInt(remaining):N0} px";
            RectTransform fillRect = distanceFill.rectTransform;
            fillRect.sizeDelta = new Vector2(Mathf.Lerp(0f, 500f, progress), fillRect.sizeDelta.y);
        }

        public void ShowDifficulty(int stage)
        {
            if (difficultyText == null)
            {
                return;
            }

            if (difficultyRoutine != null)
            {
                StopCoroutine(difficultyRoutine);
            }

            difficultyRoutine = StartCoroutine(DifficultyRoutine(stage));
        }

        private IEnumerator DifficultyRoutine(int stage)
        {
            difficultyText.text = $"★ 난이도 상승 {stage} ★";
            float time = 0f;
            while (time < 1.35f)
            {
                time += Time.deltaTime;
                float alpha = time < 0.22f ? time / 0.22f : 1f - Mathf.Clamp01((time - 0.85f) / 0.5f);
                difficultyText.color = new Color(0.8f, 1f, 1f, alpha);
                difficultyText.transform.localScale = Vector3.one * Mathf.Lerp(0.94f, 1.06f, Mathf.Sin(Mathf.Clamp01(time / 1.35f) * Mathf.PI));
                yield return null;
            }

            difficultyText.color = new Color(0.8f, 1f, 1f, 0f);
        }
    }
}
