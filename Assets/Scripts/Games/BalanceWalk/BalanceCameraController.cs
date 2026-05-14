using UnityEngine;

namespace AjouFestival.Games.BalanceWalk
{
    public sealed class BalanceCameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(2.2f, 0.4f, -10f);
        [SerializeField] private float followSpeed = 6f;
        [SerializeField] private bool followXOnly = true;

        private float fixedY;

        private void Start()
        {
            fixedY = transform.position.y;
            if (target == null)
            {
                BalancePlayerController player = FindFirstObjectByType<BalancePlayerController>();
                if (player != null)
                {
                    target = player.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                BalancePlayerController player = FindFirstObjectByType<BalancePlayerController>();
                if (player != null)
                {
                    target = player.transform;
                }
            }

            if (target == null)
            {
                return;
            }

            Vector3 desired = target.position + offset;
            if (followXOnly)
            {
                desired.y = fixedY;
            }

            transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);
        }
    }
}
