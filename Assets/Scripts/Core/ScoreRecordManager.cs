using UnityEngine;

namespace AjouFestival.Core
{
    public static class ScoreRecordManager
    {
        private const string Prefix = "AjouFestival.BestScore.";

        public static int GetBestScore(GameType type)
        {
            if (type == GameType.None)
            {
                return 0;
            }

            return PlayerPrefs.GetInt(Prefix + type, 0);
        }

        public static void SetBestScore(GameType type, int score)
        {
            if (type == GameType.None)
            {
                return;
            }

            if (score > GetBestScore(type))
            {
                PlayerPrefs.SetInt(Prefix + type, score);
                PlayerPrefs.Save();
            }
        }

        public static void ResetBestScore(GameType type)
        {
            if (type == GameType.None)
            {
                return;
            }

            PlayerPrefs.DeleteKey(Prefix + type);
            PlayerPrefs.Save();
        }

        public static void ResetAllScores()
        {
            ResetBestScore(GameType.AjouBoontu);
            ResetBestScore(GameType.BalanceWalk);
            ResetBestScore(GameType.Soccer);
        }
    }
}
