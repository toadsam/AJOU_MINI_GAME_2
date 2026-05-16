using AjouFestival.Core;
using UnityEngine;

namespace AjouFestival.Games.BalanceWalk
{
    public sealed class BalanceWalkGameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BalancePlayerController player;
        [SerializeField] private BalanceUI ui;

        [Header("Flow")]
        [SerializeField] private float countdownDuration = 3f;

        [Header("Scoring")]
        [SerializeField] private float scorePerSecond = 10f;

        [Header("Text")]
        [SerializeField] private string countdownStartMessage = "\uC2DC\uC791!";
        [SerializeField] private string resultMessageFormat = "\uADE0\uD615\uC744 \uC783\uC5C8\uC2B5\uB2C8\uB2E4. \uCD5C\uC885 \uAC01\uB3C4 {0:0}\uB3C4";
        [SerializeField] private string resultScoreTextFormat = "{0:0.0}\uCD08 \uBC84\uD300";

        public bool IsGameOver { get; private set; }
        public bool HasStarted { get; private set; }
        public float CountdownRemaining { get; private set; }
        public float ElapsedTime { get; private set; }
        public float DistanceMeters { get; private set; }
        public float Difficulty { get; private set; } = 1f;

        private float startX;

        private void Awake()
        {
            if (player == null) player = FindFirstObjectByType<BalancePlayerController>();
            if (ui == null) ui = FindFirstObjectByType<BalanceUI>();
        }

        private void Start()
        {
            GameSessionManager.Ensure().StartGame(GameType.BalanceWalk, SceneLoader.BalanceWalkScene);
            CountdownRemaining = countdownDuration;
            HasStarted = countdownDuration <= 0f;
            startX = player != null ? player.transform.position.x : 0f;

            if (player != null) player.Initialize(this);
            if (ui != null)
            {
                ui.SetBestTime(ScoreRecordManager.GetBestScore(GameType.BalanceWalk), scorePerSecond);
                ui.SetSurvivalTime(0f);
                ui.SetCountdown(Mathf.CeilToInt(CountdownRemaining).ToString());
            }
        }

        private void Update()
        {
            if (IsGameOver)
            {
                return;
            }

            if (!HasStarted)
            {
                CountdownRemaining -= Time.deltaTime;
                if (CountdownRemaining > 0f)
                {
                    ui?.SetCountdown(Mathf.CeilToInt(CountdownRemaining).ToString());
                    return;
                }

                HasStarted = true;
                ui?.SetCountdown(countdownStartMessage, true);
                return;
            }

            ui?.HideCountdown();
            ElapsedTime += Time.deltaTime;
            DistanceMeters = player != null ? Mathf.Max(0f, player.transform.position.x - startX) : 0f;
            Difficulty = 1f + DistanceMeters * 0.015f;
            ui?.SetSurvivalTime(ElapsedTime);
        }

        public void GameOver(float angle)
        {
            if (IsGameOver)
            {
                return;
            }

            IsGameOver = true;
            int score = Mathf.FloorToInt(ElapsedTime * scorePerSecond);
            string resultMessage = string.Format(resultMessageFormat, angle);
            string resultScoreText = string.Format(resultScoreTextFormat, ElapsedTime);
            GameSessionManager.Ensure().SetResult(score, resultMessage, resultScoreText);
            SceneLoader.LoadResult();
        }
    }
}
