using AjouFestival.Core;
using UnityEngine;

namespace AjouFestival.Games.Soccer
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class SoccerPlayerController : MonoBehaviour
    {
        [SerializeField] private int playerIndex = 1;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite playerSprite;
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float kickForce = 11f;
        [SerializeField] private float kickRange = 1.25f;

        public int PlayerIndex => playerIndex;

        private Rigidbody2D body;
        private SoccerGameManager gameManager;
        private SoccerBallController ball;

        public void Initialize(SoccerGameManager manager, SoccerBallController soccerBall)
        {
            gameManager = manager;
            ball = soccerBall;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.linearDamping = 2.5f;
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null && playerSprite != null) spriteRenderer.sprite = playerSprite;
        }

        private void FixedUpdate()
        {
            if (gameManager != null && gameManager.IsFinished)
            {
                body.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 input = playerIndex == 1 ? FestivalInput.MoveWasd() : FestivalInput.MoveArrows();
            body.linearVelocity = input * moveSpeed;

            if (input.sqrMagnitude > 0.001f)
            {
                transform.up = input;
            }
        }

        private void Update()
        {
            bool kickPressed = playerIndex == 1
                ? FestivalInput.GetKeyDown(KeyCode.Space)
                : FestivalInput.GetKeyDown(KeyCode.Return) || FestivalInput.GetKeyDown(KeyCode.KeypadEnter) || FestivalInput.GetKeyDown(KeyCode.RightControl);

            if (kickPressed)
            {
                TryKick();
            }
        }

        public void ResetPosition(Vector3 position)
        {
            transform.position = position;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        private void TryKick()
        {
            if (ball == null)
            {
                ball = FindFirstObjectByType<SoccerBallController>();
            }

            if (ball == null)
            {
                return;
            }

            Vector2 toBall = ball.transform.position - transform.position;
            if (toBall.magnitude > kickRange)
            {
                return;
            }

            ball.Kick(toBall.normalized * kickForce);
        }
    }
}
