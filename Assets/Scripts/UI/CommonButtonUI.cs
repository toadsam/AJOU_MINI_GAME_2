using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using AjouFestival.Core;

namespace AjouFestival.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class CommonButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISubmitHandler
    {
        [SerializeField] private float hoverScale = 1.04f;
        [SerializeField] private bool tintTargetGraphic = true;
        [SerializeField] private Color normalColor = new Color(0.08f, 0.42f, 0.86f, 0.95f);
        [SerializeField] private Color hoverColor = new Color(0.12f, 0.72f, 1f, 1f);
        [Header("SFX")]
        [SerializeField] private AudioClip clickSfx;
        [SerializeField] private AudioClip hoverSfx;
        [SerializeField, Range(0f, 1f)] private float clickSfxVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float hoverSfxVolume = 0.6f;

        private Button button;
        private Graphic targetGraphic;
        private Vector3 baseScale;

        private void Awake()
        {
            button = GetComponent<Button>();
            targetGraphic = button.targetGraphic;
            baseScale = transform.localScale;
            if (tintTargetGraphic && targetGraphic != null)
            {
                targetGraphic.color = normalColor;
            }

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button != null && !button.interactable)
            {
                return;
            }

            transform.localScale = baseScale * hoverScale;
            if (tintTargetGraphic && targetGraphic != null)
            {
                targetGraphic.color = hoverColor;
            }

            PlayHoverSfx();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.localScale = baseScale;
            if (tintTargetGraphic && targetGraphic != null)
            {
                targetGraphic.color = normalColor;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            PlayClickSfx();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            PlayClickSfx();
        }

        public void SetTintTargetGraphic(bool enabled)
        {
            tintTargetGraphic = enabled;
        }

        private void PlayClickSfx()
        {
            if (clickSfx == null || button == null || !button.interactable)
            {
                return;
            }

            AudioManager.Ensure().PlaySfx(clickSfx, clickSfxVolume);
        }

        private void PlayHoverSfx()
        {
            if (hoverSfx == null)
            {
                return;
            }

            AudioManager.Ensure().PlaySfx(hoverSfx, hoverSfxVolume);
        }
    }
}
