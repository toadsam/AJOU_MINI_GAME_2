using AjouFestival.Core;
using UnityEngine;

namespace AjouFestival.Games.BalanceWalk
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class BalancePlayerController : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite playerSprite;

        [Header("Balance")]
        [SerializeField] private float balanceTorque = 30f;
        [SerializeField] private float randomTiltForce = 4f;
        [SerializeField] private float maxSafeAngle = 35f;
        [SerializeField] private float difficultyIncreaseRate = 0.05f;
        [SerializeField] private float moveSpeed = 4.2f;
        [SerializeField] private float speedIncreaseRate = 0.035f;
        [SerializeField] private bool useAutoMove = true;
        [SerializeField] private bool useDirectRotationAssist = true;
        [SerializeField] private float directRotationSpeed = 95f;

        public float CurrentAngle { get; private set; }
        public float MaxSafeAngle => maxSafeAngle;

        private Rigidbody2D body;
        private BalanceWalkGameManager gameManager;
        private float randomTimer;
        private float randomDirection = 1f;

        public void Initialize(BalanceWalkGameManager manager)
        {
            gameManager = manager;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.angularDamping = 0.8f;
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null && playerSprite != null) spriteRenderer.sprite = playerSprite;
        }

        private void Update()
        {
            CurrentAngle = Mathf.DeltaAngle(0f, transform.eulerAngles.z);
            if (gameManager != null && gameManager.HasStarted && !gameManager.IsGameOver && Mathf.Abs(CurrentAngle) > maxSafeAngle)
            {
                gameManager.GameOver(CurrentAngle);
            }
        }

        private void FixedUpdate()
        {
            if (gameManager == null || gameManager.IsGameOver || !gameManager.HasStarted)
            {
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
                return;
            }

            float input = 0f;
            if (FestivalInput.GetKey(KeyCode.A) || FestivalInput.GetKey(KeyCode.LeftArrow)) input += 1f;
            if (FestivalInput.GetKey(KeyCode.D) || FestivalInput.GetKey(KeyCode.RightArrow)) input -= 1f;

            float difficulty = 1f + gameManager.ElapsedTime * difficultyIncreaseRate;
            body.AddTorque(input * balanceTorque, ForceMode2D.Force);

            if (useDirectRotationAssist && Mathf.Abs(input) > 0.01f)
            {
                body.MoveRotation(body.rotation + input * directRotationSpeed * Time.fixedDeltaTime);
                body.angularVelocity *= 0.82f;
            }

            randomTimer -= Time.fixedDeltaTime;
            if (randomTimer <= 0f)
            {
                randomTimer = Random.Range(0.35f, 0.75f);
                randomDirection = Random.value < 0.5f ? -1f : 1f;
            }

            body.AddTorque(randomDirection * randomTiltForce * difficulty, ForceMode2D.Force);

            if (useAutoMove)
            {
                float forwardSpeed = moveSpeed + gameManager.DistanceMeters * speedIncreaseRate;
                body.linearVelocity = new Vector2(forwardSpeed, body.linearVelocity.y);
            }
        }
    }
}
