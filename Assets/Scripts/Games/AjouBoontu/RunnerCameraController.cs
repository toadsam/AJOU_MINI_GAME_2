using UnityEngine;

namespace AjouFestival.Games.AjouBoontu
{
    public sealed class RunnerCameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(5f, 1.2f, -10f);
        [SerializeField] private float followSpeed = 8f;

        private void Start()
        {
            if (target == null)
            {
                ChitoRunnerController runner = FindFirstObjectByType<ChitoRunnerController>();
                if (runner != null) target = runner.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desired = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);
        }
    }
}
