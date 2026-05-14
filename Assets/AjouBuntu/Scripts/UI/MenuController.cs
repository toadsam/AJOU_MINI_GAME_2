using System.Collections;
using AjouBuntu.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AjouBuntu.UI
{
    public sealed class MenuController : MonoBehaviour
    {
        private GameObject helpPanel;

        public void Build(Canvas canvas)
        {
            Font font = UiFactory.GetDefaultFont();

            Text title = UiFactory.CreateText(canvas.transform, "Title", font, 62, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f), new Vector2(520f, 88f), Vector2.zero);
            title.text = "아주분투";
            title.color = new Color(0.84f, 1f, 1f, 1f);

            Text subtitle = UiFactory.CreateText(canvas.transform, "Subtitle", font, 25, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(520f, 42f), Vector2.zero);
            subtitle.text = "A+를 향한 캠퍼스 질주";
            subtitle.color = new Color(0.74f, 0.93f, 1f, 0.95f);

            Button start = UiFactory.CreateButton(canvas.transform, "StartButton", "게임 시작", new Vector2(0f, -70f));
            start.onClick.AddListener(SceneTransitionManager.LoadGame);

            Button howTo = UiFactory.CreateButton(canvas.transform, "HowToButton", "조작 방법", new Vector2(0f, -140f));
            howTo.onClick.AddListener(ToggleHelp);

            BuildHelpPanel(canvas, font);
            StartCoroutine(PulseTitle(title));
        }

        private void BuildHelpPanel(Canvas canvas, Font font)
        {
            Image panel = UiFactory.CreatePanel(canvas.transform, "HowToPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(520f, 190f), new Vector2(0f, 50f));
            panel.color = new Color(0.02f, 0.12f, 0.22f, 0.78f);
            helpPanel = panel.gameObject;
            helpPanel.SetActive(false);

            Text help = UiFactory.CreateText(panel.transform, "HowToText", font, 24, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            help.rectTransform.offsetMin = new Vector2(24f, 24f);
            help.rectTransform.offsetMax = new Vector2(-24f, -24f);
            help.text = "클릭 / 터치 / 스페이스바: 점프\n현재 핵심 플레이: 더블 점프";
            help.lineSpacing = 1.25f;
        }

        private void ToggleHelp()
        {
            helpPanel.SetActive(!helpPanel.activeSelf);
        }

        private static IEnumerator PulseTitle(Text title)
        {
            while (title != null)
            {
                float scale = 1f + Mathf.Sin(Time.time * 2.2f) * 0.018f;
                title.transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
        }
    }
}
