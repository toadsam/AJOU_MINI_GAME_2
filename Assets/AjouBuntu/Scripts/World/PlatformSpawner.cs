using System.Collections.Generic;
using AjouBuntu.Config;
using AjouBuntu.Core;
using UnityEngine;

namespace AjouBuntu.World
{
    public sealed class PlatformSpawner : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private LayerMask platformMask;

        private readonly List<PlatformController> activePlatforms = new();
        private GameManager gameManager;
        private ScoreManager scoreManager;
        private int difficulty = 1;
        private int spawnedCount;
        private float lastRightX;
        private float lastTopScreenY;

        public void Initialize(GameConfig gameConfig, GameManager manager, ScoreManager score, int platformLayer)
        {
            config = gameConfig;
            config.EnsureDefaults();
            gameManager = manager;
            scoreManager = score;
            gameObject.layer = platformLayer;
            platformMask = 1 << platformLayer;
            spawnedCount = 0;

            PlatformController[] preplacedPlatforms = GetComponentsInChildren<PlatformController>();
            if (preplacedPlatforms.Length > 0)
            {
                InitializePreplacedPlatforms(preplacedPlatforms);
            }
            else
            {
                SpawnStartPlatform(platformLayer);
            }

            InitializePreplacedItems();

            while (lastRightX < config.spawnX)
            {
                SpawnNext(platformLayer);
            }
        }

        public void SetDifficulty(int stage)
        {
            difficulty = Mathf.Clamp(stage, 1, 6);
        }

        private void Update()
        {
            if (config == null || gameManager == null || gameManager.IsFinished)
            {
                return;
            }

            activePlatforms.RemoveAll(platform => platform == null);
            float rightMost = float.NegativeInfinity;
            for (int i = 0; i < activePlatforms.Count; i++)
            {
                rightMost = Mathf.Max(rightMost, activePlatforms[i].RightX);
            }

            if (!float.IsNegativeInfinity(rightMost))
            {
                lastRightX = rightMost;
            }

            if (lastRightX < config.spawnX)
            {
                int platformLayer = gameObject.layer;
                SpawnNext(platformLayer);
            }
        }

        private void SpawnStartPlatform(int platformLayer)
        {
            lastTopScreenY = config.playerStartScreenPosition.y + 1f;
            float width = 520f;
            float topWorldY = config.WorldYFromScreenY(lastTopScreenY);
            PlatformController platform = CreatePlatform(platformLayer, new Vector2(260f, topWorldY), width, topWorldY);
            lastRightX = platform.RightX;
            spawnedCount++;
        }

        private void InitializePreplacedPlatforms(PlatformController[] preplacedPlatforms)
        {
            activePlatforms.Clear();
            lastRightX = float.NegativeInfinity;
            for (int i = 0; i < preplacedPlatforms.Length; i++)
            {
                PlatformController platform = preplacedPlatforms[i];
                platform.gameObject.layer = gameObject.layer;
                platform.InitializeFromScene(config, gameManager, config.despawnX);
                activePlatforms.Add(platform);

                if (platform.RightX > lastRightX)
                {
                    lastRightX = platform.RightX;
                    lastTopScreenY = platform.TopScreenY;
                }
            }

            spawnedCount = activePlatforms.Count;
        }

        private void InitializePreplacedItems()
        {
            ItemController[] preplacedItems = GetComponentsInChildren<ItemController>();
            for (int i = 0; i < preplacedItems.Length; i++)
            {
                preplacedItems[i].InitializeFromScene(config, scoreManager, gameManager);
            }
        }

        private void SpawnNext(int platformLayer)
        {
            float normalized = Mathf.InverseLerp(1f, 6f, difficulty);
            float gapMin = Mathf.Lerp(config.easyGapMin, config.hardGapMin, normalized);
            float gapMax = Mathf.Lerp(config.easyGapMax, config.hardGapMax, normalized);
            if (spawnedCount < 6)
            {
                gapMin = 105f;
                gapMax = 160f;
            }

            float width = Random.Range(config.platformMinWidth, config.platformMaxWidth);
            float gap = Random.Range(gapMin, gapMax);
            float centerX = lastRightX + gap + width * 0.5f;
            float topScreenY = PickNextTopScreenY();
            float topWorldY = config.WorldYFromScreenY(topScreenY);

            PlatformController next = CreatePlatform(platformLayer, new Vector2(centerX, topWorldY), width, topWorldY);
            SpawnItemsBetween(lastRightX, next.LeftX, lastTopScreenY, topScreenY);
            SpawnItemsOnPlatform(next);

            lastRightX = next.RightX;
            lastTopScreenY = topScreenY;
            spawnedCount++;
        }

        private float PickNextTopScreenY()
        {
            if (spawnedCount < 6)
            {
                float[] easy = { 438f, 422f, 440f, 408f, 432f, 410f };
                return easy[Mathf.Min(spawnedCount, easy.Length - 1)];
            }

            float maxDelta = Mathf.Lerp(42f, 92f, Mathf.InverseLerp(1f, 6f, difficulty));
            float delta = Random.Range(-maxDelta, maxDelta);
            float result = lastTopScreenY + delta;
            return Mathf.Clamp(result, 230f, config.platformTopMaxScreenY);
        }

        private PlatformController CreatePlatform(int platformLayer, Vector2 center, float width, float topWorldY)
        {
            GameObject obj = new GameObject("Platform_StoneBridge");
            obj.layer = platformLayer;
            obj.transform.SetParent(transform, false);
            obj.transform.position = new Vector3(center.x, topWorldY, 0f);

            PlatformDefinition definition = config.GetPlatform(PlatformKind.StoneBridge);
            PlatformController platform = obj.AddComponent<PlatformController>();
            platform.Initialize(definition, gameManager, width, topWorldY, config.despawnX);
            activePlatforms.Add(platform);
            return platform;
        }

        private void SpawnItemsBetween(float fromRightX, float toLeftX, float fromTopScreenY, float toTopScreenY)
        {
            float span = toLeftX - fromRightX;
            if (span < 90f)
            {
                return;
            }

            CoinArcProfile profile = difficulty >= 5 ? CoinArcProfile.Tight : CoinArcProfile.Safe;
            int count = profile == CoinArcProfile.Safe ? 5 : 4;
            float arcLift = profile == CoinArcProfile.Safe ? 92f : 116f;

            for (int i = 0; i < count; i++)
            {
                float t = (i + 1f) / (count + 1f);
                float x = Mathf.Lerp(fromRightX + 28f, toLeftX - 28f, t);
                float screenY = Mathf.Lerp(fromTopScreenY - 72f, toTopScreenY - 72f, t) - Mathf.Sin(t * Mathf.PI) * arcLift;
                SpawnItem(PickMostlyCoffee(), new Vector3(x, config.WorldYFromScreenY(screenY), 0f));
            }
        }

        private void SpawnItemsOnPlatform(PlatformController platform)
        {
            for (int i = 0; i < 4; i++)
            {
                float x = platform.LeftX + 72f + i * 42f;
                float y = platform.TopWorldY + 46f;
                SpawnItem(i == 3 && spawnedCount % 6 == 0 ? ItemKind.APlus : ItemKind.Coffee, new Vector3(x, y, 0f));
            }
        }

        private ItemKind PickMostlyCoffee()
        {
            return Random.value < 0.9f ? ItemKind.Coffee : ItemKind.APlus;
        }

        private void SpawnItem(ItemKind kind, Vector3 position)
        {
            GameObject item = new GameObject($"Item_{kind}");
            item.transform.SetParent(transform, false);
            item.transform.position = position;
            ItemController controller = item.AddComponent<ItemController>();
            controller.Initialize(config.GetItem(kind), scoreManager, gameManager);
        }
    }
}
