namespace FormNavigation
{
    public static class GameState
    {
        public static string SelectedCharacter { get; set; } = "Ninja"; // Default character
        public static string Difficulty { get; set; } = "Normal"; // Easy, Normal, Hard
        public static int Score { get; set; } = 0;
        
        public static (int maxEnemies, float spawnInterval, int enemyHealth, float enemySpeed) GetDifficultySettings()
        {
            return Difficulty switch
            {
                "Easy" => (5, 3000f, 75, 3f),       // 5 enemies, spawn every 3 sec, 75 HP, normal speed
                "Normal" => (10, 2000f, 150, 4f),    // 10 enemies, spawn every 2 sec, 150 HP, faster
                "Hard" => (15, 1000f, 250, 5f),      // 15 enemies, spawn every 1 sec, 250 HP, very fast
                _ => (5, 3000f, 75, 3f)
            };
        }
    }
}
