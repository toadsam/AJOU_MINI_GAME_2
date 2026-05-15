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
        [SerializeField] private Vector2 player1Spawn = new Vector2(-4.6f, -3.2f);
        [SerializeField] private Vector2 player2Spawn = new Vector2(4.6f, -3.2f);
        [SerializeField] private Vector2 ballSpawn = new Vector2(0f, -2.55f);
        [SerializeField] private Vector2 leftGoalPosition = new Vector2(-7.1f, -3.25f);
        [SerializeField] private Vector2 rightGoalPosition = new Vector2(7.1f, -3.25f);
        [SerializeField] private Vector2 goalTriggerSize = new Vector2(0.9f, 2f);
        [SerializeField] private Vector2 cameraCenter = new Vector2(0f, -0.35f);
        [SerializeField] private float cameraSize = 5f;

        public int Player1Score { get; private set; }
        public int Player2Score { get; private set; }
        public float TimeRemaining { get; private set; }
        public bool IsFinished { get; private set; }

        private Vector3 player1Start;
        private Vector3 player2Start;
        private Vector3 ballStart;

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
            ConfigureArena();
            TimeRemaining = matchDuration;

            if (player1 != null) player1.Initialize(this, ball);
            if (player2 != null) player2.Initialize(this, ball);

            ResetPositions();
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
            if (ball != null) ball.ResetPosition(ballStart);
        }

        private void FinishMatch()
        {
            IsFinished = true;

            string result = Player1Score == Player2Score
                ? "Draw!"
                : Player1Score > Player2Score ? "Player 1 Wins!" : "Player 2 Wins!";

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

        private void ConfigureArena()
        {
            player1Start = new Vector3(player1Spawn.x, player1Spawn.y, 0f);
            player2Start = new Vector3(player2Spawn.x, player2Spawn.y, 0f);
            ballStart = new Vector3(ballSpawn.x, ballSpawn.y, 0f);

            ConfigureGoal("SoccerGoalLeft", leftGoalPosition);
            ConfigureGoal("SoccerGoalRight", rightGoalPosition);

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = cameraSize;
                mainCamera.transform.position = new Vector3(cameraCenter.x, cameraCenter.y, mainCamera.transform.position.z);
            }
        }

        private void ConfigureGoal(string goalName, Vector2 position)
        {
            GameObject goalObject = GameObject.Find(goalName);
            if (goalObject == null)
            {
                return;
            }

            goalObject.transform.position = new Vector3(position.x, position.y, goalObject.transform.position.z);

            BoxCollider2D goalCollider = goalObject.GetComponent<BoxCollider2D>();
            if (goalCollider != null)
            {
                goalCollider.isTrigger = true;
                goalCollider.offset = Vector2.zero;
                goalCollider.size = goalTriggerSize;
            }
        }
    }
}
