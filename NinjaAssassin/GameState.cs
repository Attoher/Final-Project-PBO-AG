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
                "Easy" => (5, 3000f, 75, 1f),       // Unchanged
                "Normal" => (8, 2000f, 100, 2.5f),  // Slightly reduced stats
                "Hard" => (12, 1500f, 150, 4f),     // Made more balanced
                _ => (5, 3000f, 75, 3f)
            };
        }
    }
}
