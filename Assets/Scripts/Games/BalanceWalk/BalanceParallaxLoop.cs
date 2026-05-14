using UnityEngine;

namespace AjouFestival.Games.BalanceWalk
{
    public sealed class BalanceParallaxLoop : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float parallaxFactor = 0.35f;
        [SerializeField] private float tileWidth = 24f;
        [SerializeField] private int tileCount = 6;

        private float startX;
        private float targetStartX;

        private void Start()
        {
            startX = transform.position.x;
            if (target == null)
            {
                BalancePlayerController player = FindFirstObjectByType<BalancePlayerController>();
                if (player != null) target = player.transform;
            }

            targetStartX = target != null ? target.position.x : 0f;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            float travelled = target.position.x - targetStartX;
            Vector3 position = transform.position;
            position.x = startX + travelled * parallaxFactor;

            float rightEdge = position.x + tileWidth * 0.5f;
            if (rightEdge < target.position.x - tileWidth)
            {
                startX += tileWidth * Mathf.Max(1, tileCount);
                position.x += tileWidth * Mathf.Max(1, tileCount);
            }

            transform.position = position;
        }
    }
}
