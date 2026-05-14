namespace AjouBuntu.Core
{
    public static class GameSessionResult
    {
        public static bool HasResult { get; private set; }
        public static bool Cleared { get; private set; }
        public static int Score { get; private set; }
        public static int HighScore { get; private set; }
        public static int APlusCount { get; private set; }

        public static void Set(bool cleared, int score, int highScore, int aPlusCount)
        {
            HasResult = true;
            Cleared = cleared;
            Score = score;
            HighScore = highScore;
            APlusCount = aPlusCount;
        }

        public static void Clear()
        {
            HasResult = false;
            Cleared = false;
            Score = 0;
            HighScore = 0;
            APlusCount = 0;
        }
    }
}
