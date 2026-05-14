using AjouFestival.Core;
using UnityEngine;

namespace AjouFestival.Games.AjouBoontu
{
    public sealed class AjouBoontuGameManager : MonoBehaviour
    {
        [SerializeField] private ChitoRunnerController runner;
        [SerializeField] private RunnerUI ui;
        [SerializeField] private float scorePerSecond = 10f;
        [SerializeField] private int itemScore = 100;

        public bool IsGameOver { get; private set; }
        public int Score { get; private set; }

        private float scoreFloat;

        private void Awake()
        {
            if (runner == null) runner = FindFirstObjectByType<ChitoRunnerController>();
            if (ui == null) ui = FindFirstObjectByType<RunnerUI>();
        }

        private void Start()
        {
            GameSessionManager.Ensure().StartGame(GameType.AjouBoontu, SceneLoader.AjouBoontuScene);
            if (runner != null) runner.Initialize(this);
            if (ui != null) ui.SetBestScore(ScoreRecordManager.GetBestScore(GameType.AjouBoontu));
        }

        private void Update()
        {
            if (IsGameOver)
            {
                return;
            }

            scoreFloat += scorePerSecond * Time.deltaTime;
            Score = Mathf.FloorToInt(scoreFloat);
            if (ui != null) ui.SetScore(Score);
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
            GameSessionManager.Ensure().SetResult(Score, string.IsNullOrWhiteSpace(reason) ? "캠퍼스 질주 종료!" : reason);
            SceneLoader.LoadResult();
        }
    }
}
