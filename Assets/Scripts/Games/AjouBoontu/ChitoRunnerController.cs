using AjouFestival.Core;
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
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            maxJumpCount = Mathf.Max(1, maxJumpCount);

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
                gameManager?.GameOver("諛쒗뙋 ?꾨옒濡??⑥뼱議뚯뒿?덈떎.");
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

            body.linearVelocity = new Vector2(runSpeed, body.linearVelocity.y);
        }

        public void SetRunning(bool running)
        {
            isRunning = running;
            if (!isRunning)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                jumpsUsed = 0;
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
            groundContacts = 0;
        }

        public void SetRunSpeed(float speed)
        {
            runSpeed = Mathf.Max(0f, speed);
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
            if (collision.collider.GetComponent<RunnerPlatform>() != null)
            {
                groundContacts++;
                jumpsUsed = 0;
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.collider.GetComponent<RunnerPlatform>() != null)
            {
                groundContacts = Mathf.Max(0, groundContacts - 1);
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
                gameManager?.GameOver("?μ븷臾쇱뿉 遺?ろ삍?듬땲??");
            }
        }
    }
}
