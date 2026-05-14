using UnityEngine;
using UnityEngine.UI;

namespace AjouFestival.Games.BalanceWalk
{
    public sealed class BalanceMeterUI : MonoBehaviour
    {
        [SerializeField] private BalancePlayerController player;
        [SerializeField] private RectTransform needle;
        [SerializeField] private Image safeZoneImage;
        [SerializeField] private Image dangerZoneImage;

        private void Start()
        {
            if (player == null) player = FindFirstObjectByType<BalancePlayerController>();
        }

        private void Update()
        {
            if (player == null || needle == null)
            {
                return;
            }

            float normalized = Mathf.Clamp(player.CurrentAngle / player.MaxSafeAngle, -1f, 1f);
            needle.localRotation = Quaternion.Euler(0f, 0f, -normalized * 70f);

            bool danger = Mathf.Abs(normalized) > 0.72f;
            if (safeZoneImage != null) safeZoneImage.enabled = !danger;
            if (dangerZoneImage != null) dangerZoneImage.enabled = danger;
        }
    }
}
