using System.Collections.Generic;
using UnityEngine;

namespace AjouFestival.Games.AjouBoontu
{
    public sealed class RunnerPlatformSpawner : MonoBehaviour
    {
        [SerializeField] private bool autoSpawn = true;
        [SerializeField] private ChitoRunnerController runner;
        [SerializeField] private List<GameObject> platformPrefabs = new();
        [SerializeField] private float spawnAheadDistance = 18f;
        [SerializeField] private float despawnBehindDistance = 12f;
        [SerializeField] private float minGap = 3.2f;
        [SerializeField] private float maxGap = 5.5f;
        [SerializeField] private Vector2 yRange = new Vector2(-2.5f, 0.8f);

        private readonly List<GameObject> spawned = new();
        private float nextSpawnX;

        private void Start()
        {
            if (runner == null) runner = FindFirstObjectByType<ChitoRunnerController>();
            nextSpawnX = runner != null ? runner.transform.position.x + 4f : 4f;
        }

        private void Update()
        {
            if (!autoSpawn || runner == null || platformPrefabs.Count == 0)
            {
                return;
            }

            while (nextSpawnX < runner.transform.position.x + spawnAheadDistance)
            {
                SpawnPlatform();
            }

            for (int i = spawned.Count - 1; i >= 0; i--)
            {
                if (spawned[i] == null)
                {
                    spawned.RemoveAt(i);
                    continue;
                }

                if (spawned[i].transform.position.x < runner.transform.position.x - despawnBehindDistance)
                {
                    Destroy(spawned[i]);
                    spawned.RemoveAt(i);
                }
            }
        }

        private void SpawnPlatform()
        {
            GameObject prefab = platformPrefabs[Random.Range(0, platformPrefabs.Count)];
            float y = Random.Range(yRange.x, yRange.y);
            GameObject platform = Instantiate(prefab, new Vector3(nextSpawnX, y, 0f), Quaternion.identity, transform);
            spawned.Add(platform);

            float difficulty = runner != null ? Mathf.Clamp01(runner.transform.position.x / 120f) : 0f;
            float gap = Random.Range(Mathf.Lerp(minGap, minGap + 1.1f, difficulty), Mathf.Lerp(maxGap, maxGap + 1.8f, difficulty));
            nextSpawnX += gap;
        }
    }
}
