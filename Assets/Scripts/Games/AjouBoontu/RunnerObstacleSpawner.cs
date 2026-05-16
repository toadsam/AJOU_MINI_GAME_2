using System.Collections.Generic;
using UnityEngine;

namespace AjouFestival.Games.AjouBoontu
{
    public sealed class RunnerObstacleSpawner : MonoBehaviour
    {
        [SerializeField] private bool autoSpawn = true;
        [SerializeField] private ChitoRunnerController runner;
        [SerializeField] private List<GameObject> obstaclePrefabs = new();
        [SerializeField] private float spawnAheadDistance = 22f;
        [SerializeField] private float spawnInterval = 7f;
        [SerializeField] private float baseSpawnChance = 0.25f;
        [SerializeField] private float groundY = -1.45f;

        private float nextSpawnX;

        private void Start()
        {
            if (runner == null) runner = FindFirstObjectByType<ChitoRunnerController>();
            nextSpawnX = runner != null ? runner.transform.position.x + 12f : 12f;
        }

        private void Update()
        {
            if (!autoSpawn || runner == null || obstaclePrefabs.Count == 0)
            {
                return;
            }

            if (nextSpawnX < runner.transform.position.x + spawnAheadDistance)
            {
                float difficulty = Mathf.Clamp01(runner.transform.position.x / 150f);
                if (Random.value < baseSpawnChance + difficulty * 0.35f)
                {
                    GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
                    Instantiate(prefab, new Vector3(nextSpawnX, groundY, 0f), Quaternion.identity, transform);
                }

                nextSpawnX += spawnInterval;
            }
        }
    }
}
