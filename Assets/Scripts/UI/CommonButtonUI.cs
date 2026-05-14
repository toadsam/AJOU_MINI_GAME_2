using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AjouFestival.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class CommonButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float hoverScale = 1.04f;
        [SerializeField] private Color normalColor = new Color(0.08f, 0.42f, 0.86f, 0.95f);
        [SerializeField] private Color hoverColor = new Color(0.12f, 0.72f, 1f, 1f);

        private Button button;
        private Graphic targetGraphic;
        private Vector3 baseScale;

        private void Awake()
        {
            button = GetComponent<Button>();
            targetGraphic = button.targetGraphic;
            baseScale = transform.localScale;
            if (targetGraphic != null)
            {
                targetGraphic.color = normalColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.localScale = baseScale * hoverScale;
            if (targetGraphic != null)
            {
                targetGraphic.color = hoverColor;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.localScale = baseScale;
            if (targetGraphic != null)
            {
                targetGraphic.color = normalColor;
            }
        }
    }
}
