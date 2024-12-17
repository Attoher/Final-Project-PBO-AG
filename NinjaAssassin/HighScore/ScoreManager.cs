using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FormNavigation.HighScore
{
    public static class ScoreManager
    {
        private static readonly string SCORE_FILE = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "highscores.txt"
        );
        private static List<(string name, int score)> highScores = new List<(string name, int score)>();
        private static readonly int MAX_SCORES = 10;

        static ScoreManager()
        {
            LoadScores();
        }

        public static void AddScore(string playerName, int score)
        {
            highScores.Add((playerName, score));
            highScores = highScores
                .OrderByDescending(x => x.score)
                .Take(MAX_SCORES)
                .ToList();
            SaveScores();
        }

        public static List<(string name, int score)> GetHighScores()
        {
            return highScores.ToList();
        }

        private static void LoadScores()
        {
            if (!File.Exists(SCORE_FILE))
            {
                return;
            }

            try
            {
                var lines = File.ReadAllLines(SCORE_FILE);
                highScores.Clear();
                
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int score))
                    {
                        highScores.Add((parts[0], score));
                    }
                }

                highScores = highScores
                    .OrderByDescending(x => x.score)
                    .Take(MAX_SCORES)
                    .ToList();
            }
            catch
            {
                highScores.Clear();
            }
        }

        private static void SaveScores()
        {
            try
            {
                File.WriteAllLines(
                    SCORE_FILE,
                    highScores.Select(x => $"{x.name},{x.score}")
                );
            }
            catch
            {
                // Handle save error silently
            }
        }
    }
}
