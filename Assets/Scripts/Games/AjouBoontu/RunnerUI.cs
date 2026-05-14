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
        [SerializeField] private Button exitButton;

        private void Awake()
        {
            if (scoreText == null) scoreText = transform.Find("ScoreText")?.GetComponent<Text>();
            if (bestScoreText == null) bestScoreText = transform.Find("BestScoreText")?.GetComponent<Text>();
            if (hintText == null) hintText = transform.Find("HintText")?.GetComponent<Text>();
            if (exitButton == null) exitButton = transform.Find("ExitButton")?.GetComponent<Button>();
        }

        private void Start()
        {
            if (hintText != null) hintText.text = "Space/클릭: 점프   공중에서 길게 누르기: 와이어   R: 재시작   ESC: 선택";
            if (exitButton != null) exitButton.onClick.AddListener(SceneLoader.LoadGameSelect);
        }

        public void SetScore(int score)
        {
            if (scoreText != null) scoreText.text = $"점수 {score:N0}";
        }

        public void SetBestScore(int bestScore)
        {
            if (bestScoreText != null) bestScoreText.text = $"최고 {bestScore:N0}";
        }
    }
}
