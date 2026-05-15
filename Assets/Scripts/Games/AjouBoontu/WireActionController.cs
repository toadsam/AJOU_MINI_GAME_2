using AjouFestival.Core;
using UnityEngine;

namespace AjouFestival.Games.AjouBoontu
{
    [RequireComponent(typeof(ChitoRunnerController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class WireActionController : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float maxWireDuration = 0.7f;
        [SerializeField] private float wireMinAirTime = 0.12f;
        [SerializeField] private float wirePullForce = 18f;
        [SerializeField] private float wireMaxFallSpeed = 3f;
        [SerializeField] private Vector3 anchorOffset = new Vector3(0f, 3.6f, 0f);

        public bool IsWiring { get; private set; }
        public bool HasUsedWireThisJump { get; private set; }
        public float AirTime { get; private set; }
        public float WireTimer { get; private set; }

        private ChitoRunnerController runner;
        private Rigidbody2D body;

        private void Awake()
        {
            runner = GetComponent<ChitoRunnerController>();
            body = GetComponent<Rigidbody2D>();
            if (lineRenderer == null)
            {
                lineRenderer = GetComponentInChildren<LineRenderer>();
            }

            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
                lineRenderer.startWidth = 0.04f;
                lineRenderer.endWidth = 0.02f;
                lineRenderer.positionCount = 2;
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = new Color(0.7f, 0.95f, 1f);
                lineRenderer.endColor = Color.white;
            }

            lineRenderer.enabled = false;
        }

        private void Update()
        {
            if (runner == null || !runner.IsRunning || runner.IsGameOver)
            {
                EndWire();
                return;
            }

            if (runner.IsGrounded)
            {
                AirTime = 0f;
                EndWire();
                return;
            }

            AirTime += Time.deltaTime;
            bool held = FestivalInput.GetKey(KeyCode.Space) || FestivalInput.MouseOrTouchHeld();

            if (!IsWiring && CanStartWire(held))
            {
                StartWire();
            }

            if (IsWiring)
            {
                WireTimer += Time.deltaTime;
                if (!held || WireTimer >= maxWireDuration)
                {
                    EndWire();
                }
                else
                {
                    UpdateLine();
                }
            }
        }

        private void FixedUpdate()
        {
            if (runner == null || !runner.IsRunning)
            {
                if (lineRenderer != null)
                {
                    lineRenderer.enabled = false;
                }

                return;
            }

            if (!IsWiring)
            {
                return;
            }

            Vector2 velocity = body.linearVelocity;
            velocity.y = Mathf.Max(velocity.y, -wireMaxFallSpeed);
            body.linearVelocity = velocity;
            body.AddForce(Vector2.up * wirePullForce, ForceMode2D.Force);
        }

        public void ResetWireOnGrounded()
        {
            HasUsedWireThisJump = false;
            AirTime = 0f;
            EndWire();
        }

        public void ResetForStart()
        {
            HasUsedWireThisJump = false;
            AirTime = 0f;
            WireTimer = 0f;
            IsWiring = false;
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
        }

        private bool CanStartWire(bool held)
        {
            return held && !runner.IsGrounded && !IsWiring && !HasUsedWireThisJump && AirTime >= wireMinAirTime;
        }

        private void StartWire()
        {
            IsWiring = true;
            HasUsedWireThisJump = true;
            WireTimer = 0f;
            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
                UpdateLine();
            }
        }

        private void EndWire()
        {
            IsWiring = false;
            WireTimer = 0f;
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
        }

        private void UpdateLine()
        {
            if (lineRenderer == null)
            {
                return;
            }

            lineRenderer.SetPosition(0, transform.position + Vector3.up * 0.45f);
            lineRenderer.SetPosition(1, transform.position + anchorOffset);
        }
    }
}
