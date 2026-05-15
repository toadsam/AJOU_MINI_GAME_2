using AjouFestival.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AjouFestival.UI
{
    public sealed class GameSelectUI : MonoBehaviour
    {
        [SerializeField] private Button ajouBoontuButton;
        [SerializeField] private Button balanceWalkButton;
        [SerializeField] private Button soccerButton;
        [SerializeField] private Button mainMenuButton;

        private void Awake()
        {
            if (ajouBoontuButton == null) ajouBoontuButton = transform.Find("AjouBoontuCard/StartButton")?.GetComponent<Button>();
            if (balanceWalkButton == null) balanceWalkButton = transform.Find("BalanceWalkCard/StartButton")?.GetComponent<Button>();
            if (soccerButton == null) soccerButton = transform.Find("SoccerCard/StartButton")?.GetComponent<Button>();
            if (mainMenuButton == null) mainMenuButton = transform.Find("MainMenuButton")?.GetComponent<Button>();
        }

        private void Start()
        {
            GameSessionManager.Ensure();
            if (ajouBoontuButton != null) ajouBoontuButton.onClick.AddListener(SceneLoader.LoadAjouBoontu);
            if (balanceWalkButton != null) balanceWalkButton.onClick.AddListener(SceneLoader.LoadBalanceWalk);
            if (soccerButton != null) soccerButton.onClick.AddListener(() => SceneLoader.LoadSoccer());
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(SceneLoader.LoadMainMenu);
        }
    }
}
