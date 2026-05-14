using AjouFestival.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AjouFestival.UI
{
    public sealed class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button gameSelectButton;
        [SerializeField] private Button howToButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject howToPanel;

        private void Awake()
        {
            if (gameSelectButton == null) gameSelectButton = transform.Find("GameSelectButton")?.GetComponent<Button>();
            if (howToButton == null) howToButton = transform.Find("HowToButton")?.GetComponent<Button>();
            if (quitButton == null) quitButton = transform.Find("QuitButton")?.GetComponent<Button>();
            if (howToPanel == null) howToPanel = transform.Find("HowToPanel")?.gameObject;
        }

        private void Start()
        {
            GameSessionManager.Ensure();
            AudioManager.Ensure();

            if (gameSelectButton != null) gameSelectButton.onClick.AddListener(SceneLoader.LoadGameSelect);
            if (howToButton != null) howToButton.onClick.AddListener(ToggleHowTo);
            if (quitButton != null) quitButton.onClick.AddListener(SceneLoader.QuitGame);
            if (howToPanel != null) howToPanel.SetActive(false);
        }

        private void ToggleHowTo()
        {
            if (howToPanel != null)
            {
                howToPanel.SetActive(!howToPanel.activeSelf);
            }
        }
    }
}
