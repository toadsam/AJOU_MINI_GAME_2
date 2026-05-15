using AjouFestival.Core;
using UnityEngine;

namespace AjouFestival.Games.AjouBoontu
{
    public sealed class AjouBoontuGameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ChitoRunnerController runner;
        [SerializeField] private RunnerUI ui;

        [Header("Scoring")]
        [SerializeField] private float scorePerSecond = 10f;
        [SerializeField] private int itemScore = 100;

        [Header("Pacing")]
        [SerializeField, Min(0f)] private float initialRunSpeed = 5.8f;
        [SerializeField, Min(0f)] private float speedIncreaseIntervalSeconds = 8f;
        [SerializeField, Min(0f)] private float speedIncreaseAmount = 0.5f;
        [SerializeField, Min(1f)] private float clearDistance = 120f;

        public bool IsGameOver { get; private set; }
        public int Score { get; private set; }

        private float scoreFloat;
        private float elapsedTime;
        private float nextSpeedIncreaseTime;
        private float runStartX;

        private void Awake()
        {
            if (runner == null) runner = FindFirstObjectByType<ChitoRunnerController>();
            if (ui == null) ui = FindFirstObjectByType<RunnerUI>();
        }

        private void Start()
        {
            GameSessionManager.Ensure().StartGame(GameType.AjouBoontu, SceneLoader.AjouBoontuScene);

            if (runner != null)
            {
                runner.Initialize(this);
                runner.SetRunSpeed(initialRunSpeed);
                runStartX = runner.transform.position.x;
            }

            nextSpeedIncreaseTime = speedIncreaseIntervalSeconds > 0f ? speedIncreaseIntervalSeconds : float.PositiveInfinity;

            if (ui != null)
            {
                ui.SetBestScore(ScoreRecordManager.GetBestScore(GameType.AjouBoontu));
                ui.SetScore(Score);
                ui.SetRemainingDistance(clearDistance);
            }
        }

        private void Update()
        {
            if (IsGameOver)
            {
                return;
            }

            elapsedTime += Time.deltaTime;
            UpdateRunSpeed();

            scoreFloat += scorePerSecond * Time.deltaTime;
            Score = Mathf.FloorToInt(scoreFloat);

            float remainingDistance = GetRemainingDistance();
            if (ui != null)
            {
                ui.SetScore(Score);
                ui.SetRemainingDistance(remainingDistance);
            }

            if (remainingDistance <= 0f)
            {
                ClearRun();
            }
        }

        public void AddItemScore(int amount)
        {
            if (IsGameOver)
            {
                return;
            }

            scoreFloat += amount <= 0 ? itemScore : amount;
            Score = Mathf.FloorToInt(scoreFloat);
            if (ui != null) ui.SetScore(Score);
        }

        public void GameOver(string reason)
        {
            if (IsGameOver)
            {
                return;
            }

            IsGameOver = true;
            GameSessionManager.Ensure().SetResult(Score, string.IsNullOrWhiteSpace(reason) ? "Run ended." : reason);
            SceneLoader.LoadResult();
        }

        private void ClearRun()
        {
            if (IsGameOver)
            {
                return;
            }

            IsGameOver = true;
            GameSessionManager.Ensure().SetResult(Score, "Reached the finish line.", $"Score {Score:N0}");
            SceneLoader.LoadResult();
        }

        private void UpdateRunSpeed()
        {
            if (runner == null || speedIncreaseIntervalSeconds <= 0f || speedIncreaseAmount <= 0f)
            {
                return;
            }

            while (elapsedTime >= nextSpeedIncreaseTime)
            {
                runner.SetRunSpeed(runner.RunSpeed + speedIncreaseAmount);
                nextSpeedIncreaseTime += speedIncreaseIntervalSeconds;
            }
        }

        private float GetRemainingDistance()
        {
            if (runner == null)
            {
                return clearDistance;
            }

            float travelledDistance = Mathf.Max(0f, runner.transform.position.x - runStartX);
            return Mathf.Max(0f, clearDistance - travelledDistance);
        }
    }
}
