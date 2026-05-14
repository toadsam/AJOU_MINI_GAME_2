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
            if (hintText != null) hintText.text = "Space/Click: Jump, Double Jump   R: Restart   ESC: Select";
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
    }
}
