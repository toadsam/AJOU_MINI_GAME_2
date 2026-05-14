using AjouBuntu.Config;
using AjouBuntu.Player;
using AjouBuntu.UI;
using AjouBuntu.World;
using UnityEngine;

namespace AjouBuntu.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        public float CurrentSpeed { get; private set; }
        public float Distance { get; private set; }
        public float DistanceRemaining => Mathf.Max(0f, config.targetDistance - Distance);
        public int DifficultyStage { get; private set; } = 1;
        public bool IsFinished { get; private set; }

        private float elapsedMs;
        private PlayerController player;
        private ScoreManager score;
        private UIManager ui;
        private PlatformSpawner platformSpawner;
        private BackgroundManager background;

        public void Initialize(
            GameConfig gameConfig,
            PlayerController playerController,
            ScoreManager scoreManager,
            UIManager uiManager,
            PlatformSpawner spawner,
            BackgroundManager backgroundManager)
        {
            config = gameConfig;
            player = playerController;
            score = scoreManager;
            ui = uiManager;
            platformSpawner = spawner;
            background = backgroundManager;
            CurrentSpeed = config.initialSpeed;
            Distance = 0f;
            elapsedMs = 0f;
            DifficultyStage = 1;
            IsFinished = false;
            ui?.SetDistance(DistanceRemaining, config.targetDistance, 0f);
        }

        private void Update()
        {
            if (config == null || IsFinished)
            {
                return;
            }

            elapsedMs += Time.deltaTime * 1000f;
            CurrentSpeed = Mathf.Min(config.maxSpeed, config.initialSpeed + (elapsedMs / 900f) * config.speedIncreasePer900Ms);
            Distance += CurrentSpeed * Time.deltaTime;

            float progress = Mathf.Clamp01(Distance / config.targetDistance);
            int nextDifficulty = Mathf.Clamp(Mathf.FloorToInt(Mathf.InverseLerp(config.initialSpeed, config.maxSpeed, CurrentSpeed) * 6f) + 1, 1, 6);
            if (nextDifficulty != DifficultyStage)
            {
                DifficultyStage = nextDifficulty;
                ui?.ShowDifficulty(DifficultyStage);
            }

            ui?.SetDistance(DistanceRemaining, config.targetDistance, progress);
            background?.SetProgress(progress, CurrentSpeed);
            platformSpawner?.SetDifficulty(DifficultyStage);

            if (player != null && config.ScreenYFromWorldY(player.transform.position.y) > config.deathLineScreenY)
            {
                Finish(false);
                return;
            }

            if (Distance >= config.targetDistance)
            {
                Finish(true);
            }
        }

        private void Finish(bool cleared)
        {
            if (IsFinished)
            {
                return;
            }

            IsFinished = true;
            int highScore = score != null ? score.CommitHighScore() : 0;
            GameSessionResult.Set(cleared, score != null ? score.Score : 0, highScore, score != null ? score.APlusCount : 0);
            SceneTransitionManager.LoadGameOver();
        }
    }
}
