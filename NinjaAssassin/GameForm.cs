using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

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
        private Point cameraOffset;
        private const int WORLD_WIDTH = 2000;  // Total world width
        private const int WORLD_HEIGHT = 2000; // Total world height
        private const string TILES_PATH = "Resources/Tiles";
        private Image[] grassTiles;
        private int[,] tileMap;
        private const int TILE_SIZE = 32;
        private Random random = new Random();
        private bool isFacingLeft = false;
        private bool isMoving = false;
        private Point lastMousePosition;
        private Point mouseWorldPos; // Add this field
        private const string WEAPONS_PATH = "Resources/Weapons";
        private Image gunSprite;
        private const int GUN_OFFSET_X = 30; // Adjust these values to position the gun
        private const int GUN_OFFSET_Y = 20;
        private Point currentMousePosition;  // Add this field
        private List<Bullet> bullets = new List<Bullet>();
        private const float BULLET_SPEED = 15f;
        private const int BULLET_SIZE = 5;
        private const int STANDARD_WIDTH = 75; // Standard width for all characters

        private class Bullet
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float VelocityX { get; set; }
            public float VelocityY { get; set; }
            public bool IsActive { get; set; } = true;
        }

        public GameForm()
        {
            InitializeForm();
            InitializeControls();
            InitializeGame();
            this.MouseMove += GameForm_MouseMove;
            currentMousePosition = Cursor.Position;  // Initialize mouse position
            this.MouseDown += GameForm_MouseDown;
            this.MouseUp += GameForm_MouseUp;
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
            lastMousePosition = Point.Empty;
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
                WORLD_WIDTH / 2,  // Start in center of world
                WORLD_HEIGHT / 2
            );
            UpdateCameraPosition();

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

            // Load grass tiles
            LoadGrassTiles();
            GenerateTileMap();

            // Load weapon sprite
            try
            {
                gunSprite = Image.FromFile(System.IO.Path.Combine(WEAPONS_PATH, "Gun1.png"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading weapon sprite: {ex.Message}");
            }

            gameTimer.Start();
            this.Paint += GameForm_Paint;
        }

        private void LoadGrassTiles()
        {
            try
            {
                // Load all grass variations (assuming you have Grass1.png, Grass2.png, etc.)
                grassTiles = new Image[]
                {
                    Image.FromFile(System.IO.Path.Combine(TILES_PATH, "Grass1.png")),
                    Image.FromFile(System.IO.Path.Combine(TILES_PATH, "Grass2.png")),
                    Image.FromFile(System.IO.Path.Combine(TILES_PATH, "Grass3.png"))
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading grass tiles: {ex.Message}");
            }
        }

        private void GenerateTileMap()
        {
            int tilesX = WORLD_WIDTH / TILE_SIZE;
            int tilesY = WORLD_HEIGHT / TILE_SIZE;
            tileMap = new int[tilesX, tilesY];

            // Generate random tile indices
            for (int x = 0; x < tilesX; x++)
            {
                for (int y = 0; y < tilesY; y++)
                {
                    tileMap[x, y] = random.Next(grassTiles.Length);
                }
            }
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

        private (int width, int height) GetScaledDimensions()
        {
            var (originalWidth, originalHeight) = GetCharacterDimensions();
            float scaleRatio = (float)STANDARD_WIDTH / originalWidth;
            
            return (
                STANDARD_WIDTH,
                (int)(originalHeight * scaleRatio)
            );
        }

        private void UpdateCameraPosition()
        {
            // Center camera directly on player position since it's now the center point
            cameraOffset.X = playerPosition.X - (ClientSize.Width / 2);
            cameraOffset.Y = playerPosition.Y - (ClientSize.Height / 2);

            // Clamp camera to world bounds
            cameraOffset.X = Math.Max(0, Math.Min(cameraOffset.X, WORLD_WIDTH - ClientSize.Width));
            cameraOffset.Y = Math.Max(0, Math.Min(cameraOffset.Y, WORLD_HEIGHT - ClientSize.Height));
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // Track if player is moving
            isMoving = isWPressed || isSPressed || isAPressed || isDPressed;

            // Update facing direction based on stored mouse position
            isFacingLeft = mouseWorldPos.X < (playerPosition.X + GetCharacterDimensions().width / 2);

            // Update player position based on key states
            if (isWPressed) playerPosition.Y -= PLAYER_SPEED;
            if (isSPressed) playerPosition.Y += PLAYER_SPEED;
            if (isAPressed) playerPosition.X -= PLAYER_SPEED;
            if (isDPressed) playerPosition.X += PLAYER_SPEED;

            // Get current character dimensions and bounds checking
            var (width, height) = GetCharacterDimensions();
            playerPosition.X = Math.Max(0, Math.Min(playerPosition.X, WORLD_WIDTH - width));
            playerPosition.Y = Math.Max(0, Math.Min(playerPosition.Y, WORLD_HEIGHT - height));

            // Update bullets
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                var bullet = bullets[i];
                bullet.X += bullet.VelocityX;
                bullet.Y += bullet.VelocityY;

                // Remove bullets that are out of bounds
                if (bullet.X < 0 || bullet.X > WORLD_WIDTH || 
                    bullet.Y < 0 || bullet.Y > WORLD_HEIGHT)
                {
                    bullets.RemoveAt(i);
                }
            }

            UpdateCameraPosition();
            this.Invalidate(); // Always redraw when moving
        }

        private void GameForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Black);

            // Enable smooth graphics
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

            // Create a translated graphics context for camera offset
            using (Matrix transform = new Matrix())
            {
                transform.Translate(-cameraOffset.X, -cameraOffset.Y);
                e.Graphics.Transform = transform;

                // Draw grass tiles
                if (grassTiles != null)
                {
                    // Calculate visible region
                    int startX = Math.Max(0, cameraOffset.X / TILE_SIZE);
                    int startY = Math.Max(0, cameraOffset.Y / TILE_SIZE);
                    int endX = Math.Min(tileMap.GetLength(0), (cameraOffset.X + ClientSize.Width) / TILE_SIZE + 1);
                    int endY = Math.Min(tileMap.GetLength(1), (cameraOffset.Y + ClientSize.Height) / TILE_SIZE + 1);

                    // Draw only visible tiles
                    for (int x = startX; x < endX; x++)
                    {
                        for (int y = startY; y < endY; y++)
                        {
                            int tileIndex = tileMap[x, y];
                            e.Graphics.DrawImage(grassTiles[tileIndex],
                                x * TILE_SIZE,
                                y * TILE_SIZE,
                                TILE_SIZE,
                                TILE_SIZE);
                        }
                    }
                }

                // Draw world bounds
                using (Pen pen = new Pen(Color.DarkGray))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, WORLD_WIDTH, WORLD_HEIGHT);
                }

                // Draw player and weapon
                if (playerAnimation != null)
                {
                    var (width, height) = GetCharacterDimensions();
                    var (scaledWidth, scaledHeight) = GetScaledDimensions();

                    // Calculate drawing position adjusted for center
                    int drawX = playerPosition.X - scaledWidth / 2;
                    int drawY = playerPosition.Y - scaledHeight / 2;
                    
                    // Calculate player center (now exactly at playerPosition)
                    int playerCenterX = playerPosition.X;
                    int playerCenterY = playerPosition.Y;

                    var state = e.Graphics.Save();
                    try
                    {
                        if (isFacingLeft)
                        {
                            e.Graphics.TranslateTransform(playerPosition.X, playerPosition.Y);
                            e.Graphics.ScaleTransform(-1, 1);
                            playerAnimation.DrawFrame(e.Graphics, 
                                new Rectangle(-scaledWidth/2, -scaledHeight/2, scaledWidth, scaledHeight));
                        }
                        else
                        {
                            playerAnimation.DrawFrame(e.Graphics, 
                                new Rectangle(drawX, drawY, scaledWidth, scaledHeight));
                        }

                        e.Graphics.Restore(state);
                        state = e.Graphics.Save();

                        // Draw weapon separately
                        if (gunSprite != null)
                        {
                            float gunAngle = (float)Math.Atan2(
                                (currentMousePosition.Y + cameraOffset.Y) - playerCenterY,
                                (currentMousePosition.X + cameraOffset.X) - playerCenterX
                            );

                            e.Graphics.TranslateTransform(playerCenterX, playerCenterY);
                            e.Graphics.RotateTransform((float)(gunAngle * 180 / Math.PI));

                            int gunWidth = 70;
                            int gunHeight = 36;

                            if (isFacingLeft)
                            {
                                // Hanya flip horizontal untuk senjata saat menghadap kiri
                                e.Graphics.ScaleTransform(1, -1);
                                e.Graphics.DrawImage(gunSprite,
                                    25,
                                    -20, // Mengubah dari positif ke negatif untuk menggerakkan ke atas
                                    gunWidth,
                                    gunHeight);
                            }
                            else
                            {
                                e.Graphics.DrawImage(gunSprite,
                                    25,
                                    -20, // Mengubah offset Y menjadi lebih besar (negatif karena tidak di-flip)
                                    gunWidth,
                                    gunHeight);
                            }
                        }
                    }
                    finally
                    {
                        e.Graphics.Restore(state);
                    }
                }

                // Draw bullets after everything else
                using (SolidBrush bulletBrush = new SolidBrush(Color.Yellow))
                {
                    foreach (var bullet in bullets)
                    {
                        e.Graphics.FillEllipse(bulletBrush, 
                            bullet.X - BULLET_SIZE/2, 
                            bullet.Y - BULLET_SIZE/2,
                            BULLET_SIZE, 
                            BULLET_SIZE);
                    }
                }
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

        private void GameForm_MouseMove(object sender, MouseEventArgs e)
        {
            // Store current mouse position in screen coordinates
            currentMousePosition = e.Location;
            
            // Update world position
            mouseWorldPos = new Point(
                currentMousePosition.X + cameraOffset.X,
                currentMousePosition.Y + cameraOffset.Y
            );
        }

        private void GameForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int playerCenterX = playerPosition.X;
                int playerCenterY = playerPosition.Y;
                
                float angle = (float)Math.Atan2(
                    (currentMousePosition.Y + cameraOffset.Y) - playerCenterY,
                    (currentMousePosition.X + cameraOffset.X) - playerCenterX
                );

                // Calculate bullet velocity
                float velocityX = (float)Math.Cos(angle) * BULLET_SPEED;
                float velocityY = (float)Math.Sin(angle) * BULLET_SPEED;

                // Calculate bullet spawn position
                int gunOffsetX = 95; // Distance from center to gun tip
                float bulletStartX, bulletStartY;

                if (isFacingLeft)
                {
                    // When facing left, reverse the angle but keep same spawn distance
                    bulletStartX = playerCenterX + (gunOffsetX * (float)Math.Cos(angle));
                    bulletStartY = playerCenterY + (gunOffsetX * (float)Math.Sin(angle));
                }
                else
                {
                    bulletStartX = playerCenterX + (gunOffsetX * (float)Math.Cos(angle));
                    bulletStartY = playerCenterY + (gunOffsetX * (float)Math.Sin(angle));
                }

                bullets.Add(new Bullet
                {
                    X = bulletStartX,
                    Y = bulletStartY,
                    VelocityX = velocityX,
                    VelocityY = velocityY
                });
            }
        }

        private void GameForm_MouseUp(object sender, MouseEventArgs e)
        {
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            gameTimer?.Stop();
            playerAnimation?.Dispose();
            gunSprite?.Dispose();
            if (grassTiles != null)
            {
                foreach (var tile in grassTiles)
                {
                    tile?.Dispose();
                }
            }
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            this.Close(); // Menutup GameForm dan kembali ke MainForm
        }
    }
}
