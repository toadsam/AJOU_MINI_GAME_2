using AjouFestival.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AjouFestival.Games.BalanceWalk
{
    public sealed class BalanceUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Text timeText;
        [SerializeField] private Text bestText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text countdownText;
        [SerializeField] private Button exitButton;

        [Header("Text")]
        [SerializeField] private string hintMessage = "A/D \uB610\uB294 \uBC29\uD5A5\uD0A4: \uADE0\uD615 \uC7A1\uAE30   R: \uB2E4\uC2DC\uD558\uAE30   ESC: \uC120\uD0DD";
        [SerializeField] private string survivalTimeFormat = "\uBC84\uD2F4 \uC2DC\uAC04 {0:0.0}\uCD08";
        [SerializeField] private string bestTimeFormat = "\uCD5C\uACE0 \uC2DC\uAC04 {0:0.0}\uCD08";

        [Header("Countdown")]
        [SerializeField] private float countdownScale = 1.35f;
        [SerializeField] private float startScale = 1.15f;
        [SerializeField] private float startHideDelay = 0.45f;

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
            if (hintText != null) hintText.text = hintMessage;
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

        public void SetSurvivalTime(float seconds)
        {
            if (timeText != null) timeText.text = string.Format(survivalTimeFormat, seconds);
        }

        public void SetBestTime(int bestScore, float scorePerSecond)
        {
            float bestSeconds = scorePerSecond > 0f ? bestScore / scorePerSecond : bestScore;
            if (bestText != null) bestText.text = string.Format(bestTimeFormat, bestSeconds);
        }

        public void SetCountdown(string message, bool isStartMessage = false)
        {
            if (countdownText == null)
            {
                return;
            }

            countdownText.gameObject.SetActive(true);
            countdownText.text = message;
            countdownText.transform.localScale = Vector3.one * (isStartMessage ? startScale : countdownScale);
            hideCountdownAt = isStartMessage ? Time.time + startHideDelay : 0f;
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
