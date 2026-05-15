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
            if (hintText != null)
            {
                hintText.text = "P1 A/D move, W jump, Space or S kick   P2 Left/Right move, Up jump, Enter, Ctrl or Down kick   R restart   ESC menu";
            }

            if (exitButton != null) exitButton.onClick.AddListener(SceneLoader.LoadGameSelect);
        }

        public void SetMatch(float timeRemaining, int p1Score, int p2Score)
        {
            if (timeText != null) timeText.text = $"Time {Mathf.CeilToInt(timeRemaining)}";
            if (scoreText != null) scoreText.text = $"P1 {p1Score} : {p2Score} P2";
        }
    }
}
