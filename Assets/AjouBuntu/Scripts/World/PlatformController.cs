using AjouBuntu.Config;
using AjouBuntu.Core;
using UnityEngine;

namespace AjouBuntu.World
{
    public sealed class PlatformController : MonoBehaviour
    {
        [SerializeField] private PlatformKind platformKind = PlatformKind.StoneBridge;
        [SerializeField] private float width = 520f;
        [SerializeField] private float topScreenY = 403f;

        private GameManager gameManager;
        private float despawnX;
        private SpriteRenderer spriteRenderer;

        public float Width => width;
        public float TopWorldY { get; private set; }
        public float TopScreenY => topScreenY;
        public float RightX => transform.position.x + width * 0.5f;
        public float LeftX => transform.position.x - width * 0.5f;

        public void InitializeFromScene(GameConfig config, GameManager manager, float despawnAtX)
        {
            PlatformDefinition definition = config.GetPlatform(platformKind);
            Initialize(definition, manager, width, config.WorldYFromScreenY(topScreenY), despawnAtX);
        }

        public void Initialize(PlatformDefinition definition, GameManager manager, float targetWidth, float topWorldY, float despawnAtX)
        {
            gameManager = manager;
            despawnX = despawnAtX;
            width = targetWidth;
            TopWorldY = topWorldY;
            topScreenY = 540f - topWorldY;

            Rigidbody2D platformBody = GetComponent<Rigidbody2D>();
            if (platformBody == null)
            {
                platformBody = gameObject.AddComponent<Rigidbody2D>();
            }

            platformBody.bodyType = RigidbodyType2D.Kinematic;
            platformBody.simulated = true;
            platformBody.interpolation = RigidbodyInterpolation2D.Interpolate;

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sortingOrder = 5;
            spriteRenderer.sprite = definition.sprite != null
                ? definition.sprite
                : RuntimeSpriteFactory.CreateRoundedRectSprite(new Color(0.42f, 0.62f, 0.74f), new Color(0.12f, 0.3f, 0.44f), 256, 64, 8);

            float visualHeight = definition.visualSize.y;
            transform.position = new Vector3(transform.position.x, topWorldY - visualHeight * 0.5f, 0f);
            FitSprite(spriteRenderer, new Vector2(targetWidth, visualHeight));

            Transform existingTop = transform.Find("OneWayTopCollider");
            GameObject top = existingTop != null ? existingTop.gameObject : new GameObject("OneWayTopCollider");
            top.layer = gameObject.layer;
            top.transform.SetParent(transform, false);
            top.transform.localPosition = new Vector3(definition.colliderOffset.x, definition.colliderOffset.y, 0f);

            BoxCollider2D collider = top.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = top.AddComponent<BoxCollider2D>();
            }

            collider.size = new Vector2(Mathf.Min(definition.colliderSize.x, targetWidth - 24f), definition.colliderSize.y);
            collider.usedByEffector = true;

            PlatformEffector2D effector = top.GetComponent<PlatformEffector2D>();
            if (effector == null)
            {
                effector = top.AddComponent<PlatformEffector2D>();
            }

            effector.useOneWay = true;
            effector.useSideBounce = false;
            effector.useSideFriction = false;
            effector.surfaceArc = 170f;
        }

        private void Update()
        {
            if (gameManager == null || gameManager.IsFinished)
            {
                return;
            }

            transform.Translate(Vector3.left * gameManager.CurrentSpeed * Time.deltaTime, Space.World);
            if (RightX < despawnX)
            {
                Destroy(gameObject);
            }
        }

        private static void FitSprite(SpriteRenderer renderer, Vector2 targetSize)
        {
            Vector2 size = renderer.sprite.bounds.size;
            if (size.x <= 0f || size.y <= 0f)
            {
                return;
            }

            renderer.transform.localScale = new Vector3(targetSize.x / size.x, targetSize.y / size.y, 1f);
        }

#if UNITY_EDITOR
        public void SetEditorValues(PlatformKind kind, float editorWidth, float editorTopScreenY)
        {
            platformKind = kind;
            width = editorWidth;
            topScreenY = editorTopScreenY;
            TopWorldY = 540f - editorTopScreenY;
        }
#endif
    }
}
