using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.IO; // Tambahkan ini untuk Path, File, dan Directory

namespace FormNavigation
{
    public class GameForm : Form
    {
        private Button backButton;
        private Timer gameTimer;
        private Point playerPosition;
        private bool isWPressed, isSPressed, isAPressed, isDPressed;
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
        private const float BULLET_SPEED = 20f; // Slightly reduced speed for larger bullets
        private const int BULLET_SIZE = 15; // Changed from 5 to 15
        private const int STANDARD_WIDTH = 75; // Standard width for all characters
        private const int SAMURAI_WIDTH = 150; // Special width for Samurai (2x normal)
        private const string BUSH_PATH = "Resources/Obstacles";
        private Image bushSprite;
        private List<Rectangle> bushColliders = new List<Rectangle>();
        private const int BUSH_SIZE = 64;
        private const int NUM_BUSHES = 35; // Jumlah bush yang akan di-spawn
        private SpriteAnimation playerIdleAnimation;
        private SpriteAnimation playerRunAnimation;
        private const string CHARACTERS_RUN_PATH = "Resources/Characters/Run";
        private const float BULLET_COOLDOWN = 500f;
        private DateTime lastBulletTime = DateTime.MinValue;
        private Bitmap tileMapBuffer; // Add this field
        private const string CHARACTERS_SKILL_PATH = "Resources/Characters/Skills";
        private SpriteAnimation playerSkillAnimation;
        private bool isSkillActive = false;
        private DateTime skillStartTime;
        private const float SKILL_DURATION = 5f;
        private Point lastMouseWorldPos;
        private bool hasMouseMoved = false;
        private bool skillCompleted = true;  // Add this field
        private const float SKILL_COOLDOWN = 2000f; // 2 seconds cooldown
        private DateTime lastSkillTime = DateTime.MinValue;
        private int skillRepetitionsRequired;
        private int currentSkillRepetition;
        private int skillFrameCount;
        private const string ENEMY_PATH = "Resources/Enemy";
        private List<Enemy> enemies = new List<Enemy>();
        private int playerMaxHealth;
        private int playerCurrentHealth;
        private int playerAttack;
        private float playerSpeed;  // Keep only this one
        private const int BULLET_DAMAGE = 25;
        private DateTime lastEnemySpawnTime = DateTime.MinValue;
        private Label scoreLabel;
        private int currentScore = 0;
        private (int maxEnemies, float spawnInterval, int enemyHealth, float enemySpeed) difficultySettings;
        private readonly Dictionary<string, (int width, int height, int idleFrames, int walkFrames, int attackFrames)> enemyConfigs = 
            new Dictionary<string, (int width, int height, int idleFrames, int walkFrames, int attackFrames)>
        {
            {"Slime", (64, 64, 6, 6, 15)},
            {"Bomb Puppet", (50, 50, 8, 8, 20)}
        };

        private int skillInterval;
        private float skillDuration;

        private const float INVULNERABILITY_DURATION = 1000f; // 1 second of invulnerability after hit
        private DateTime lastHitTime = DateTime.MinValue;
        private bool isInvulnerable = false;

        // Add these constants for HP bar styling
        private const int HP_BAR_WIDTH = 200;
        private const int HP_BAR_HEIGHT = 20;
        private const int HP_BAR_MARGIN = 10;

        private class Bullet
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float VelocityX { get; set; }
            public float VelocityY { get; set; }
            public bool IsActive { get; set; } = true;
            public int Damage { get; set; }
        }

        public GameForm()
        {
            difficultySettings = GameState.GetDifficultySettings();
            InitializePlayerStats();
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

            // Add score label
            scoreLabel = new Label
            {
                Text = "Score: 0",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(128, 0, 0, 0),
                AutoSize = true,
                Location = new Point(100, 10)
            };
            this.Controls.Add(scoreLabel);
            scoreLabel.BringToFront();
        }

        private void InitializeGame()
        {
            try
            {
                // Initialize game timer first
                gameTimer = new Timer
                {
                    Interval = 16 // ~60 FPS
                };
                gameTimer.Tick += GameTimer_Tick;

                // Set initial position
                playerPosition = new Point(WORLD_WIDTH / 2, WORLD_HEIGHT / 2);
                UpdateCameraPosition();

                // Create required directories if they don't exist
                string runPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CHARACTERS_RUN_PATH);
                if (!Directory.Exists(runPath))
                {
                    Directory.CreateDirectory(runPath);
                }

                // Load resources
                LoadCharacterAnimations();
                LoadGrassTiles();
                
                if (grassTiles != null && grassTiles.Length > 0)
                {
                    GenerateTileMap();
                    PreRenderTileMap(); // Add this line
                }

                // Load weapon and obstacles
                try
                {
                    string weaponPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, WEAPONS_PATH, "Gun1.png");
                    string bushPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BUSH_PATH, "Bush.png");

                    if (File.Exists(weaponPath))
                    {
                        gunSprite = Image.FromFile(weaponPath);
                    }

                    if (File.Exists(bushPath))
                    {
                        bushSprite = Image.FromFile(bushPath);
                        GenerateRandomBushes();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading sprites: {ex.Message}");
                }

                // Add enemy initialization
                LoadEnemies();

                // Start game loop
                gameTimer.Start();
                this.Paint += GameForm_Paint;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing game:\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}");
                this.Close();
            }
        }

        private void LoadCharacterAnimations()
        {
            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string idleFile = Path.Combine(basePath, CHARACTERS_PATH, $"{GameState.SelectedCharacter}_Idle.png");
                string runFile = Path.Combine(basePath, CHARACTERS_RUN_PATH, $"{GameState.SelectedCharacter}_Run.png");
                string skillFile = Path.Combine(basePath, CHARACTERS_SKILL_PATH, $"{GameState.SelectedCharacter}_Skill.png");

                if (!File.Exists(idleFile))
                {
                    throw new FileNotFoundException($"Idle animation not found: {idleFile}");
                }
                if (!File.Exists(runFile))
                {
                    throw new FileNotFoundException($"Run animation not found: {runFile}");
                }
                if (!File.Exists(skillFile))
                {
                    throw new FileNotFoundException($"Skill animation not found: {skillFile}");
                }

                // Update run frames untuk masing-masing karakter
                var (frameWidth, frameHeight, idleFrames, runFrames, skillFrames) = GameState.SelectedCharacter switch
                {
                    "Ninja" => (25, 25, 9, 6, 11),      // Run: 6 frames
                    "Puppeteer" => (25, 50, 4, 4, 8),  // Run: 4 frames
                    "Samurai" => (50, 50, 4, 12, 8),   // Run: 12 frames
                    "Scarecrow" => (25, 25, 4, 4, 9),  // Run: 4 frames
                    "Shaman" => (25, 50, 4, 4, 4),     // Run: 4 frames
                    _ => throw new Exception($"Invalid character: {GameState.SelectedCharacter}")
                };

                (skillFrameCount, skillRepetitionsRequired, skillInterval) = GameState.SelectedCharacter switch
                {
                    "Ninja" => (11, 1, 50),      // 11 frames, 1x, fast animation (50ms per frame)
                    "Puppeteer" => (8, 2, 80),   // 8 frames, 2x, medium animation (80ms per frame)
                    "Samurai" => (8, 1, 100),    // 8 frames, 1x, slower animation (100ms per frame)
                    "Scarecrow" => (9, 1, 70),   // 9 frames, 1x, medium-fast animation (70ms per frame)
                    "Shaman" => (4, 3, 150),     // 4 frames, 3x, slow animation (150ms per frame)
                    _ => throw new Exception($"Invalid character: {GameState.SelectedCharacter}")
                };

                // Hitung total durasi skill berdasarkan frames dan interval
                skillDuration = skillFrameCount * skillInterval;

                using (var idleSheet = Image.FromFile(idleFile))
                using (var runSheet = Image.FromFile(runFile))
                using (var skillSheet = Image.FromFile(skillFile))
                {
                    playerIdleAnimation = new SpriteAnimation((Image)idleSheet.Clone(), frameWidth, frameHeight, idleFrames, 50);
                    playerRunAnimation = new SpriteAnimation((Image)runSheet.Clone(), frameWidth, frameHeight, runFrames, 50);
                    playerSkillAnimation = new SpriteAnimation((Image)skillSheet.Clone(), frameWidth, frameHeight, skillFrames, skillInterval);
                }

                playerIdleAnimation.Start();
                playerRunAnimation.Start();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading character animations: {ex.Message}", ex);
            }
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
            float scaleRatio;
            
            // Use larger scale for Samurai
            if (GameState.SelectedCharacter == "Samurai")
            {
                scaleRatio = (float)SAMURAI_WIDTH / originalWidth;
                return (SAMURAI_WIDTH, (int)(originalHeight * scaleRatio));
            }
            
            // Normal scale for other characters
            scaleRatio = (float)STANDARD_WIDTH / originalWidth;
            return (STANDARD_WIDTH, (int)(originalHeight * scaleRatio));
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

            // Update facing direction only if mouse has moved
            if (hasMouseMoved)
            {
                isFacingLeft = lastMouseWorldPos.X < (playerPosition.X + GetCharacterDimensions().width / 2);
                hasMouseMoved = false;  // Reset flag until next mouse movement
            }

            // Calculate movement vector
            float moveX = 0;
            float moveY = 0;

            if (isAPressed) moveX -= 1;
            if (isDPressed) moveX += 1;
            if (isWPressed) moveY -= 1;
            if (isSPressed) moveY += 1;

            // Normalize diagonal movement
            if (moveX != 0 && moveY != 0)
            {
                float length = (float)Math.Sqrt(moveX * moveX + moveY * moveY);
                moveX /= length;
                moveY /= length;
            }

            // Apply movement speed
            Point newPosition = new Point(
                playerPosition.X + (int)(moveX * playerSpeed),
                playerPosition.Y + (int)(moveY * playerSpeed)
            );

            // Hanya update posisi jika tidak bertabrakan dengan bush
            if (!CheckBushCollision(newPosition))
            {
                playerPosition = newPosition;
            }

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

            // Check bullet collisions with enemies
            HandleBulletCollisions();

            // Update skill state dengan timing yang tepat
            if (isSkillActive)
            {
                TimeSpan skillElapsed = DateTime.Now - skillStartTime;
                
                // Check if current repetition is complete
                if (skillElapsed.TotalMilliseconds >= skillDuration)
                {
                    currentSkillRepetition++;
                    if (currentSkillRepetition >= skillRepetitionsRequired)
                    {
                        isSkillActive = false;
                        skillCompleted = true;
                        playerSkillAnimation?.Stop();
                    }
                    else
                    {
                        // Reset for next repetition
                        skillStartTime = DateTime.Now;
                        playerSkillAnimation?.Start();
                    }
                }
            }

            // Update enemies
            foreach (var enemy in enemies)
            {
                if (enemy.IsActive)
                {
                    enemy.MoveTowardsPlayer(playerPosition);
                    enemy.Update();
                }
            }

            // Check if we should spawn a new enemy
            TimeSpan timeSinceLastSpawn = DateTime.Now - lastEnemySpawnTime;
            if (timeSinceLastSpawn.TotalMilliseconds >= difficultySettings.spawnInterval && 
                enemies.Count < difficultySettings.maxEnemies)
            {
                SpawnNewEnemy();
                lastEnemySpawnTime = DateTime.Now;
            }

            // Handle enemy attacks
            isInvulnerable = (DateTime.Now - lastHitTime).TotalMilliseconds < INVULNERABILITY_DURATION;

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];
                if (enemy.IsActive && enemy.ShouldDealDamage(playerPosition))
                {
                    if (!isInvulnerable)
                    {
                        playerCurrentHealth -= enemy.Attack;
                        lastHitTime = DateTime.Now;
                        isInvulnerable = true;
                        
                        // Check if player died
                        if (playerCurrentHealth <= 0)
                        {
                            GameOver();
                            return;
                        }
                    }

                    // Remove Bomb Puppet after explosion
                    if (enemy.Type == "Bomb Puppet" && enemy.HasExploded)
                    {
                        enemies.RemoveAt(i);
                    }
                }
            }

            UpdateCameraPosition();
            this.Invalidate(); // Always redraw when moving
        }

        private void GameForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Black);

            // Enable pixel-perfect rendering
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.AssumeLinear;
            e.Graphics.SmoothingMode = SmoothingMode.None;

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

                // Tambahkan setelah menggambar grass tiles dan sebelum player
                if (bushSprite != null)
                {
                    for (int i = 0; i < bushColliders.Count; i++)
                    {
                        Rectangle bush = bushColliders[i];
                        // Gambar bush dengan posisi yang disesuaikan
                        e.Graphics.DrawImage(bushSprite,
                            bush.X - BUSH_SIZE/4,        // Kembalikan offset X
                            bush.Y - (BUSH_SIZE * 1/3),  // Kembalikan ke posisi atas bush
                            BUSH_SIZE,                   // Ukuran sprite bush
                            BUSH_SIZE);

                        // Debug: Draw collider boxes
                        using (Pen debugPen = new Pen(Color.Red, 2))
                        {
                            // Draw bush collider
                            e.Graphics.DrawRectangle(debugPen, bush);

                            // Draw player collider
                            Rectangle playerRect = new Rectangle(
                                playerPosition.X - STANDARD_WIDTH/2,
                                playerPosition.Y - STANDARD_WIDTH/2,
                                STANDARD_WIDTH,
                                STANDARD_WIDTH
                            );
                            e.Graphics.DrawRectangle(debugPen, playerRect);
                        }
                    }
                }

                // Draw player and weapon
                if (playerIdleAnimation != null && playerRunAnimation != null)
                {
                    var (width, height) = GetCharacterDimensions();
                    var (scaledWidth, scaledHeight) = GetScaledDimensions();

                    // Adjust drawing position for characters
                    int drawY;
                    if (GameState.SelectedCharacter == "Puppeteer")
                    {
                        drawY = playerPosition.Y - scaledHeight / 3;
                    }
                    else if (GameState.SelectedCharacter == "Shaman")
                    {
                        drawY = playerPosition.Y - (int)(scaledHeight * 0.75);
                    }
                    else if (GameState.SelectedCharacter == "Samurai")
                    {
                        // Menggeser Samurai sedikit ke atas
                        drawY = playerPosition.Y - (int)(scaledHeight * 0.6);
                    }
                    else
                    {
                        drawY = playerPosition.Y - scaledHeight / 2;
                    }
                    
                    int drawX = playerPosition.X - scaledWidth / 2;
                    int playerCenterX = playerPosition.X;
                    int playerCenterY = playerPosition.Y;

                    var state = e.Graphics.Save();
                    try
                    {
                        if (isSkillActive && playerSkillAnimation != null)
                        {
                            // Draw skill animation instead of idle/run
                            if (isFacingLeft)
                            {
                                e.Graphics.TranslateTransform(playerPosition.X, playerPosition.Y);
                                e.Graphics.ScaleTransform(-1, 1);
                                playerSkillAnimation.DrawFrame(e.Graphics,
                                    new Rectangle(-scaledWidth/2, 
                                                GameState.SelectedCharacter == "Puppeteer" ? -scaledHeight/3 : 
                                                GameState.SelectedCharacter == "Shaman" ? -(int)(scaledHeight * 0.75) :
                                                GameState.SelectedCharacter == "Samurai" ? -(int)(scaledHeight * 0.6) :
                                                -scaledHeight/2, 
                                                scaledWidth, 
                                                scaledHeight));
                            }
                            else
                            {
                                playerSkillAnimation.DrawFrame(e.Graphics,
                                    new Rectangle(drawX, drawY, scaledWidth, scaledHeight));
                            }
                        }
                        else
                        {
                            if (isFacingLeft)
                            {
                                e.Graphics.TranslateTransform(playerPosition.X, playerPosition.Y);
                                e.Graphics.ScaleTransform(-1, 1);
                                
                                if (isMoving)
                                {
                                    playerRunAnimation.DrawFrame(e.Graphics, 
                                        new Rectangle(-scaledWidth/2, 
                                                    GameState.SelectedCharacter == "Puppeteer" ? -scaledHeight/3 : 
                                                    GameState.SelectedCharacter == "Shaman" ? -(int)(scaledHeight * 0.75) :
                                                    GameState.SelectedCharacter == "Samurai" ? -(int)(scaledHeight * 0.6) :
                                                    -scaledHeight/2, 
                                                    scaledWidth, 
                                                    scaledHeight));
                                }
                                else
                                {
                                    playerIdleAnimation.DrawFrame(e.Graphics, 
                                        new Rectangle(-scaledWidth/2, 
                                                    GameState.SelectedCharacter == "Shaman" ? -(int)(scaledHeight * 0.75) :
                                                    GameState.SelectedCharacter == "Samurai" ? -(int)(scaledHeight * 0.6) :
                                                    -scaledHeight/2, 
                                                    scaledWidth, 
                                                    scaledHeight));
                                }
                            }
                            else
                            {
                                if (isMoving)
                                {
                                    playerRunAnimation.DrawFrame(e.Graphics, 
                                        new Rectangle(drawX, drawY, scaledWidth, scaledHeight));
                                }
                                else
                                {
                                    playerIdleAnimation.DrawFrame(e.Graphics, 
                                        new Rectangle(drawX, 
                                                    GameState.SelectedCharacter == "Shaman" ? 
                                                        playerPosition.Y - (int)(scaledHeight * 0.75) :
                                                    GameState.SelectedCharacter == "Samurai" ?
                                                        playerPosition.Y - (int)(scaledHeight * 0.6) :
                                                    playerPosition.Y - scaledHeight/2, 
                                                    scaledWidth, 
                                                    scaledHeight));
                                }
                            }
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
                            const int GUN_FORWARD_OFFSET = 35; // Decreased from 45 to 35 to move gun back slightly

                            if (isFacingLeft)
                            {
                                e.Graphics.ScaleTransform(1, -1);
                                e.Graphics.DrawImage(gunSprite,
                                    GUN_FORWARD_OFFSET,
                                    -20,
                                    gunWidth,
                                    gunHeight);
                            }
                            else
                            {
                                e.Graphics.DrawImage(gunSprite,
                                    GUN_FORWARD_OFFSET,
                                    -20,
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

                // Replace enemy drawing code
                foreach (var enemy in enemies)
                {
                    if (enemy.IsActive)
                    {
                        enemy.Draw(e.Graphics, enemy.Bounds);

                        // Draw HP bar above enemy
                        int healthBarWidth = 100;
                        int healthBarHeight = 8;
                        float healthPercent = (float)enemy.CurrentHealth / enemy.MaxHealth;
                        
                        Rectangle backgroundBar = new Rectangle(
                            enemy.Position.X - healthBarWidth/2,
                            enemy.Bounds.Top - 20,  // Position bar above enemy sprite
                            healthBarWidth,
                            healthBarHeight
                        );
                        
                        Rectangle healthBar = new Rectangle(
                            backgroundBar.X,
                            backgroundBar.Y,
                            (int)(healthBarWidth * healthPercent),
                            healthBarHeight
                        );

                        using (SolidBrush redBrush = new SolidBrush(Color.Red))
                        using (SolidBrush greenBrush = new SolidBrush(Color.Green))
                        {
                            e.Graphics.FillRectangle(redBrush, backgroundBar);
                            e.Graphics.FillRectangle(greenBrush, healthBar);
                        }
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

            // Reset transform for UI elements (this ensures they stay fixed on screen)
            e.Graphics.ResetTransform();

            // Draw player HP bar in top right corner with screen-space coordinates
            int hpBarX = this.ClientSize.Width - HP_BAR_WIDTH - HP_BAR_MARGIN;
            int hpBarY = HP_BAR_MARGIN;
            float playerHealthPercent = (float)playerCurrentHealth / playerMaxHealth;
            
            // Draw HP text
            string hpText = $"HP: {playerCurrentHealth}/{playerMaxHealth}";
            using (Font font = new Font("Arial", 12, FontStyle.Bold))
            {
                // Add shadow effect for better visibility
                e.Graphics.DrawString(hpText, font, Brushes.Black, 
                    new Point(hpBarX - 4, hpBarY + HP_BAR_HEIGHT + 6));
                e.Graphics.DrawString(hpText, font, Brushes.White, 
                    new Point(hpBarX - 5, hpBarY + HP_BAR_HEIGHT + 5));
            }

            // Background (empty health bar)
            Rectangle playerBackground = new Rectangle(hpBarX, hpBarY, HP_BAR_WIDTH, HP_BAR_HEIGHT);
            
            // Foreground (filled health bar)
            Rectangle playerHealth = new Rectangle(
                hpBarX, 
                hpBarY, 
                (int)(HP_BAR_WIDTH * playerHealthPercent), 
                HP_BAR_HEIGHT
            );

            // Draw fancy HP bar with border and gradient
            using (LinearGradientBrush greenGradient = new LinearGradientBrush(
                    playerHealth, 
                    Color.LightGreen, 
                    Color.DarkGreen, 
                    LinearGradientMode.Vertical))
            using (LinearGradientBrush redGradient = new LinearGradientBrush(
                    playerBackground, 
                    Color.DarkRed, 
                    Color.Red, 
                    LinearGradientMode.Vertical))
            using (Pen borderPen = new Pen(Color.Black, 2))
            {
                // Draw shadow
                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(shadowBrush, 
                        new Rectangle(playerBackground.X + 2, playerBackground.Y + 2, 
                                    playerBackground.Width, playerBackground.Height));
                }

                // Draw background (red part)
                e.Graphics.FillRectangle(redGradient, playerBackground);
                
                // Draw health (green part)
                e.Graphics.FillRectangle(greenGradient, playerHealth);
                
                // Draw border
                e.Graphics.DrawRectangle(borderPen, playerBackground);
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
                case Keys.E:
                    // Check both skill completion and cooldown
                    TimeSpan timeSinceLastSkill = DateTime.Now - lastSkillTime;
                    if (!isSkillActive && skillCompleted && timeSinceLastSkill.TotalMilliseconds >= SKILL_COOLDOWN)
                    {
                        isSkillActive = true;
                        skillCompleted = false;
                        currentSkillRepetition = 0;
                        skillStartTime = DateTime.Now;
                        lastSkillTime = DateTime.Now; // Update last skill time
                        playerSkillAnimation?.Stop();
                        playerSkillAnimation?.Start();
                    }
                    break;
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
            Point newMouseWorldPos = new Point(
                e.Location.X + cameraOffset.X,
                e.Location.Y + cameraOffset.Y
            );

            // Only update facing direction if mouse has actually moved
            if (newMouseWorldPos != lastMouseWorldPos)
            {
                hasMouseMoved = true;
                lastMouseWorldPos = newMouseWorldPos;
                currentMousePosition = e.Location;
            }
            
            mouseWorldPos = newMouseWorldPos;
        }

        private void GameForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Check cooldown
                TimeSpan timeSinceLastBullet = DateTime.Now - lastBulletTime;
                if (timeSinceLastBullet.TotalMilliseconds < BULLET_COOLDOWN)
                {
                    return; // Still in cooldown, don't shoot
                }

                // Update last bullet time
                lastBulletTime = DateTime.Now;

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
                    VelocityY = velocityY,
                    Damage = BULLET_DAMAGE
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
            playerIdleAnimation?.Dispose();
            playerRunAnimation?.Dispose();
            playerSkillAnimation?.Dispose(); // Add skill animation disposal
            gunSprite?.Dispose();
            if (grassTiles != null)
            {
                foreach (var tile in grassTiles)
                {
                    tile?.Dispose();
                }
            }
            bushSprite?.Dispose();
            foreach (var enemy in enemies)
            {
                enemy?.Dispose();
            }
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            this.Close(); // Menutup GameForm dan kembali ke MainForm
        }

        private void GenerateRandomBushes()
        {
            for (int i = 0; i < NUM_BUSHES; i++)
            {
                int x = random.Next(WORLD_WIDTH - BUSH_SIZE);
                int y = random.Next(WORLD_HEIGHT - BUSH_SIZE);
                
                // Collider lebih kecil dan tetap di 1/3 bagian bawah
                Rectangle collider = new Rectangle(
                    x + BUSH_SIZE/4,              // Offset dari pinggir 1/4 ukuran
                    y + (BUSH_SIZE * 1/3),        // Posisi Y di 2/3 tinggi sprite
                    BUSH_SIZE/2,                  // Lebar collider 1/2 dari sprite
                    BUSH_SIZE/3                   // Tinggi collider 1/3 dari sprite
                );
                
                bushColliders.Add(collider);
            }
        }

        private bool CheckBushCollision(Point newPosition)
        {
            var (scaledWidth, scaledHeight) = GetScaledDimensions();
            
            Rectangle playerRect = new Rectangle(
                newPosition.X - scaledWidth/2,
                newPosition.Y - scaledHeight/2,
                scaledWidth,
                scaledHeight
            );

            foreach (var bush in bushColliders)
            {
                if (playerRect.IntersectsWith(bush))
                {
                    return true;
                }
            }
            return false;
        }

        private void PreRenderTileMap()
        {
            if (grassTiles == null) return;

            tileMapBuffer = new Bitmap(WORLD_WIDTH, WORLD_HEIGHT);
            using (Graphics g = Graphics.FromImage(tileMapBuffer))
            {
                // Set pixel-perfect rendering for tiles
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.AssumeLinear;
                g.SmoothingMode = SmoothingMode.None;

                // Draw all tiles
                for (int x = 0; x < tileMap.GetLength(0); x++)
                {
                    for (int y = 0; y < tileMap.GetLength(1); y++)
                    {
                        int tileIndex = tileMap[x, y];
                        g.DrawImage(grassTiles[tileIndex],
                            x * TILE_SIZE,
                            y * TILE_SIZE,
                            TILE_SIZE,
                            TILE_SIZE);
                    }
                }
            }
        }

        private void LoadEnemies()
        {
            try
            {
                foreach (var enemyType in enemyConfigs.Keys)
                {
                    string basePath = Path.Combine(ENEMY_PATH, enemyType);
                    var config = enemyConfigs[enemyType];

                    var idleAnim = new SpriteAnimation(
                        Image.FromFile(Path.Combine(basePath, $"{enemyType}_Idle.png")),
                        config.width, config.height, config.idleFrames, 100);

                    var walkAnim = new SpriteAnimation(
                        Image.FromFile(Path.Combine(basePath, $"{enemyType}_Walk.png")),
                        config.width, config.height, config.walkFrames, 100);

                    var attackAnim = new SpriteAnimation(
                        Image.FromFile(Path.Combine(basePath, $"{enemyType}_Attack.png")),
                        config.width, config.height, config.attackFrames, 100);

                    // Create enemy at random position
                    Point randomPos = new Point(
                        random.Next(WORLD_WIDTH),
                        random.Next(WORLD_HEIGHT)
                    );

                    // Add health and speed parameters
                    enemies.Add(new Enemy(
                        enemyType, 
                        randomPos, 
                        idleAnim, 
                        walkAnim, 
                        attackAnim,
                        difficultySettings.enemyHealth,
                        difficultySettings.enemySpeed
                    ));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading enemy sprites: {ex.Message}", ex);
            }
        }

        private void InitializePlayerStats()
        {
            // Increase health values significantly
            (playerMaxHealth, playerAttack, playerSpeed) = GameState.SelectedCharacter switch
            {
                "Ninja" => (200, 20, 12),      // Fast but medium health
                "Puppeteer" => (175, 25, 10),  // Medium health and speed
                "Samurai" => (250, 30, 8),     // High health but slower
                "Scarecrow" => (180, 22, 11),  // Balanced stats
                "Shaman" => (160, 35, 9),      // Lower health but high attack
                _ => (200, 20, 10)
            };
            playerCurrentHealth = playerMaxHealth;
        }

        private void SpawnNewEnemy()
        {
            // Spawn enemy at random edge of the screen
            Point spawnPos;
            int edge = random.Next(4);
            
            switch (edge)
            {
                case 0: // Top
                    spawnPos = new Point(random.Next(WORLD_WIDTH), 0);
                    break;
                case 1: // Right
                    spawnPos = new Point(WORLD_WIDTH, random.Next(WORLD_HEIGHT));
                    break;
                case 2: // Bottom
                    spawnPos = new Point(random.Next(WORLD_WIDTH), WORLD_HEIGHT);
                    break;
                default: // Left
                    spawnPos = new Point(0, random.Next(WORLD_HEIGHT));
                    break;
            }

            string[] enemyTypes = { "Slime", "Bomb Puppet" };
            string enemyType = enemyTypes[random.Next(enemyTypes.Length)];
            
            var config = enemyConfigs[enemyType];
            var enemy = new Enemy(
                enemyType,
                spawnPos,
                new SpriteAnimation(Image.FromFile(Path.Combine(ENEMY_PATH, enemyType, $"{enemyType}_Idle.png")), config.width, config.height, config.idleFrames, 100),
                new SpriteAnimation(Image.FromFile(Path.Combine(ENEMY_PATH, enemyType, $"{enemyType}_Walk.png")), config.width, config.height, config.walkFrames, 100),
                new SpriteAnimation(Image.FromFile(Path.Combine(ENEMY_PATH, enemyType, $"{enemyType}_Attack.png")), config.width, config.height, config.attackFrames, 100),
                difficultySettings.enemyHealth,
                difficultySettings.enemySpeed
            );

            enemies.Add(enemy);
        }

        private void HandleBulletCollisions()
        {
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                var bullet = bullets[i];
                Rectangle bulletRect = new Rectangle(
                    (int)bullet.X - BULLET_SIZE/2,
                    (int)bullet.Y - BULLET_SIZE/2,
                    BULLET_SIZE,
                    BULLET_SIZE
                );

                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    var enemy = enemies[j];
                    if (enemy.IsActive && bulletRect.IntersectsWith(enemy.Bounds))
                    {
                        if (enemy.TakeDamage(BULLET_DAMAGE))
                        {
                            // Enemy died, increase score
                            currentScore += enemy.Type == "Slime" ? 100 : 150;
                            scoreLabel.Text = $"Score: {currentScore}";
                            enemies.RemoveAt(j);
                        }
                        bullets.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private void GameOver()
        {
            gameTimer.Stop();
            MessageBox.Show($"Game Over! Final Score: {currentScore}", "Game Over",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
