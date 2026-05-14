using UnityEngine;

namespace AjouFestival.Core
{
    public sealed class GlobalShortcutHandler : MonoBehaviour
    {
        [SerializeField] private bool escapeToGameSelect = true;
        [SerializeField] private bool rToRestart = true;

        private void Update()
        {
            if (escapeToGameSelect && FestivalInput.GetKeyDown(KeyCode.Escape))
            {
                SceneLoader.LoadGameSelect();
            }

            if (rToRestart && FestivalInput.GetKeyDown(KeyCode.R))
            {
                SceneLoader.RestartLastGame();
            }
        }
    }
}
