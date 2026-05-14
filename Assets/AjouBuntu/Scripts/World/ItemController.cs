using System.Collections;
using AjouBuntu.Config;
using AjouBuntu.Core;
using UnityEngine;

namespace AjouBuntu.World
{
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class ItemController : MonoBehaviour
    {
        [SerializeField] private ItemKind itemKind = ItemKind.Coffee;

        private ItemDefinition definition;
        private ScoreManager scoreManager;
        private GameManager gameManager;
        private SpriteRenderer spriteRenderer;
        private bool collected;

        public void InitializeFromScene(GameConfig config, ScoreManager score, GameManager manager)
        {
            Initialize(config.GetItem(itemKind), score, manager);
        }

        public void Initialize(ItemDefinition itemDefinition, ScoreManager score, GameManager manager)
        {
            definition = itemDefinition;
            itemKind = definition.kind;
            scoreManager = score;
            gameManager = manager;
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sortingOrder = 8;
            spriteRenderer.sprite = definition.sprite != null ? definition.sprite : CreateFallbackSprite(definition.kind);
            FitSprite(spriteRenderer, new Vector2(34f, 34f));

            CircleCollider2D trigger = GetComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 18f;
        }

        private void Update()
        {
            if (gameManager != null && !gameManager.IsFinished)
            {
                transform.Translate(Vector3.left * gameManager.CurrentSpeed * Time.deltaTime, Space.World);
                if (transform.position.x < -180f)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            transform.Rotate(0f, 0f, 70f * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected || other.GetComponent<AjouBuntu.Player.PlayerController>() == null)
            {
                return;
            }

            collected = true;
            scoreManager?.AddItem(definition, transform.position);
            StartCoroutine(CollectRoutine());
        }

        private IEnumerator CollectRoutine()
        {
            CircleCollider2D trigger = GetComponent<CircleCollider2D>();
            trigger.enabled = false;

            float time = 0f;
            Vector3 startScale = transform.localScale;
            while (time < 0.22f)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / 0.22f);
                transform.localScale = Vector3.Lerp(startScale, startScale * 1.8f, t);
                Color color = spriteRenderer.color;
                color.a = 1f - t;
                spriteRenderer.color = color;
                yield return null;
            }

            SpawnSpark(transform.position);
            Destroy(gameObject);
        }

        private static Sprite CreateFallbackSprite(ItemKind kind)
        {
            Color fill = kind == ItemKind.APlus ? new Color(1f, 0.9f, 0.22f) : new Color(0.2f, 0.82f, 1f);
            return RuntimeSpriteFactory.CreateRoundedRectSprite(fill, Color.white, 64, 64, 16);
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

        private static void SpawnSpark(Vector3 position)
        {
            GameObject spark = new GameObject("CollectSpark");
            spark.transform.position = position;
            SpriteRenderer renderer = spark.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.CreateSolidSprite(new Color(0.75f, 1f, 1f, 0.8f), 10, 10);
            renderer.sortingOrder = 20;
            spark.AddComponent<AutoDestroy>().Initialize(0.18f, true);
        }

#if UNITY_EDITOR
        public void SetEditorKind(ItemKind kind)
        {
            itemKind = kind;
        }
#endif
    }
}
