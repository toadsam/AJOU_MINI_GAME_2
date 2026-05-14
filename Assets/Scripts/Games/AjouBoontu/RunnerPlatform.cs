using UnityEngine;

namespace AjouFestival.Games.AjouBoontu
{
    public sealed class RunnerPlatform : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite platformSprite;

        private void Awake()
        {
            ApplySprite();
        }

        private void OnValidate()
        {
            ApplySprite();
        }

        private void ApplySprite()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null && platformSprite != null) spriteRenderer.sprite = platformSprite;
        }
    }
}
