using UnityEngine.SceneManagement;

namespace AjouBuntu.Core
{
    public static class SceneTransitionManager
    {
        public const string BootScene = "Boot";
        public const string MenuScene = "Menu";
        public const string GameScene = "Game";
        public const string GameOverScene = "GameOver";

        public static void LoadBoot() => SceneManager.LoadScene(BootScene);
        public static void LoadMenu() => SceneManager.LoadScene(MenuScene);
        public static void LoadGame() => SceneManager.LoadScene(GameScene);
        public static void LoadGameOver() => SceneManager.LoadScene(GameOverScene);
    }
}
