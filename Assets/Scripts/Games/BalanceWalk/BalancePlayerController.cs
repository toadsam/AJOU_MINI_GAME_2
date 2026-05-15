using AjouFestival.Core;
using UnityEngine;

namespace AjouFestival.Games.BalanceWalk
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class BalancePlayerController : MonoBehaviour
    {
        [Header("Rig")]
        [SerializeField] private Transform visualAnchor;
        [SerializeField] private Transform bodyCircle;
        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;
        [SerializeField] private Sprite circleSprite;
        [SerializeField] private Sprite stickSprite;
        [SerializeField] private float visualFootPivotY = -0.18f;

        [Header("Slump Motion")]
        [SerializeField] private float bodyLeanAngle = 26f;
        [SerializeField] private float bodyAngularVelocityInfluence = 0.22f;
        [SerializeField] private float bodyMaxOvershootAngle = 16f;
        [SerializeField] private float bodyFollowSmoothTime = 0.24f;
        [SerializeField] private float bodySlideDistance = 0.48f;
        [SerializeField] private float bodyDropDistance = 0.58f;
        [SerializeField] private float bodyCompressX = 0.08f;
        [SerializeField] private float bodyStretchY = 0.14f;
        [SerializeField] private float settleBob = 0.04f;
        [SerializeField] private float settleFrequency = 4.8f;

        [Header("Leg Supports")]
        [SerializeField] private float hipWidth = 0.16f;
        [SerializeField] private float legTopDrop = 0.22f;
        [SerializeField] private float groundSpreadDistance = 0.26f;
        [SerializeField] private float legThicknessLeanScale = 0.12f;
        [SerializeField] private float legFollowSmoothTime = 0.14f;

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

        private const int BodyIndex = 0;
        private const int LeftLegIndex = 1;
        private const int RightLegIndex = 2;

        private static readonly Color BodyColor = new(0.2f, 0.35f, 0.28f, 1f);
        private static readonly Color LegColor = new(0.55f, 0.43f, 0.29f, 1f);

        private Rigidbody2D body;
        private BalanceWalkGameManager gameManager;
        private float randomTimer;
        private float randomDirection = 1f;
        private SpriteRenderer sourceRenderer;

        private Transform[] partTransforms;
        private Vector3[] baseLocalPositions;
        private Vector3[] currentLocalPositions;
        private Vector3[] positionVelocities;
        private Vector3[] baseLocalScales;
        private Vector3[] currentLocalScales;
        private float[] baseLocalAngles;
        private float[] currentLocalAngles;
        private float[] angleVelocities;

        private Vector3 baseAnchorLocalPosition;
        private Vector2 baseLeftGroundPoint;
        private Vector2 baseRightGroundPoint;
        private float baseLegThickness;
        private float currentBodyAngle;
        private float bodyAngleVelocity;

        public void Initialize(BalanceWalkGameManager manager)
        {
            gameManager = manager;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.angularDamping = 0.8f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            sourceRenderer = GetComponent<SpriteRenderer>();
            EnsureVisualRig();
            CacheRigPose();
        }

        private void Update()
        {
            CurrentAngle = Mathf.DeltaAngle(0f, transform.eulerAngles.z);
            if (gameManager != null && gameManager.HasStarted && !gameManager.IsGameOver && Mathf.Abs(CurrentAngle) > maxSafeAngle)
            {
                gameManager.GameOver(CurrentAngle);
            }
        }

        private void LateUpdate()
        {
            UpdateSlumpPose();
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

        private void EnsureVisualRig()
        {
            if (visualAnchor == null)
            {
                visualAnchor = transform.Find("VisualAnchor");
            }

            if (visualAnchor == null)
            {
                GameObject anchorObject = new("VisualAnchor");
                visualAnchor = anchorObject.transform;
                visualAnchor.SetParent(transform, false);
                visualAnchor.localPosition = new Vector3(0f, visualFootPivotY, 0f);
                visualAnchor.localRotation = Quaternion.identity;
                visualAnchor.localScale = Vector3.one;
            }

            bodyCircle = EnsurePart(ref bodyCircle, "BodyCircle", circleSprite, new Vector3(0f, 1.34f, 0f), new Vector3(0.38f, 0.38f, 1f), 2, BodyColor);
            leftLeg = EnsurePart(ref leftLeg, "LeftLeg", stickSprite, new Vector3(-0.24f, -0.44f, 0f), new Vector3(0.24f, 1.04f, 1f), 0, LegColor);
            rightLeg = EnsurePart(ref rightLeg, "RightLeg", stickSprite, new Vector3(0.24f, -0.44f, 0f), new Vector3(0.24f, 1.04f, 1f), 0, LegColor);

            partTransforms = new[]
            {
                bodyCircle,
                leftLeg,
                rightLeg
            };

            if (sourceRenderer != null)
            {
                sourceRenderer.enabled = false;
            }
        }

        private Transform EnsurePart(ref Transform part, string partName, Sprite sprite, Vector3 defaultLocalPosition, Vector3 defaultLocalScale, int sortingOffset, Color defaultColor)
        {
            if (part == null && visualAnchor != null)
            {
                part = visualAnchor.Find(partName);
            }

            bool created = false;
            if (part == null)
            {
                GameObject partObject = new(partName);
                part = partObject.transform;
                part.SetParent(visualAnchor, false);
                part.localPosition = defaultLocalPosition;
                part.localRotation = Quaternion.identity;
                part.localScale = defaultLocalScale;
                created = true;
            }

            SpriteRenderer renderer = part.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = part.gameObject.AddComponent<SpriteRenderer>();
                created = true;
            }

            if (created && sourceRenderer != null)
            {
                renderer.sharedMaterial = sourceRenderer.sharedMaterial;
                renderer.sortingLayerID = sourceRenderer.sortingLayerID;
                renderer.sortingOrder = sourceRenderer.sortingOrder + sortingOffset;
            }

            if (created)
            {
                renderer.color = defaultColor;
            }

            if (sprite != null && renderer.sprite == null)
            {
                renderer.sprite = sprite;
            }

            return part;
        }

        private void CacheRigPose()
        {
            if (visualAnchor == null || partTransforms == null)
            {
                return;
            }

            baseAnchorLocalPosition = visualAnchor.localPosition;

            int count = partTransforms.Length;
            baseLocalPositions = new Vector3[count];
            currentLocalPositions = new Vector3[count];
            positionVelocities = new Vector3[count];
            baseLocalScales = new Vector3[count];
            currentLocalScales = new Vector3[count];
            baseLocalAngles = new float[count];
            currentLocalAngles = new float[count];
            angleVelocities = new float[count];

            for (int i = 0; i < count; i++)
            {
                Transform part = partTransforms[i];
                if (part == null)
                {
                    continue;
                }

                baseLocalPositions[i] = part.localPosition;
                currentLocalPositions[i] = part.localPosition;
                baseLocalScales[i] = part.localScale == Vector3.zero ? Vector3.one : part.localScale;
                currentLocalScales[i] = baseLocalScales[i];
                baseLocalAngles[i] = part.localEulerAngles.z;
                currentLocalAngles[i] = baseLocalAngles[i];
            }

            baseLegThickness = baseLocalScales[LeftLegIndex].x;
            baseLeftGroundPoint = new Vector2(baseLocalPositions[LeftLegIndex].x, baseLocalPositions[LeftLegIndex].y - (baseLocalScales[LeftLegIndex].y * 0.5f));
            baseRightGroundPoint = new Vector2(baseLocalPositions[RightLegIndex].x, baseLocalPositions[RightLegIndex].y - (baseLocalScales[RightLegIndex].y * 0.5f));
        }

        private void UpdateSlumpPose()
        {
            if (visualAnchor == null || partTransforms == null || partTransforms.Length < 3)
            {
                return;
            }

            bool active = gameManager != null && gameManager.HasStarted && !gameManager.IsGameOver;
            float normalized = active ? Mathf.Clamp(CurrentAngle / maxSafeAngle, -1f, 1f) : 0f;
            float magnitude = Mathf.Abs(normalized);
            float overshoot = active
                ? Mathf.Clamp(body.angularVelocity * bodyAngularVelocityInfluence, -bodyMaxOvershootAngle, bodyMaxOvershootAngle)
                : 0f;

            float overshootNormalized = bodyMaxOvershootAngle > 0.001f ? overshoot / bodyMaxOvershootAngle : 0f;
            float targetBodyAngle = normalized * bodyLeanAngle + overshoot;
            currentBodyAngle = Mathf.SmoothDampAngle(currentBodyAngle, targetBodyAngle, ref bodyAngleVelocity, bodyFollowSmoothTime);

            float bob = active
                ? -Mathf.Sin(Time.time * settleFrequency) * settleBob * Mathf.Max(0.25f, magnitude)
                : 0f;

            float slideX = normalized * bodySlideDistance + overshootNormalized * 0.12f;
            float drop = magnitude * bodyDropDistance + Mathf.Abs(overshootNormalized) * 0.06f;

            visualAnchor.localPosition = baseAnchorLocalPosition;
            visualAnchor.localRotation = Quaternion.identity;

            Vector3 bodyTarget = baseLocalPositions[BodyIndex] + new Vector3(
                slideX,
                -drop + bob,
                0f);

            Vector3 bodyScaleTarget = new(
                baseLocalScales[BodyIndex].x * (1f - magnitude * bodyCompressX),
                baseLocalScales[BodyIndex].y * (1f + magnitude * bodyStretchY),
                baseLocalScales[BodyIndex].z);

            ApplyBodyPose(bodyTarget, bodyScaleTarget, currentBodyAngle);

            float leftSpread = Mathf.Max(0f, -normalized) * groundSpreadDistance + magnitude * 0.04f;
            float rightSpread = Mathf.Max(0f, normalized) * groundSpreadDistance + magnitude * 0.04f;
            Vector2 leftGround = baseLeftGroundPoint + new Vector2(-leftSpread, 0f);
            Vector2 rightGround = baseRightGroundPoint + new Vector2(rightSpread, 0f);

            Quaternion bodyRotation = Quaternion.Euler(0f, 0f, currentLocalAngles[BodyIndex]);
            Vector2 leftHipAttach = (Vector2)currentLocalPositions[BodyIndex] + (Vector2)(bodyRotation * new Vector3(-hipWidth, -legTopDrop, 0f));
            Vector2 rightHipAttach = (Vector2)currentLocalPositions[BodyIndex] + (Vector2)(bodyRotation * new Vector3(hipWidth, -legTopDrop, 0f));

            PoseLeg(LeftLegIndex, leftHipAttach, leftGround);
            PoseLeg(RightLegIndex, rightHipAttach, rightGround);
        }

        private void ApplyBodyPose(Vector3 targetPosition, Vector3 targetScale, float targetAngle)
        {
            currentLocalPositions[BodyIndex] = Vector3.SmoothDamp(currentLocalPositions[BodyIndex], targetPosition, ref positionVelocities[BodyIndex], bodyFollowSmoothTime);
            currentLocalAngles[BodyIndex] = Mathf.SmoothDampAngle(currentLocalAngles[BodyIndex], targetAngle, ref angleVelocities[BodyIndex], bodyFollowSmoothTime);

            float scaleLerp = 1f - Mathf.Exp(-10f * Time.deltaTime);
            currentLocalScales[BodyIndex] = Vector3.Lerp(currentLocalScales[BodyIndex], targetScale, scaleLerp);

            bodyCircle.localPosition = currentLocalPositions[BodyIndex];
            bodyCircle.localRotation = Quaternion.Euler(0f, 0f, currentLocalAngles[BodyIndex]);
            bodyCircle.localScale = currentLocalScales[BodyIndex];
        }

        private void PoseLeg(int index, Vector2 topPoint, Vector2 groundPoint)
        {
            Transform leg = partTransforms[index];
            if (leg == null)
            {
                return;
            }

            Vector2 legVector = groundPoint - topPoint;
            float legLength = Mathf.Max(0.2f, legVector.magnitude);
            float angle = Mathf.Atan2(legVector.y, legVector.x) * Mathf.Rad2Deg - 90f;

            Vector3 targetPosition = (topPoint + groundPoint) * 0.5f;
            Vector3 targetScale = new(
                baseLegThickness * (1f - (Mathf.Abs(CurrentAngle) / maxSafeAngle) * legThicknessLeanScale),
                legLength,
                1f);

            currentLocalPositions[index] = Vector3.SmoothDamp(currentLocalPositions[index], targetPosition, ref positionVelocities[index], legFollowSmoothTime);
            currentLocalAngles[index] = Mathf.SmoothDampAngle(currentLocalAngles[index], angle, ref angleVelocities[index], legFollowSmoothTime);

            float scaleLerp = 1f - Mathf.Exp(-14f * Time.deltaTime);
            currentLocalScales[index] = Vector3.Lerp(currentLocalScales[index], targetScale, scaleLerp);

            leg.localPosition = currentLocalPositions[index];
            leg.localRotation = Quaternion.Euler(0f, 0f, currentLocalAngles[index]);
            leg.localScale = currentLocalScales[index];
        }
    }
}
