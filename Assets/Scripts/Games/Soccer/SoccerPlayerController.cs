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

        private enum ControlMode
        {
            Human,
            AI
        }

        private readonly struct AIProfile
        {
            public AIProfile(float reactionInterval, float predictionTime, float moveDeadZone, float attackOffset, float jumpAccuracy, float kickAccuracy)
            {
                ReactionInterval = reactionInterval;
                PredictionTime = predictionTime;
                MoveDeadZone = moveDeadZone;
                AttackOffset = attackOffset;
                JumpAccuracy = jumpAccuracy;
                KickAccuracy = kickAccuracy;
            }

            public float ReactionInterval { get; }
            public float PredictionTime { get; }
            public float MoveDeadZone { get; }
            public float AttackOffset { get; }
            public float JumpAccuracy { get; }
            public float KickAccuracy { get; }
        }

        private static PhysicsMaterial2D sharedPlayerMaterial;

        private readonly RaycastHit2D[] groundHits = new RaycastHit2D[6];

        private Rigidbody2D body;
        private Collider2D hitbox;
        private SoccerGameManager gameManager;
        private SoccerBallController ball;
        private ControlMode controlMode = ControlMode.Human;
        private SoccerAIDifficulty aiDifficulty = SoccerAIDifficulty.Medium;
        private bool jumpQueued;
        private bool isGrounded;
        private float facingDirection;
        private float aiMoveInput;
        private float aiDecisionTimer;
        private float aiKickCooldownTimer;

        public void Initialize(SoccerGameManager manager, SoccerBallController soccerBall)
        {
            gameManager = manager;
            ball = soccerBall;
        }

        public void SetHumanControl()
        {
            controlMode = ControlMode.Human;
            aiMoveInput = 0f;
            aiDecisionTimer = 0f;
            aiKickCooldownTimer = 0f;
        }

        public void SetAIControl(SoccerAIDifficulty difficulty)
        {
            controlMode = ControlMode.AI;
            aiDifficulty = difficulty;
            aiMoveInput = 0f;
            aiDecisionTimer = 0f;
            aiKickCooldownTimer = 0f;
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

            if (gameManager != null && !gameManager.IsMatchActive)
            {
                body.linearVelocity = Vector2.zero;
                jumpQueued = false;
                aiMoveInput = 0f;
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
            if (gameManager != null && !gameManager.IsMatchActive)
            {
                return;
            }

            if (controlMode == ControlMode.AI)
            {
                UpdateAI();
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
            aiMoveInput = 0f;
            aiDecisionTimer = 0f;
            aiKickCooldownTimer = 0f;
            facingDirection = position.x <= 0f ? 1f : -1f;
            ApplyFacing();
        }

        private void UpdateAI()
        {
            if (ball == null)
            {
                ball = FindFirstObjectByType<SoccerBallController>();
                if (ball == null)
                {
                    return;
                }
            }

            aiDecisionTimer -= Time.deltaTime;
            aiKickCooldownTimer -= Time.deltaTime;

            if (aiDecisionTimer > 0f)
            {
                return;
            }

            AIProfile profile = GetAIProfile(aiDifficulty);
            aiDecisionTimer = profile.ReactionInterval;

            EvaluateAI(profile, out aiMoveInput, out bool jumpPressed, out bool kickPressed);

            if (jumpPressed)
            {
                jumpQueued = true;
            }

            if (kickPressed && aiKickCooldownTimer <= 0f && TryKick())
            {
                aiKickCooldownTimer = profile.ReactionInterval + 0.08f;
            }
        }

        private void EvaluateAI(AIProfile profile, out float moveInput, out bool jumpPressed, out bool kickPressed)
        {
            Vector2 ballPosition = ball.transform.position;
            Vector2 ballVelocity = ball.Velocity;
            Vector2 playerPosition = transform.position;

            float homeSide = playerIndex == 1 ? -1f : 1f;
            float orientedBallX = ballPosition.x * homeSide;
            float orientedBallVelocityX = ballVelocity.x * homeSide;
            float predictedX = ballPosition.x + ballVelocity.x * profile.PredictionTime;
            float orientedPredictedX = predictedX * homeSide;

            bool defend = orientedBallX > -0.2f || orientedBallVelocityX > 1f;
            float targetOrientedX = defend
                ? Mathf.Clamp(Mathf.Max(orientedPredictedX, 1.8f), 1.8f, 6.2f)
                : Mathf.Clamp(orientedPredictedX + profile.AttackOffset, -5.5f, 5.8f);

            float targetX = targetOrientedX * homeSide;
            float deltaX = targetX - playerPosition.x;
            moveInput = Mathf.Abs(deltaX) <= profile.MoveDeadZone ? 0f : Mathf.Sign(deltaX);

            float xDistanceToBall = Mathf.Abs(ballPosition.x - playerPosition.x);
            bool closeToBall = xDistanceToBall < 1.05f;
            bool ballAboveHead = ballPosition.y > playerPosition.y + 0.45f;
            bool goalDanger = defend && orientedBallX > 1.2f && ballPosition.y > -3.35f;

            jumpPressed = isGrounded
                && closeToBall
                && (ballAboveHead || goalDanger)
                && Random.value <= profile.JumpAccuracy;

            Vector2 toBall = ballPosition - playerPosition;
            bool closeEnoughToKick = toBall.magnitude <= kickRange;
            float attackDirection = playerIndex == 1 ? 1f : -1f;
            bool ballInAttackDirection = Mathf.Abs(toBall.x) < 0.1f || Mathf.Sign(toBall.x) == attackDirection;

            float kickChance = goalDanger ? 1f : profile.KickAccuracy;
            if (!ballInAttackDirection)
            {
                kickChance *= 0.55f;
            }

            kickPressed = closeEnoughToKick
                && (ballInAttackDirection || goalDanger)
                && Random.value <= kickChance;
        }

        private bool TryKick()
        {
            if (ball == null)
            {
                ball = FindFirstObjectByType<SoccerBallController>();
            }

            if (ball == null)
            {
                return false;
            }

            Vector2 toBall = ball.transform.position - transform.position;
            if (toBall.magnitude > kickRange)
            {
                return false;
            }

            float kickDirection = Mathf.Abs(toBall.x) > 0.1f ? Mathf.Sign(toBall.x) : facingDirection;
            float lift = kickLift + Mathf.Clamp(toBall.y * 2.25f, -0.5f, 2f);
            ball.Kick(new Vector2(kickDirection * kickForce, lift));
            return true;
        }

        private float GetMoveInput()
        {
            if (controlMode == ControlMode.AI)
            {
                return aiMoveInput;
            }

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

        private static AIProfile GetAIProfile(SoccerAIDifficulty difficulty)
        {
            return difficulty switch
            {
                SoccerAIDifficulty.Easy => new AIProfile(0.24f, 0.2f, 0.42f, 0.85f, 0.55f, 0.62f),
                SoccerAIDifficulty.Medium => new AIProfile(0.12f, 0.42f, 0.22f, 0.55f, 0.8f, 0.86f),
                SoccerAIDifficulty.Hard => new AIProfile(0.05f, 0.75f, 0.1f, 0.32f, 0.97f, 1f),
                _ => new AIProfile(0.12f, 0.42f, 0.22f, 0.55f, 0.8f, 0.86f)
            };
        }
    }
}
