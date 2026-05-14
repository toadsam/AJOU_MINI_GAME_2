using UnityEngine;

namespace AjouFestival.Games.BalanceWalk
{
    public sealed class BalanceDistanceCueLoop : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private TextMesh distanceLabel;
        [SerializeField] private float spacing = 8f;
        [SerializeField] private int cueCount = 24;
        [SerializeField] private float recycleBehind = 16f;
        [SerializeField] private float labelMetersPerUnit = 1f;

        private void Start()
        {
            if (target == null)
            {
                BalancePlayerController player = FindFirstObjectByType<BalancePlayerController>();
                if (player != null) target = player.transform;
            }

            if (distanceLabel == null) distanceLabel = GetComponentInChildren<TextMesh>();
            RefreshLabel();
        }

        private void Update()
        {
            if (target == null)
            {
                return;
            }

            while (transform.position.x < target.position.x - recycleBehind)
            {
                transform.position += Vector3.right * spacing * Mathf.Max(1, cueCount);
                RefreshLabel();
            }
        }

        private void RefreshLabel()
        {
            if (distanceLabel == null)
            {
                return;
            }

            int meters = Mathf.Max(0, Mathf.RoundToInt(transform.position.x * labelMetersPerUnit));
            distanceLabel.text = $"{meters}m";
        }
    }
}
