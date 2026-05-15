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
        [SerializeField] private float moveSpeed = 7.25f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float airControl = 0.9f;
        [SerializeField] private float gravityScale = 3.2f;
        [SerializeField] private float kickForce = 9.5f;
        [SerializeField] private float kickLift = 4.75f;
        [SerializeField] private float kickRange = 1.7f;
        [SerializeField] private float groundCheckDistance = 0.14f;
        [SerializeField] private float maxFallSpeed = 18f;

        public int PlayerIndex => playerIndex;

        private static PhysicsMaterial2D sharedPlayerMaterial;

        private readonly RaycastHit2D[] groundHits = new RaycastHit2D[6];

        private Rigidbody2D body;
        private Collider2D hitbox;
        private SoccerGameManager gameManager;
        private SoccerBallController ball;
        private bool jumpQueued;
        private bool isGrounded;
        private float facingDirection;

        public void Initialize(SoccerGameManager manager, SoccerBallController soccerBall)
        {
            gameManager = manager;
            ball = soccerBall;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            hitbox = GetComponent<Collider2D>();

            body.gravityScale = gravityScale;
            body.freezeRotation = true;
            body.linearDamping = 0.15f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (hitbox != null && hitbox.sharedMaterial == null)
            {
                sharedPlayerMaterial ??= new PhysicsMaterial2D("RuntimeSoccerPlayer")
                {
                    bounciness = 0.05f,
                    friction = 0.35f
                };
                hitbox.sharedMaterial = sharedPlayerMaterial;
            }

            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null && playerSprite != null) spriteRenderer.sprite = playerSprite;

            facingDirection = playerIndex == 1 ? 1f : -1f;
            ApplyFacing();
        }

        private void FixedUpdate()
        {
            UpdateGroundedState();

            if (gameManager != null && gameManager.IsFinished)
            {
                body.linearVelocity = Vector2.zero;
                jumpQueued = false;
                return;
            }

            float moveInput = GetMoveInput();
            if (Mathf.Abs(moveInput) > 0.01f)
            {
                facingDirection = Mathf.Sign(moveInput);
                ApplyFacing();
            }

            Vector2 velocity = body.linearVelocity;
            velocity.x = moveInput * moveSpeed * (isGrounded ? 1f : airControl);
            if (velocity.y < -maxFallSpeed)
            {
                velocity.y = -maxFallSpeed;
            }

            if (jumpQueued && isGrounded)
            {
                velocity.y = jumpForce;
                isGrounded = false;
            }

            body.linearVelocity = velocity;
            jumpQueued = false;
        }

        private void Update()
        {
            if (gameManager != null && gameManager.IsFinished)
            {
                return;
            }

            if (GetJumpPressed())
            {
                jumpQueued = true;
            }

            if (GetKickPressed())
            {
                TryKick();
            }
        }

        public void ResetPosition(Vector3 position)
        {
            transform.position = position;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            jumpQueued = false;
            isGrounded = false;
            facingDirection = position.x <= 0f ? 1f : -1f;
            ApplyFacing();
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

            float kickDirection = Mathf.Abs(toBall.x) > 0.1f ? Mathf.Sign(toBall.x) : facingDirection;
            float lift = kickLift + Mathf.Clamp(toBall.y * 2.25f, -0.5f, 2f);
            ball.Kick(new Vector2(kickDirection * kickForce, lift));
        }

        private float GetMoveInput()
        {
            float input = 0f;

            if (playerIndex == 1)
            {
                if (FestivalInput.GetKey(KeyCode.A)) input -= 1f;
                if (FestivalInput.GetKey(KeyCode.D)) input += 1f;
            }
            else
            {
                if (FestivalInput.GetKey(KeyCode.LeftArrow)) input -= 1f;
                if (FestivalInput.GetKey(KeyCode.RightArrow)) input += 1f;
            }

            return Mathf.Clamp(input, -1f, 1f);
        }

        private bool GetJumpPressed()
        {
            return playerIndex == 1
                ? FestivalInput.GetKeyDown(KeyCode.W)
                : FestivalInput.GetKeyDown(KeyCode.UpArrow);
        }

        private bool GetKickPressed()
        {
            if (playerIndex == 1)
            {
                return FestivalInput.GetKeyDown(KeyCode.Space) || FestivalInput.GetKeyDown(KeyCode.S);
            }

            return FestivalInput.GetKeyDown(KeyCode.Return)
                || FestivalInput.GetKeyDown(KeyCode.KeypadEnter)
                || FestivalInput.GetKeyDown(KeyCode.RightControl)
                || FestivalInput.GetKeyDown(KeyCode.DownArrow);
        }

        private void UpdateGroundedState()
        {
            if (hitbox == null)
            {
                isGrounded = false;
                return;
            }

            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = false;

            int hitCount = hitbox.Cast(Vector2.down, filter, groundHits, groundCheckDistance);
            isGrounded = false;

            for (int i = 0; i < hitCount; i++)
            {
                if (groundHits[i].normal.y > 0.2f)
                {
                    isGrounded = true;
                    return;
                }
            }
        }

        private void ApplyFacing()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = facingDirection < 0f;
            }
        }
    }
}
