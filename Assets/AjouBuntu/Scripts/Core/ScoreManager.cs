using AjouBuntu.Config;
using AjouBuntu.UI;
using UnityEngine;

namespace AjouBuntu.Core
{
    public sealed class ScoreManager : MonoBehaviour
    {
        private const string HighScoreKey = "AjouBuntu.HighScore";

        [SerializeField] private GameConfig config;

        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public int APlusCount { get; private set; }

        private UIManager ui;
        private float scoreRemainder;

        public void Initialize(GameConfig gameConfig, UIManager uiManager)
        {
            config = gameConfig;
            ui = uiManager;
            Score = 0;
            APlusCount = 0;
            scoreRemainder = 0f;
            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
            ui?.SetScore(Score, HighScore, APlusCount);
        }

        private void Update()
        {
            if (config == null)
            {
                return;
            }

            scoreRemainder += config.scorePerSecond * Time.deltaTime;
            if (scoreRemainder >= 1f)
            {
                int add = Mathf.FloorToInt(scoreRemainder);
                scoreRemainder -= add;
                AddScore(add, false, Vector3.zero);
            }
        }

        public void AddItem(ItemDefinition item, Vector3 worldPosition)
        {
            if (item == null)
            {
                return;
            }

            AddScore(item.score, true, worldPosition);
            if (item.kind == ItemKind.APlus)
            {
                APlusCount++;
            }

            ui?.SetScore(Score, HighScore, APlusCount);
        }

        public int CommitHighScore()
        {
            if (Score > HighScore)
            {
                HighScore = Score;
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
                PlayerPrefs.Save();
            }

            return HighScore;
        }

        private void AddScore(int amount, bool showFloatingText, Vector3 worldPosition)
        {
            Score += amount;
            if (Score > HighScore)
            {
                HighScore = Score;
            }

            ui?.SetScore(Score, HighScore, APlusCount);
            if (showFloatingText)
            {
                FloatingText.Spawn($"+{amount}", worldPosition + new Vector3(0f, 44f, 0f));
            }
        }
    }
}
