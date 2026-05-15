using UnityEngine;
using UnityEngine.SceneManagement;

namespace AjouFestival.Core
{
    public sealed class SceneLoader : MonoBehaviour
    {
        public const string MainMenuScene = "00_MainMenu";
        public const string GameSelectScene = "01_GameSelect";
        public const string AjouBoontuScene = "02_AjouBoontu";
        public const string BalanceWalkScene = "03_BalanceWalk";
        public const string SoccerScene = "04_OneVsOneSoccer";
        public const string ResultScene = "05_Result";

        public static void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("SceneLoader.LoadScene called with empty scene name.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }

        public static void LoadMainMenu() => LoadScene(MainMenuScene);
        public static void LoadGameSelect() => LoadScene(GameSelectScene);

        public static void LoadAjouBoontu()
        {
            GameSessionManager.Ensure().StartGame(GameType.AjouBoontu, AjouBoontuScene);
            LoadScene(AjouBoontuScene);
        }

        public static void LoadBalanceWalk()
        {
            GameSessionManager.Ensure().StartGame(GameType.BalanceWalk, BalanceWalkScene);
            LoadScene(BalanceWalkScene);
        }

        public static void LoadSoccer(bool preserveSelection = false)
        {
            GameSessionManager session = GameSessionManager.Ensure();
            if (!preserveSelection)
            {
                session.ClearSoccerMatchSelection();
            }

            session.StartGame(GameType.Soccer, SoccerScene);
            LoadScene(SoccerScene);
        }

        public static void LoadResult() => LoadScene(ResultScene);

        public static void RestartLastGame()
        {
            GameSessionManager session = GameSessionManager.Ensure();
            switch (session.CurrentGameType)
            {
                case GameType.AjouBoontu:
                    LoadAjouBoontu();
                    break;
                case GameType.BalanceWalk:
                    LoadBalanceWalk();
                    break;
                case GameType.Soccer:
                    LoadSoccer(true);
                    break;
                default:
                    LoadGameSelect();
                    break;
            }
        }

        public static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void LoadMainMenuButton() => LoadMainMenu();
        public void LoadGameSelectButton() => LoadGameSelect();
        public void LoadAjouBoontuButton() => LoadAjouBoontu();
        public void LoadBalanceWalkButton() => LoadBalanceWalk();
        public void LoadSoccerButton() => LoadSoccer();
        public void LoadResultButton() => LoadResult();
        public void RestartLastGameButton() => RestartLastGame();
        public void QuitGameButton() => QuitGame();
    }
}
