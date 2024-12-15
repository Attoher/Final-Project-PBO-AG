using System;
using System.Drawing;
using System.Windows.Forms;

namespace FormNavigation
{
    public class GameForm : Form
    {
        private Button backButton;
        private Timer gameTimer;
        private Point playerPosition;
        private const int PLAYER_SPEED = 5;
        private bool isWPressed, isSPressed, isAPressed, isDPressed;
        private SpriteAnimation playerAnimation;
        private const string CHARACTERS_PATH = "Resources/Characters";

        public GameForm()
        {
            InitializeForm();
            InitializeControls();
            InitializeGame();
        }

        private void InitializeForm()
        {
            this.Text = "Game Level";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true; // Prevent flickering
            
            // Set up keyboard handling
            this.KeyPreview = true;
            this.KeyDown += GameForm_KeyDown;
            this.KeyUp += GameForm_KeyUp;
        }

        private void InitializeControls()
        {
            // Tombol Back
            backButton = new Button
            {
                Text = "Back",
                Location = new Point(10, 10),
                Size = new Size(80, 30)
            };
            backButton.Click += BackButton_Click;
            this.Controls.Add(backButton);
        }

        private void InitializeGame()
        {
            playerPosition = new Point(
                this.ClientSize.Width / 2,
                this.ClientSize.Height / 2
            );

            gameTimer = new Timer();
            gameTimer.Interval = 16;
            gameTimer.Tick += GameTimer_Tick;

            try
            {
                string characterFile = GameState.SelectedCharacter + "_Idle.png";
                Image playerSheet = Image.FromFile(System.IO.Path.Combine(CHARACTERS_PATH, characterFile));
                
                // Set animation parameters based on character
                int frameWidth = 24, frameHeight = 24, frames = 9;
                switch (GameState.SelectedCharacter)
                {
                    case "Ninja":
                        frameWidth = 25; frameHeight = 25; frames = 9;
                        break;
                    case "Puppeteer":
                        frameWidth = 25; frameHeight = 50; frames = 4;
                        break;
                    case "Samurai":
                        frameWidth = 50; frameHeight = 50; frames = 4;
                        break;
                    case "Scarecrow":
                        frameWidth = 25; frameHeight = 25; frames = 4;
                        break;
                    case "Shaman":
                        frameWidth = 25; frameHeight = 50; frames = 4;
                        break;
                }

                playerAnimation = new SpriteAnimation(playerSheet, frameWidth, frameHeight, frames, 50);
                playerAnimation.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading player animation: {ex.Message}");
            }

            gameTimer.Start();
            this.Paint += GameForm_Paint;
        }

        private (int width, int height) GetCharacterDimensions()
        {
            return GameState.SelectedCharacter switch
            {
                "Ninja" => (25, 25),
                "Puppeteer" => (25, 50),
                "Samurai" => (50, 50),
                "Scarecrow" => (25, 25),
                "Shaman" => (25, 50),
                _ => (25, 25)
            };
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // Update player position based on key states
            if (isWPressed) playerPosition.Y -= PLAYER_SPEED;
            if (isSPressed) playerPosition.Y += PLAYER_SPEED;
            if (isAPressed) playerPosition.X -= PLAYER_SPEED;
            if (isDPressed) playerPosition.X += PLAYER_SPEED;

            // Get current character dimensions
            var (width, height) = GetCharacterDimensions();

            // Keep player within bounds using actual character dimensions
            playerPosition.X = Math.Max(0, Math.Min(playerPosition.X, this.ClientSize.Width - width));
            playerPosition.Y = Math.Max(0, Math.Min(playerPosition.Y, this.ClientSize.Height - height));

            this.Invalidate();
        }

        private void GameForm_Paint(object sender, PaintEventArgs e)
        {
            if (playerAnimation != null)
            {
                var (width, height) = GetCharacterDimensions();
                
                // Scale factor to make characters visible but maintain proportions
                int scale = GameState.SelectedCharacter switch
                {
                    "Samurai" => 2,     // 50x50 -> 100x100
                    "Puppeteer" => 2,   // 25x50 -> 50x100
                    "Shaman" => 2,      // 25x50 -> 50x100
                    _ => 3              // 25x25 -> 75x75 for others
                };

                playerAnimation.DrawFrame(e.Graphics, new Rectangle(
                    playerPosition.X, 
                    playerPosition.Y, 
                    width * scale, 
                    height * scale));
            }
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W: isWPressed = true; break;
                case Keys.S: isSPressed = true; break;
                case Keys.A: isAPressed = true; break;
                case Keys.D: isDPressed = true; break;
            }
        }

        private void GameForm_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W: isWPressed = false; break;
                case Keys.S: isSPressed = false; break;
                case Keys.A: isAPressed = false; break;
                case Keys.D: isDPressed = false; break;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            gameTimer?.Stop();
            playerAnimation?.Dispose();
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            this.Close(); // Menutup GameForm dan kembali ke MainForm
        }
    }
}
