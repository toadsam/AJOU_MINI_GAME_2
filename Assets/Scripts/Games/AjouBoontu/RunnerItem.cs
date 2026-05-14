using UnityEngine;

namespace AjouFestival.Games.AjouBoontu
{
    public sealed class RunnerItem : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite itemSprite;
        [SerializeField] private int scoreValue = 100;

        public int ScoreValue => scoreValue;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null && itemSprite != null) spriteRenderer.sprite = itemSprite;
        }

        private void Update()
        {
            transform.Rotate(0f, 0f, 90f * Time.deltaTime);
        }

        public void Collect(AjouBoontuGameManager gameManager)
        {
            gameManager?.AddItemScore(scoreValue);
            Destroy(gameObject);
        }
    }
}
