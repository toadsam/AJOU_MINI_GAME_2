using UnityEngine;

namespace AjouFestival.Games.BalanceWalk
{
    public sealed class BalanceObstacleSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject obstaclePrefab;
        [SerializeField] private float interval = 4f;
        [SerializeField] private float xSpawn = 8f;
        [SerializeField] private float y = -2.4f;

        private float timer;

        private void Update()
        {
            if (obstaclePrefab == null)
            {
                return;
            }

            timer += Time.deltaTime;
            if (timer >= interval)
            {
                timer = 0f;
                Instantiate(obstaclePrefab, new Vector3(xSpawn, y, 0f), Quaternion.identity, transform);
            }
        }
    }
}
