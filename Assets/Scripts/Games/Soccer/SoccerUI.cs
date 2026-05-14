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

        private void Awake()
        {
            if (timeText == null) timeText = transform.Find("TimeText")?.GetComponent<Text>();
            if (scoreText == null) scoreText = transform.Find("ScoreText")?.GetComponent<Text>();
            if (hintText == null) hintText = transform.Find("HintText")?.GetComponent<Text>();
            if (exitButton == null) exitButton = transform.Find("ExitButton")?.GetComponent<Button>();
        }

        private void Start()
        {
            if (hintText != null) hintText.text = "P1: WASD + Space 슛   P2: 방향키 + Enter/RightCtrl 슛   R: 재시작   ESC: 선택";
            if (exitButton != null) exitButton.onClick.AddListener(SceneLoader.LoadGameSelect);
        }

        public void SetMatch(float timeRemaining, int p1Score, int p2Score)
        {
            if (timeText != null) timeText.text = $"남은 시간 {Mathf.CeilToInt(timeRemaining)}";
            if (scoreText != null) scoreText.text = $"P1 {p1Score} : {p2Score} P2";
        }
    }
}
