using System;
using System.Collections.Generic;
using UnityEngine;

namespace AjouFestival.Core
{
    public static class ScoreHistoryManager
    {
        private const string HistoryKey = "AjouFestival.ScoreHistory";

        [Serializable]
        public sealed class ScoreHistoryRecord
        {
            public string id;
            public GameType gameType;
            public string playerName;
            public int score;
            public string scoreText;
            public string resultMessage;
            public string recordedAt;
        }

        [Serializable]
        private sealed class ScoreHistoryData
        {
            public List<ScoreHistoryRecord> records = new();
        }

        public static ScoreHistoryRecord AddRecord(GameType type, string playerName, int score, string scoreText, string resultMessage)
        {
            ScoreHistoryData data = LoadData();
            var record = new ScoreHistoryRecord
            {
                id = Guid.NewGuid().ToString("N"),
                gameType = type,
                playerName = NormalizeName(playerName),
                score = score,
                scoreText = string.IsNullOrWhiteSpace(scoreText) ? score.ToString() : scoreText,
                resultMessage = resultMessage ?? string.Empty,
                recordedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            data.records.Add(record);
            SaveData(data);
            return record;
        }

        public static List<ScoreHistoryRecord> GetRecords(GameType type = GameType.None)
        {
            List<ScoreHistoryRecord> records = LoadData().records ?? new List<ScoreHistoryRecord>();
            List<ScoreHistoryRecord> filtered = type == GameType.None
                ? new List<ScoreHistoryRecord>(records)
                : records.FindAll(record => record != null && record.gameType == type);

            filtered.Sort((a, b) =>
            {
                int scoreCompare = b.score.CompareTo(a.score);
                return scoreCompare != 0
                    ? scoreCompare
                    : string.CompareOrdinal(b.recordedAt, a.recordedAt);
            });

            return filtered;
        }

        public static void ClearRecords(GameType type = GameType.None)
        {
            if (type == GameType.None)
            {
                PlayerPrefs.DeleteKey(HistoryKey);
                PlayerPrefs.Save();
                return;
            }

            ScoreHistoryData data = LoadData();
            data.records.RemoveAll(record => record != null && record.gameType == type);
            SaveData(data);
        }

        private static string NormalizeName(string playerName)
        {
            string trimmed = string.IsNullOrWhiteSpace(playerName) ? string.Empty : playerName.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? "Player" : trimmed;
        }

        private static ScoreHistoryData LoadData()
        {
            string json = PlayerPrefs.GetString(HistoryKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new ScoreHistoryData();
            }

            try
            {
                ScoreHistoryData data = JsonUtility.FromJson<ScoreHistoryData>(json);
                if (data == null)
                {
                    return new ScoreHistoryData();
                }

                data.records ??= new List<ScoreHistoryRecord>();
                data.records.RemoveAll(record => record == null);
                return data;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to load score history: {exception.Message}");
                return new ScoreHistoryData();
            }
        }

        private static void SaveData(ScoreHistoryData data)
        {
            data.records ??= new List<ScoreHistoryRecord>();
            PlayerPrefs.SetString(HistoryKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
    }
}
