using AjouFestival.Core;
using UnityEngine;

namespace AjouFestival.Games.Soccer
{
    public sealed class SoccerGameManager : MonoBehaviour
    {
        [SerializeField] private SoccerPlayerController player1;
        [SerializeField] private SoccerPlayerController player2;
        [SerializeField] private SoccerBallController ball;
        [SerializeField] private SoccerUI ui;
        [SerializeField] private float matchDuration = 60f;

        public int Player1Score { get; private set; }
        public int Player2Score { get; private set; }
        public float TimeRemaining { get; private set; }
        public bool IsFinished { get; private set; }

        private Vector3 player1Start;
        private Vector3 player2Start;

        private void Awake()
        {
            if (player1 == null || player2 == null)
            {
                SoccerPlayerController[] players = FindObjectsByType<SoccerPlayerController>(FindObjectsSortMode.None);
                foreach (SoccerPlayerController player in players)
                {
                    if (player.PlayerIndex == 1) player1 = player;
                    if (player.PlayerIndex == 2) player2 = player;
                }
            }

            if (ball == null) ball = FindFirstObjectByType<SoccerBallController>();
            if (ui == null) ui = FindFirstObjectByType<SoccerUI>();
        }

        private void Start()
        {
            GameSessionManager.Ensure().StartGame(GameType.Soccer, SceneLoader.SoccerScene);
            TimeRemaining = matchDuration;
            if (player1 != null) player1Start = player1.transform.position;
            if (player2 != null) player2Start = player2.transform.position;
            if (player1 != null) player1.Initialize(this, ball);
            if (player2 != null) player2.Initialize(this, ball);
            UpdateUI();
        }

        private void Update()
        {
            if (IsFinished)
            {
                return;
            }

            TimeRemaining -= Time.deltaTime;
            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                FinishMatch();
            }

            UpdateUI();
        }

        public void AddGoal(int scoringPlayer)
        {
            if (IsFinished)
            {
                return;
            }

            if (scoringPlayer == 1) Player1Score++;
            if (scoringPlayer == 2) Player2Score++;
            ResetPositions();
            UpdateUI();
        }

        private void ResetPositions()
        {
            if (player1 != null) player1.ResetPosition(player1Start);
            if (player2 != null) player2.ResetPosition(player2Start);
            if (ball != null) ball.ResetPosition(Vector3.zero);
        }

        private void FinishMatch()
        {
            IsFinished = true;
            string result = Player1Score == Player2Score
                ? "무승부!"
                : Player1Score > Player2Score ? "Player 1 승리!" : "Player 2 승리!";
            int winnerScore = Mathf.Max(Player1Score, Player2Score);
            GameSessionManager.Ensure().SetResult(winnerScore, result, $"P1 {Player1Score} : {Player2Score} P2");
            SceneLoader.LoadResult();
        }

        private void UpdateUI()
        {
            if (ui != null)
            {
                ui.SetMatch(TimeRemaining, Player1Score, Player2Score);
            }
        }
    }
}
