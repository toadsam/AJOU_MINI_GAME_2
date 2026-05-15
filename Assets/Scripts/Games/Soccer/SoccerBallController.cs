using UnityEngine;

namespace AjouFestival.Games.Soccer
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class SoccerBallController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite ballSprite;
        [SerializeField] private float maxSpeed = 16f;
        [SerializeField] private float linearDamping = 0.35f;
        [SerializeField] private float gravityScale = 2.2f;
        [SerializeField] private float bounciness = 0.78f;
        [SerializeField] private float friction = 0.2f;

        private static PhysicsMaterial2D sharedBallMaterial;

        private Rigidbody2D body;
        private CircleCollider2D ballCollider;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            ballCollider = GetComponent<CircleCollider2D>();

            body.gravityScale = gravityScale;
            body.linearDamping = linearDamping;
            body.angularDamping = 0.2f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (ballCollider != null && ballCollider.sharedMaterial == null)
            {
                sharedBallMaterial ??= new PhysicsMaterial2D("RuntimeSoccerBall")
                {
                    bounciness = bounciness,
                    friction = friction
                };
                ballCollider.sharedMaterial = sharedBallMaterial;
            }

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
            transform.rotation = Quaternion.identity;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
    }
}
