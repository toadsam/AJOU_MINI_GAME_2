using System.Collections.Generic;
using UnityEngine;

namespace AjouFestival.Games.BalanceWalk
{
    public sealed class BalanceParallaxLoop : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float parallaxFactor = 0.35f;
        [SerializeField] private float tileWidth = 24f;
        [SerializeField] private int tileCount = 6;
        [SerializeField] private bool autoTileWidthFromSprite = true;

        [Header("Single Sprite Auto Tiling")]
        [SerializeField] private bool autoCreateTiles;
        [SerializeField, Min(1)] private int generatedTileCount = 4;
        [SerializeField] private bool centerGeneratedTiles = true;

        private float startX;
        private float targetStartX;
        private float wrapSpanWidth;
        private readonly List<Transform> tiles = new();
        private readonly List<float> baseOffsets = new();
        private readonly List<int> wrapCounts = new();

        private void Start()
        {
            startX = transform.position.x;
            if (target == null)
            {
                BalancePlayerController player = FindFirstObjectByType<BalancePlayerController>();
                if (player != null) target = player.transform;
            }

            targetStartX = target != null ? target.position.x : 0f;
            TryResolveTileWidth();
            InitializeTiles();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            float travelled = target.position.x - targetStartX;
            float anchorX = startX + travelled * parallaxFactor;

            if (tiles.Count == 0)
            {
                Vector3 position = transform.position;
                position.x = anchorX;
                transform.position = position;
                return;
            }

            float recycleThreshold = target.position.x - tileWidth;
            for (int i = 0; i < tiles.Count; i++)
            {
                Transform tile = tiles[i];
                if (tile == null)
                {
                    continue;
                }

                float x = anchorX + baseOffsets[i] + wrapCounts[i] * wrapSpanWidth;
                float rightEdge = x + tileWidth * 0.5f;
                while (rightEdge < recycleThreshold)
                {
                    wrapCounts[i]++;
                    x += wrapSpanWidth;
                    rightEdge += wrapSpanWidth;
                }

                Vector3 position = tile.position;
                position.x = x;
                tile.position = position;
            }
        }

        private void TryResolveTileWidth()
        {
            if (!autoTileWidthFromSprite)
            {
                return;
            }

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return;
            }

            tileWidth = spriteRenderer.bounds.size.x;
        }

        private void InitializeTiles()
        {
            tiles.Clear();
            baseOffsets.Clear();
            wrapCounts.Clear();

            if (autoCreateTiles)
            {
                CreateGeneratedTiles();
                return;
            }

            tiles.Add(transform);
            baseOffsets.Add(0f);
            wrapCounts.Add(0);
            wrapSpanWidth = tileWidth * Mathf.Max(1, tileCount);
        }

        private void CreateGeneratedTiles()
        {
            int totalCount = Mathf.Max(1, generatedTileCount);
            int originalIndex = centerGeneratedTiles ? totalCount / 2 : 0;

            for (int i = 0; i < totalCount; i++)
            {
                Transform tile = i == originalIndex ? transform : CreateTileClone(i);
                if (tile == null)
                {
                    continue;
                }

                float offset = (i - originalIndex) * tileWidth;
                Vector3 position = tile.position;
                position.x = startX + offset;
                tile.position = position;

                tiles.Add(tile);
                baseOffsets.Add(offset);
                wrapCounts.Add(0);
            }

            wrapSpanWidth = tileWidth * Mathf.Max(1, tiles.Count);
        }

        private Transform CreateTileClone(int index)
        {
            GameObject clone = Instantiate(gameObject, transform.parent);
            clone.name = $"{name}_Tile_{index}";

            BalanceParallaxLoop cloneLoop = clone.GetComponent<BalanceParallaxLoop>();
            if (cloneLoop != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(cloneLoop);
                }
                else
                {
                    DestroyImmediate(cloneLoop);
                }
            }

            return clone.transform;
        }
    }
}
