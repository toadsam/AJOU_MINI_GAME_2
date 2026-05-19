using UnityEngine;

namespace AjouFestival.Games.BalanceWalk
{
    public sealed class BalanceCameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(2.8f, 0.4f, -10f);
        [SerializeField] private float followSpeed = 3.2f;
        [SerializeField] private bool followXOnly = true;
        [SerializeField] private bool useSceneOffsetOnStart = true;

        private float fixedY;
        private BalanceWalkGameManager gameManager;
        private bool hasStartedFollowing;

        private void Start()
        {
            fixedY = transform.position.y;
            gameManager = FindFirstObjectByType<BalanceWalkGameManager>();
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
            if (gameManager != null && !gameManager.HasStarted)
            {
                return;
            }

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

            if (!hasStartedFollowing)
            {
                if (useSceneOffsetOnStart)
                {
                    offset = transform.position - target.position;
                }

                fixedY = transform.position.y;
                hasStartedFollowing = true;
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
