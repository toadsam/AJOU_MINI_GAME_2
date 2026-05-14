using AjouFestival.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AjouFestival.Games.BalanceWalk
{
    public sealed class BalanceUI : MonoBehaviour
    {
        [SerializeField] private Text timeText;
        [SerializeField] private Text bestText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text countdownText;
        [SerializeField] private Button exitButton;

        private float hideCountdownAt;

        private void Awake()
        {
            if (timeText == null) timeText = transform.Find("TimeText")?.GetComponent<Text>();
            if (bestText == null) bestText = transform.Find("BestText")?.GetComponent<Text>();
            if (hintText == null) hintText = transform.Find("HintText")?.GetComponent<Text>();
            if (countdownText == null) countdownText = transform.Find("CountdownText")?.GetComponent<Text>();
            if (exitButton == null) exitButton = transform.Find("ExitButton")?.GetComponent<Button>();
        }

        private void Start()
        {
            if (hintText != null) hintText.text = "A/D 또는 ←/→로 균형 잡기   R: 다시하기   ESC: 선택";
            if (exitButton != null) exitButton.onClick.AddListener(SceneLoader.LoadGameSelect);
        }

        private void Update()
        {
            if (hideCountdownAt > 0f && Time.time >= hideCountdownAt)
            {
                HideCountdown();
                hideCountdownAt = 0f;
            }
        }

        public void SetDistance(float meters)
        {
            if (timeText != null) timeText.text = $"이동 거리 {meters:0.0} m";
        }

        public void SetBestDistance(int bestScore, float scorePerMeter)
        {
            float bestMeters = scorePerMeter > 0f ? bestScore / scorePerMeter : bestScore;
            if (bestText != null) bestText.text = $"최고 거리 {bestMeters:0.0} m";
        }

        public void SetCountdown(string message)
        {
            if (countdownText == null)
            {
                return;
            }

            countdownText.gameObject.SetActive(true);
            countdownText.text = message;
            countdownText.transform.localScale = Vector3.one * (message == "Start!" ? 1.15f : 1.35f);
            hideCountdownAt = message == "Start!" ? Time.time + 0.45f : 0f;
        }

        public void HideCountdown()
        {
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(false);
            }
        }
    }
}
