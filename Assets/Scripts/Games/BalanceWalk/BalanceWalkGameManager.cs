using AjouFestival.Core;
using UnityEngine;

namespace AjouFestival.Games.BalanceWalk
{
    public sealed class BalanceWalkGameManager : MonoBehaviour
    {
        [SerializeField] private BalancePlayerController player;
        [SerializeField] private BalanceUI ui;
        [SerializeField] private float countdownDuration = 3f;
        [SerializeField] private float scorePerMeter = 10f;

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
                ui.SetBestDistance(ScoreRecordManager.GetBestScore(GameType.BalanceWalk), scorePerMeter);
                ui.SetDistance(0f);
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
                ui?.SetCountdown("Start!");
                return;
            }

            ui?.HideCountdown();
            ElapsedTime += Time.deltaTime;
            DistanceMeters = player != null ? Mathf.Max(0f, player.transform.position.x - startX) : 0f;
            Difficulty = 1f + DistanceMeters * 0.015f;
            ui?.SetDistance(DistanceMeters);
        }

        public void GameOver(float angle)
        {
            if (IsGameOver)
            {
                return;
            }

            IsGameOver = true;
            int score = Mathf.FloorToInt(DistanceMeters * scorePerMeter);
            GameSessionManager.Ensure().SetResult(score, $"균형을 잃었습니다. 최종 각도 {angle:0}도", $"{DistanceMeters:0.0} m");
            SceneLoader.LoadResult();
        }
    }
}
