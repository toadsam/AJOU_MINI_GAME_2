using UnityEngine;

namespace AjouFestival.Games.Soccer
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class SoccerBallController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite ballSprite;
        [SerializeField] private float maxSpeed = 12f;
        [SerializeField] private float linearDamping = 1.2f;

        private Rigidbody2D body;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.linearDamping = linearDamping;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null && ballSprite != null) spriteRenderer.sprite = ballSprite;
        }

        private void FixedUpdate()
        {
            if (body.linearVelocity.magnitude > maxSpeed)
            {
                body.linearVelocity = body.linearVelocity.normalized * maxSpeed;
            }
        }

        public void Kick(Vector2 impulse)
        {
            body.AddForce(impulse, ForceMode2D.Impulse);
        }

        public void ResetPosition(Vector3 position)
        {
            transform.position = position;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
    }
}
