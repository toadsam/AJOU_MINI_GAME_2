using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AjouBuntu.UI
{
    public sealed class FloatingText : MonoBehaviour
    {
        public static void Spawn(string message, Vector3 worldPosition)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                canvas = UiFactory.CreateCanvas();
            }

            Camera camera = Camera.main;
            Vector2 screenPosition = camera != null ? RectTransformUtility.WorldToScreenPoint(camera, worldPosition) : new Vector2(worldPosition.x, worldPosition.y);
            Text text = UiFactory.CreateText(canvas.transform, "FloatingText", UiFactory.GetDefaultFont(), 24, TextAnchor.MiddleCenter, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0.5f, 0.5f), new Vector2(120f, 36f), screenPosition);
            text.color = new Color(0.9f, 1f, 0.55f, 1f);
            FloatingText component = text.gameObject.AddComponent<FloatingText>();
            text.text = message;
            component.StartCoroutine(component.Run(text));
        }

        private IEnumerator Run(Text text)
        {
            float time = 0f;
            Vector2 start = text.rectTransform.anchoredPosition;
            while (time < 0.55f)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / 0.55f);
                text.rectTransform.anchoredPosition = start + Vector2.up * Mathf.Lerp(0f, 42f, t);
                Color color = text.color;
                color.a = 1f - t;
                text.color = color;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
