using UnityEngine;

namespace AjouFestival.Games.Soccer
{
    public sealed class SoccerGoal : MonoBehaviour
    {
        [SerializeField] private int scoringPlayer = 1;
        [SerializeField] private SoccerGameManager gameManager;

        private void Start()
        {
            if (gameManager == null) gameManager = FindFirstObjectByType<SoccerGameManager>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<SoccerBallController>() != null)
            {
                gameManager?.AddGoal(scoringPlayer);
            }
        }
    }
}
