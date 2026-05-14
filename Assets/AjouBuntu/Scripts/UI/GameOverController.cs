using AjouBuntu.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AjouBuntu.UI
{
    public sealed class GameOverController : MonoBehaviour
    {
        public void Build(Canvas canvas)
        {
            Font font = UiFactory.GetDefaultFont();
            bool cleared = GameSessionResult.HasResult && GameSessionResult.Cleared;

            Text title = UiFactory.CreateText(canvas.transform, "ResultTitle", font, 54, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), new Vector2(560f, 74f), Vector2.zero);
            title.text = cleared ? "미션 클리어" : "게임 오버";
            title.color = cleared ? new Color(0.85f, 1f, 0.55f, 1f) : new Color(1f, 0.78f, 0.78f, 1f);

            Image panel = UiFactory.CreatePanel(canvas.transform, "ResultPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(520f, 200f), new Vector2(0f, 10f));
            panel.color = new Color(0.02f, 0.12f, 0.22f, 0.72f);

            string message = cleared ? "목표 거리에 도달했습니다." : "캠퍼스 아래로 떨어졌습니다.";
            Text info = UiFactory.CreateText(panel.transform, "ResultInfo", font, 25, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            info.rectTransform.offsetMin = new Vector2(24f, 20f);
            info.rectTransform.offsetMax = new Vector2(-24f, -20f);
            info.text = $"{message}\n점수 {GameSessionResult.Score:N0}\n최고 점수 {GameSessionResult.HighScore:N0}\n획득한 A+ {GameSessionResult.APlusCount}";
            info.lineSpacing = 1.25f;

            Button retry = UiFactory.CreateButton(canvas.transform, "RetryButton", "다시 시작", new Vector2(-130f, -170f));
            retry.onClick.AddListener(SceneTransitionManager.LoadGame);

            Button menu = UiFactory.CreateButton(canvas.transform, "MenuButton", "메인 메뉴", new Vector2(130f, -170f));
            menu.onClick.AddListener(SceneTransitionManager.LoadMenu);
        }
    }
}
