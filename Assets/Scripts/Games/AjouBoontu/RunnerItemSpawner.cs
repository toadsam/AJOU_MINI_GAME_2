using System.Collections.Generic;
using UnityEngine;

namespace AjouFestival.Games.AjouBoontu
{
    public sealed class RunnerItemSpawner : MonoBehaviour
    {
        [SerializeField] private ChitoRunnerController runner;
        [SerializeField] private List<GameObject> itemPrefabs = new();
        [SerializeField] private float spawnAheadDistance = 20f;
        [SerializeField] private float spawnInterval = 4.5f;
        [SerializeField] private Vector2 yRange = new Vector2(0.5f, 2.6f);

        private float nextSpawnX;

        private void Start()
        {
            if (runner == null) runner = FindFirstObjectByType<ChitoRunnerController>();
            nextSpawnX = runner != null ? runner.transform.position.x + 7f : 7f;
        }

        private void Update()
        {
            if (runner == null || itemPrefabs.Count == 0)
            {
                return;
            }

            if (nextSpawnX < runner.transform.position.x + spawnAheadDistance)
            {
                GameObject prefab = itemPrefabs[Random.Range(0, itemPrefabs.Count)];
                Instantiate(prefab, new Vector3(nextSpawnX, Random.Range(yRange.x, yRange.y), 0f), Quaternion.identity, transform);
                nextSpawnX += spawnInterval;
            }
        }
    }
}
