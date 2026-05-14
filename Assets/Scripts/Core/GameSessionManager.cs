using UnityEngine;

namespace AjouFestival.Core
{
    public sealed class GameSessionManager : MonoBehaviour
    {
        public static GameSessionManager Instance { get; private set; }

        [SerializeField] private GameType currentGameType = GameType.None;
        [SerializeField] private int lastScore;
        [SerializeField] private string lastScoreText = "0";
        [SerializeField] private string lastResultMessage = "";
        [SerializeField] private string lastPlayedSceneName = "";

        public GameType CurrentGameType => currentGameType;
        public int LastScore => lastScore;
        public string LastScoreText => string.IsNullOrWhiteSpace(lastScoreText) ? lastScore.ToString() : lastScoreText;
        public string LastResultMessage => lastResultMessage;
        public string LastPlayedSceneName => lastPlayedSceneName;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static GameSessionManager Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject obj = new GameObject("GameSessionManager");
            return obj.AddComponent<GameSessionManager>();
        }

        public void StartGame(GameType type, string sceneName)
        {
            currentGameType = type;
            lastPlayedSceneName = sceneName;
            lastScore = 0;
            lastScoreText = "0";
            lastResultMessage = "";
        }

        public void SetResult(int score, string resultMessage, string scoreText = null)
        {
            lastScore = score;
            lastScoreText = string.IsNullOrWhiteSpace(scoreText) ? score.ToString() : scoreText;
            lastResultMessage = resultMessage;
            ScoreRecordManager.SetBestScore(currentGameType, score);
        }
    }
}
