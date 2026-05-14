using UnityEngine;

namespace AjouFestival.Games.AjouBoontu
{
    public sealed class RunnerObstacle : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite obstacleSprite;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null && obstacleSprite != null) spriteRenderer.sprite = obstacleSprite;
        }
    }
}
