using UnityEngine;

namespace AjouBuntu.World
{
    public sealed class AutoDestroy : MonoBehaviour
    {
        private float life;
        private bool expand;
        private float age;
        private SpriteRenderer spriteRenderer;
        private Vector3 startScale;

        public void Initialize(float lifetime, bool expandAndFade)
        {
            life = lifetime;
            expand = expandAndFade;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            startScale = transform.localScale;
        }

        private void Update()
        {
            age += Time.deltaTime;
            float t = life <= 0f ? 1f : Mathf.Clamp01(age / life);
            if (expand)
            {
                transform.localScale = Vector3.Lerp(startScale, startScale * 4f, t);
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = 1f - t;
                    spriteRenderer.color = color;
                }
            }

            if (age >= life)
            {
                Destroy(gameObject);
            }
        }
    }
}
