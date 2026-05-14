using AjouFestival.Core;
using UnityEngine;

namespace AjouFestival.Games.AjouBoontu
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class ChitoRunnerController : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite runSprite;
        [SerializeField] private Sprite jumpSprite;
        [SerializeField] private Sprite fallSprite;

        [Header("Movement")]
        [SerializeField] private float runSpeed = 5.8f;
        [SerializeField] private float jumpForce = 11f;
        [SerializeField] private float fallDeathY = -7f;

        private Rigidbody2D body;
        private AjouBoontuGameManager gameManager;
        private int groundContacts;

        public bool IsGrounded => groundContacts > 0;
        public bool IsGameOver => gameManager != null && gameManager.IsGameOver;
        public Rigidbody2D Body => body;

        public void Initialize(AjouBoontuGameManager manager)
        {
            gameManager = manager;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (IsGameOver)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                return;
            }

            if ((FestivalInput.GetKeyDown(KeyCode.Space) || FestivalInput.MouseOrTouchDown()) && IsGrounded)
            {
                Jump();
            }

            UpdateVisualState();

            if (transform.position.y < fallDeathY)
            {
                gameManager?.GameOver("발판 아래로 떨어졌습니다.");
            }
        }

        private void FixedUpdate()
        {
            if (IsGameOver)
            {
                return;
            }

            body.linearVelocity = new Vector2(runSpeed, body.linearVelocity.y);
        }

        private void Jump()
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);
            groundContacts = 0;
        }

        private void UpdateVisualState()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            if (IsGrounded && runSprite != null)
            {
                spriteRenderer.sprite = runSprite;
            }
            else if (body.linearVelocity.y >= 0f && jumpSprite != null)
            {
                spriteRenderer.sprite = jumpSprite;
            }
            else if (body.linearVelocity.y < 0f && fallSprite != null)
            {
                spriteRenderer.sprite = fallSprite;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.GetComponent<RunnerPlatform>() != null)
            {
                groundContacts++;
                GetComponent<WireActionController>()?.ResetWireOnGrounded();
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
            RunnerItem item = other.GetComponent<RunnerItem>();
            if (item != null)
            {
                item.Collect(gameManager);
                return;
            }

            if (other.GetComponent<RunnerObstacle>() != null)
            {
                gameManager?.GameOver("장애물에 부딪혔습니다.");
            }
        }
    }
}
