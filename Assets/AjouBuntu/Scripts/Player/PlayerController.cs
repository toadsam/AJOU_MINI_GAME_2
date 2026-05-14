using AjouBuntu.Config;
using AjouBuntu.Core;
using UnityEngine;

namespace AjouBuntu.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(PlayerAnimationController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private LayerMask groundMask;

        private Rigidbody2D body;
        private BoxCollider2D bodyCollider;
        private PlayerAnimationController animationController;
        private InputManager input;
        private int airJumpsUsed;
        private bool grounded;
        private float lastGroundedTime;
        private float landingUntil;
        private bool wasGrounded;

        public void Initialize(GameConfig gameConfig, InputManager inputManager, LayerMask platformMask)
        {
            config = gameConfig;
            input = inputManager;
            groundMask = platformMask;
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<BoxCollider2D>();
            animationController = GetComponent<PlayerAnimationController>();
            animationController.Initialize(config);

            body.gravityScale = 1f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            bodyCollider.size = new Vector2(42f, 78f);
            bodyCollider.offset = new Vector2(0f, 38f);

            Vector2 start = new Vector2(config.playerStartScreenPosition.x, config.WorldYFromScreenY(config.playerStartScreenPosition.y));
            transform.position = start;
        }

        private void Update()
        {
            if (config == null || input == null)
            {
                return;
            }

            UpdateGroundProbe();

            if (input.JumpPressed)
            {
                TryJump();
            }

            UpdateAnimationState();
            wasGrounded = grounded;
        }

        private void FixedUpdate()
        {
            if (config == null)
            {
                return;
            }

            Vector2 velocity = body.linearVelocity;
            velocity.x = 0f;
            body.linearVelocity = velocity;
        }

        private void TryJump()
        {
            bool canGroundJump = grounded || Time.time - lastGroundedTime <= config.coyoteTime;
            bool canAirJump = !canGroundJump && airJumpsUsed < config.maxAirJumps;
            if (!canGroundJump && !canAirJump)
            {
                return;
            }

            Vector2 velocity = body.linearVelocity;
            velocity.y = Mathf.Abs(config.jumpSpeed);
            body.linearVelocity = velocity;

            if (canAirJump)
            {
                airJumpsUsed++;
            }

            grounded = false;
            animationController.SetState(PlayerAnimState.Jump);
        }

        private void UpdateGroundProbe()
        {
            Bounds bounds = bodyCollider.bounds;
            Vector2 origin = new Vector2(bounds.center.x, bounds.min.y - 2f);
            Vector2 size = new Vector2(bounds.size.x * 0.72f, 6f);
            grounded = Physics2D.OverlapBox(origin, size, 0f, groundMask) != null && body.linearVelocity.y <= 1f;

            if (grounded)
            {
                lastGroundedTime = Time.time;
                airJumpsUsed = 0;
                if (!wasGrounded)
                {
                    landingUntil = Time.time + config.landingDuration;
                    animationController.SetState(PlayerAnimState.Landing);
                }
            }
        }

        private void UpdateAnimationState()
        {
            if (grounded)
            {
                if (Time.time < landingUntil)
                {
                    animationController.SetState(PlayerAnimState.Landing);
                }
                else
                {
                    animationController.SetState(PlayerAnimState.Running);
                }

                return;
            }

            animationController.SetState(body.linearVelocity.y >= 0f ? PlayerAnimState.Jump : PlayerAnimState.Fall);
        }
    }
}
