using UnityEngine;

namespace AjouBuntu.Core
{
    public static class RuntimeSpriteFactory
    {
        public static Sprite CreateSolidSprite(Color color, int width = 64, int height = 64)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.name = "RuntimeSolidSprite";
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 1f);
        }

        public static Sprite CreateGradientSprite(Color top, Color bottom, int width = 960, int height = 540)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.name = "RuntimeGradientSprite";
            for (int y = 0; y < height; y++)
            {
                float t = y / (float)(height - 1);
                Color row = Color.Lerp(bottom, top, t);
                for (int x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, row);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 1f);
        }

        public static Sprite CreateCapsuleSprite(Color bodyColor, Color outlineColor, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.name = "RuntimeCapsuleSprite";
            Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
            float radiusX = width * 0.38f;
            float radiusY = height * 0.45f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 p = new Vector2(x, y);
                    float normalized = Mathf.Pow((p.x - center.x) / radiusX, 2f) + Mathf.Pow((p.y - center.y) / radiusY, 2f);
                    if (normalized <= 1f)
                    {
                        texture.SetPixel(x, y, normalized > 0.82f ? outlineColor : bodyColor);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.1f), 1f);
        }

        public static Sprite CreateRoundedRectSprite(Color fill, Color edge, int width = 128, int height = 48, int radius = 14)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.name = "RuntimeRoundedRectSprite";
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = Mathf.Max(radius - x, 0, x - (width - radius - 1));
                    float dy = Mathf.Max(radius - y, 0, y - (height - radius - 1));
                    bool inside = dx * dx + dy * dy <= radius * radius;
                    if (!inside)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    bool border = x < 3 || y < 3 || x > width - 4 || y > height - 4 || dx * dx + dy * dy > (radius - 3) * (radius - 3);
                    texture.SetPixel(x, y, border ? edge : fill);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }
    }
}
