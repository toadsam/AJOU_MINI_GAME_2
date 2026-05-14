using AjouFestival.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AjouFestival.UI
{
    public sealed class ResultUI : MonoBehaviour
    {
        [SerializeField] private Text gameNameText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Text resultMessageText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button gameSelectButton;
        [SerializeField] private Button mainMenuButton;

        private void Awake()
        {
            if (gameNameText == null) gameNameText = transform.Find("GameNameText")?.GetComponent<Text>();
            if (scoreText == null) scoreText = transform.Find("ScoreText")?.GetComponent<Text>();
            if (bestScoreText == null) bestScoreText = transform.Find("BestScoreText")?.GetComponent<Text>();
            if (resultMessageText == null) resultMessageText = transform.Find("ResultMessageText")?.GetComponent<Text>();
            if (retryButton == null) retryButton = transform.Find("RetryButton")?.GetComponent<Button>();
            if (gameSelectButton == null) gameSelectButton = transform.Find("GameSelectButton")?.GetComponent<Button>();
            if (mainMenuButton == null) mainMenuButton = transform.Find("MainMenuButton")?.GetComponent<Button>();
        }

        private void Start()
        {
            GameSessionManager session = GameSessionManager.Ensure();
            GameType type = session.CurrentGameType;

            if (gameNameText != null) gameNameText.text = GetGameName(type);
            if (scoreText != null) scoreText.text = $"최종 점수: {session.LastScoreText}";
            if (bestScoreText != null) bestScoreText.text = $"최고 점수: {ScoreRecordManager.GetBestScore(type):N0}";
            if (resultMessageText != null) resultMessageText.text = string.IsNullOrWhiteSpace(session.LastResultMessage) ? "플레이 결과" : session.LastResultMessage;

            if (retryButton != null) retryButton.onClick.AddListener(SceneLoader.RestartLastGame);
            if (gameSelectButton != null) gameSelectButton.onClick.AddListener(SceneLoader.LoadGameSelect);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(SceneLoader.LoadMainMenu);
        }

        private static string GetGameName(GameType type)
        {
            return type switch
            {
                GameType.AjouBoontu => "아주분투",
                GameType.BalanceWalk => "치토 균형걷기",
                GameType.Soccer => "아주 1대1 축구",
                _ => "미니게임"
            };
        }
    }
}
