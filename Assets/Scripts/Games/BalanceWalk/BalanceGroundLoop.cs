using UnityEngine;

namespace AjouFestival.Games.BalanceWalk
{
    public sealed class BalanceGroundLoop : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float tileWidth = 60f;
        [SerializeField] private int tileCount = 3;
        [SerializeField] private float recycleBehind = 35f;

        private void Start()
        {
            if (target == null)
            {
                BalancePlayerController player = FindFirstObjectByType<BalancePlayerController>();
                if (player != null) target = player.transform;
            }
        }

        private void Update()
        {
            if (target == null)
            {
                return;
            }

            float rightEdge = transform.position.x + tileWidth * 0.5f;
            if (rightEdge < target.position.x - recycleBehind)
            {
                transform.position += Vector3.right * tileWidth * Mathf.Max(1, tileCount);
            }
        }
    }
}
