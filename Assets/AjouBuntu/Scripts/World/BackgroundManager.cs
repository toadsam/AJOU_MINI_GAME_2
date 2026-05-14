using AjouBuntu.Config;
using AjouBuntu.Core;
using UnityEngine;

namespace AjouBuntu.World
{
    public sealed class BackgroundManager : MonoBehaviour
    {
        private GameConfig config;
        private Transform dayLayer;
        private Transform lunchLayer;
        private Transform eveningLayer;
        private SpriteRenderer[] dayRenderers;
        private SpriteRenderer[] lunchRenderers;
        private SpriteRenderer[] eveningRenderers;
        private SpriteRenderer[] lightRenderers;
        private float scroll;

        public void Initialize(GameConfig gameConfig, bool decorativeMenu)
        {
            config = gameConfig;
            dayLayer = CreateTiledLayer("Day", config.bgCampusDay, new Color(0.78f, 0.95f, 1f), new Color(0.35f, 0.68f, 0.95f), 0);
            lunchLayer = CreateTiledLayer("Lunch", config.bgCampusSunset, new Color(0.99f, 0.93f, 0.72f), new Color(0.66f, 0.82f, 1f), 1);
            eveningLayer = CreateTiledLayer("Evening", config.bgCampusNight, new Color(0.98f, 0.62f, 0.34f), new Color(0.16f, 0.11f, 0.32f), 2);

            dayRenderers = dayLayer.GetComponentsInChildren<SpriteRenderer>();
            lunchRenderers = lunchLayer.GetComponentsInChildren<SpriteRenderer>();
            eveningRenderers = eveningLayer.GetComponentsInChildren<SpriteRenderer>();
            SetLayerAlpha(dayRenderers, 1f);
            SetLayerAlpha(lunchRenderers, decorativeMenu ? 0.08f : 0f);
            SetLayerAlpha(eveningRenderers, 0f);

            CreateCampusSilhouette();
            CreateLightDots();
            SetLayerAlpha(lightRenderers, decorativeMenu ? 0.08f : 0f);
        }

        public void SetProgress(float progress, float speed)
        {
            scroll += speed * 0.2f * Time.deltaTime;
            ScrollLayer(dayLayer, scroll * 0.35f);
            ScrollLayer(lunchLayer, scroll * 0.32f);
            ScrollLayer(eveningLayer, scroll * 0.28f);

            float dayAlpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.28f, 0.42f, progress));
            float lunchFadeIn = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.28f, 0.42f, progress));
            float lunchFadeOut = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.66f, 0.80f, progress));
            float lunchAlpha = lunchFadeIn * (1f - lunchFadeOut);
            float eveningAlpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.66f, 0.82f, progress));

            SetLayerAlpha(dayRenderers, dayAlpha);
            SetLayerAlpha(lunchRenderers, lunchAlpha);
            SetLayerAlpha(eveningRenderers, eveningAlpha);
            SetLayerAlpha(lightRenderers, eveningAlpha * 0.72f);
        }

        private Transform CreateTiledLayer(string name, Sprite sprite, Color top, Color bottom, int order)
        {
            GameObject layer = new GameObject($"Background_{name}");
            layer.transform.SetParent(transform, false);

            Sprite layerSprite = sprite != null ? sprite : RuntimeSpriteFactory.CreateGradientSprite(top, bottom);
            for (int i = 0; i < 2; i++)
            {
                GameObject tile = new GameObject($"{name}_Tile_{i}");
                tile.transform.SetParent(layer.transform, false);
                tile.transform.position = new Vector3(config.logicalSize.x * (0.5f + i), config.logicalSize.y * 0.5f, 10f + order);
                SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
                renderer.sprite = layerSprite;
                renderer.sortingOrder = -50 + order;
                FitSprite(renderer, config.logicalSize);
            }

            return layer.transform;
        }

        private void ScrollLayer(Transform layer, float amount)
        {
            float width = config.logicalSize.x;
            float offset = amount % width;
            for (int i = 0; i < layer.childCount; i++)
            {
                Transform child = layer.GetChild(i);
                child.position = new Vector3(width * (0.5f + i) - offset, config.logicalSize.y * 0.5f, child.position.z);
            }
        }

        private void CreateCampusSilhouette()
        {
            Color silhouette = new Color(0.03f, 0.15f, 0.25f, 0.42f);
            for (int i = 0; i < 10; i++)
            {
                GameObject building = new GameObject($"CampusSilhouette_{i}");
                building.transform.SetParent(transform, false);
                float width = Random.Range(70f, 150f);
                float height = Random.Range(70f, 180f);
                building.transform.position = new Vector3(i * 115f + 28f, height * 0.5f, 0f);
                SpriteRenderer renderer = building.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeSpriteFactory.CreateSolidSprite(silhouette, 16, 16);
                renderer.sortingOrder = -20;
                building.transform.localScale = new Vector3(width / 16f, height / 16f, 1f);
            }
        }

        private void CreateLightDots()
        {
            lightRenderers = new SpriteRenderer[36];
            for (int i = 0; i < 36; i++)
            {
                GameObject dot = new GameObject($"CampusLight_{i}");
                dot.transform.SetParent(transform, false);
                dot.transform.position = new Vector3(Random.Range(0f, config.logicalSize.x), Random.Range(90f, config.logicalSize.y - 30f), 0f);
                SpriteRenderer renderer = dot.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeSpriteFactory.CreateSolidSprite(new Color(0.75f, 1f, 1f, 0.42f), 6, 6);
                renderer.sortingOrder = -10;
                lightRenderers[i] = renderer;
            }
        }

        private static void SetLayerAlpha(SpriteRenderer[] renderers, float alpha)
        {
            foreach (SpriteRenderer renderer in renderers)
            {
                Color color = renderer.color;
                color.a = alpha;
                renderer.color = color;
            }
        }

        private static void FitSprite(SpriteRenderer renderer, Vector2 targetSize)
        {
            Vector2 size = renderer.sprite.bounds.size;
            if (size.x <= 0f || size.y <= 0f)
            {
                return;
            }

            renderer.transform.localScale = new Vector3(targetSize.x / size.x, targetSize.y / size.y, 1f);
        }
    }
}
