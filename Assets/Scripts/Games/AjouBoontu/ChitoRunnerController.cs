using AjouFestival.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AjouFestival.Games.AjouBoontu
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class ChitoRunnerController : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private Animator animator;

        [Header("Movement")]
        [SerializeField] private float runSpeed = 5.8f;
        [SerializeField] private float jumpForce = 11f;
        [SerializeField] private float fallDeathY = -7f;
        [SerializeField, Min(1)] private int maxJumpCount = 2;
        [SerializeField, Range(0f, 1f)] private float minimumGroundNormalY = 0.55f;

        [Header("Auto Unstick")]
        [SerializeField] private bool useAutoDropThrough = true;
        [SerializeField, Min(0f)] private float stuckDurationSeconds = 1f;
        [SerializeField, Min(0f)] private float minimumProgressSpeed = 0.15f;
        [SerializeField, Min(0.05f)] private float dropThroughSeconds = 0.2f;
        [SerializeField] private float dropVelocityY = -2f;

        private static readonly int IdleStateHash = Animator.StringToHash("Base Layer.Idle");
        private static readonly int RunStateHash = Animator.StringToHash("Base Layer.Run");
        private static readonly int JumpStateHash = Animator.StringToHash("Base Layer.Jump");
        private static readonly int FallStateHash = Animator.StringToHash("Base Layer.Fall");

        private Rigidbody2D body;
        private AjouBoontuGameManager gameManager;
        private int groundContacts;
        private int jumpsUsed;
        private VisualState currentVisualState = VisualState.Idle;
        private bool isRunning;
        private readonly HashSet<Collider2D> groundedPlatforms = new();
        private Collider2D mainCollider;
        private float lastXPosition;
        private float stuckTimer;
        private bool isDroppingThrough;

        public bool IsGrounded => groundContacts > 0;
        public bool IsRunning => isRunning;
        public bool IsGameOver => gameManager != null && gameManager.IsGameOver;
        public Rigidbody2D Body => body;
        public float RunSpeed => runSpeed;

        private enum VisualState
        {
            Idle,
            Run,
            Jump,
            Fall
        }

        public void Initialize(AjouBoontuGameManager manager)
        {
            gameManager = manager;
            isRunning = false;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            mainCollider = GetComponent<Collider2D>();
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            maxJumpCount = Mathf.Max(1, maxJumpCount);
            lastXPosition = transform.position.x;

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            ApplyVisualState(true);
        }

        private void Update()
        {
            if (!isRunning || IsGameOver)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                return;
            }

            if ((FestivalInput.GetKeyDown(KeyCode.Space) || FestivalInput.MouseOrTouchDown()) && CanJump())
            {
                Jump();
            }

            if (transform.position.y < fallDeathY)
            {
                gameManager?.GameOver("Fell off the stage.");
            }
        }

        private void LateUpdate()
        {
            ApplyVisualState();
        }

        private void FixedUpdate()
        {
            if (!isRunning || IsGameOver)
            {
                return;
            }

            UpdateAutoDropThrough();
            body.linearVelocity = new Vector2(runSpeed, body.linearVelocity.y);
        }

        public void SetRunning(bool running)
        {
            isRunning = running;
            stuckTimer = 0f;
            lastXPosition = transform.position.x;

            if (!isRunning)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                groundedPlatforms.Clear();
                groundContacts = 0;
                jumpsUsed = 0;
                isDroppingThrough = false;
                if (mainCollider != null)
                {
                    mainCollider.enabled = true;
                }
                ApplyVisualState(true);
            }
        }

        private bool CanJump()
        {
            return IsGrounded || jumpsUsed < maxJumpCount;
        }

        private void Jump()
        {
            jumpsUsed++;
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);
            groundedPlatforms.Clear();
            groundContacts = 0;
        }

        public void SetRunSpeed(float speed)
        {
            runSpeed = Mathf.Max(0f, speed);
        }

        private void UpdateAutoDropThrough()
        {
            float currentX = transform.position.x;
            float actualProgressSpeed = Mathf.Abs(currentX - lastXPosition) / Mathf.Max(Time.fixedDeltaTime, 0.0001f);
            lastXPosition = currentX;

            if (!useAutoDropThrough || isDroppingThrough || mainCollider == null)
            {
                stuckTimer = 0f;
                return;
            }

            if (actualProgressSpeed <= minimumProgressSpeed)
            {
                stuckTimer += Time.fixedDeltaTime;
                if (stuckTimer >= stuckDurationSeconds)
                {
                    StartCoroutine(DropThroughRoutine());
                }
            }
            else
            {
                stuckTimer = 0f;
            }
        }

        private IEnumerator DropThroughRoutine()
        {
            if (isDroppingThrough || mainCollider == null)
            {
                yield break;
            }

            isDroppingThrough = true;
            stuckTimer = 0f;
            groundedPlatforms.Clear();
            groundContacts = 0;
            mainCollider.enabled = false;
            body.linearVelocity = new Vector2(runSpeed, Mathf.Min(body.linearVelocity.y, dropVelocityY));

            yield return new WaitForSeconds(dropThroughSeconds);

            if (mainCollider != null)
            {
                mainCollider.enabled = true;
            }

            lastXPosition = transform.position.x;
            isDroppingThrough = false;
        }

        private void ApplyVisualState(bool forceRestart = false)
        {
            if (animator == null)
            {
                return;
            }

            VisualState nextState = ResolveVisualState();
            if (!forceRestart && nextState == currentVisualState)
            {
                return;
            }

            currentVisualState = nextState;
            animator.Play(GetStateHash(nextState), 0, 0f);
            animator.Update(0f);
        }

        private VisualState ResolveVisualState()
        {
            if (!IsRunning)
            {
                return VisualState.Idle;
            }

            if (IsGameOver)
            {
                return VisualState.Idle;
            }

            if (!IsGrounded)
            {
                return body.linearVelocity.y >= 0f ? VisualState.Jump : VisualState.Fall;
            }

            return VisualState.Run;
        }

        private static int GetStateHash(VisualState state)
        {
            return state switch
            {
                VisualState.Run => RunStateHash,
                VisualState.Jump => JumpStateHash,
                VisualState.Fall => FallStateHash,
                _ => IdleStateHash
            };
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            UpdateGroundContact(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            UpdateGroundContact(collision);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.collider.GetComponent<RunnerPlatform>() != null)
            {
                groundedPlatforms.Remove(collision.collider);
                groundContacts = groundedPlatforms.Count;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isRunning || IsGameOver)
            {
                return;
            }

            RunnerItem item = other.GetComponent<RunnerItem>();
            if (item != null)
            {
                item.Collect(gameManager);
                return;
            }

            if (other.GetComponent<RunnerObstacle>() != null)
            {
                gameManager?.GameOver("Hit an obstacle.");
            }
        }

        private void UpdateGroundContact(Collision2D collision)
        {
            Collider2D platformCollider = collision.collider;
            if (platformCollider.GetComponent<RunnerPlatform>() == null)
            {
                return;
            }

            bool isGroundContact = false;
            int contactCount = collision.contactCount;
            for (int i = 0; i < contactCount; i++)
            {
                ContactPoint2D contact = collision.GetContact(i);
                if (contact.normal.y >= minimumGroundNormalY)
                {
                    isGroundContact = true;
                    break;
                }
            }

            if (isGroundContact)
            {
                groundedPlatforms.Add(platformCollider);
                jumpsUsed = 0;
            }
            else
            {
                groundedPlatforms.Remove(platformCollider);
            }

            groundContacts = groundedPlatforms.Count;
        }
    }
}
