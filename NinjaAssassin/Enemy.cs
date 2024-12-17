using System;
using System.Drawing;
using System.Collections.Generic;

namespace FormNavigation
{
    public class Enemy
    {
        // Add sprite configuration fields
        private readonly Dictionary<string, (int width, int height, int scale)> spriteConfigs = new Dictionary<string, (int width, int height, int scale)>
        {
            { "Slime", (64, 64, 2) },         // Original size 64x64, scale 2x
            { "Bomb Puppet", (50, 50, 2) }     // Original size 50x50, scale 2x
        };

        // Add new attack timing configuration
        private readonly Dictionary<string, (float attackCooldown, float attackDuration, float explosionDelay)> attackConfigs = 
        new Dictionary<string, (float attackCooldown, float attackDuration, float explosionDelay)>
        {
            { "Slime", (1500f, 500f, 0f) },           // Attack every 1.5 sec, attack animation 0.5 sec
            { "Bomb Puppet", (0f, 2000f, 1500f) }     // No cooldown, explosion wind-up 2 sec, explosion after 1.5 sec
        };

        // Add frame timing configuration
        private readonly Dictionary<string, (int attackFrames, int explosionFrame)> animationConfigs = 
        new Dictionary<string, (int attackFrames, int explosionFrame)>
        {
            { "Slime", (15, 0) },             // 15 attack frames, no explosion
            { "Bomb Puppet", (20, 20) }       // 20 attack frames, explodes at frame 15 (75% through animation)
        };

        private float attackCooldown;
        private float attackDuration;
        private float explosionDelay;

        private int spriteWidth;
        private int spriteHeight;
        private int spriteScale;

        private int currentAttackFrame = 0;
        private int totalAttackFrames;
        private int explosionFrame;

        public Point Position { get; set; }
        public string Type { get; private set; }
        public SpriteAnimation IdleAnimation { get; private set; }
        public SpriteAnimation WalkAnimation { get; private set; }
        public SpriteAnimation AttackAnimation { get; private set; }
        public bool IsActive { get; set; } = true;
        public string CurrentState { get; set; } = "Idle";
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public int Attack { get; private set; }
        public float Speed { get; private set; }
        private const int ENEMY_SIZE = 128; // Changed from 64 to 128 (2x larger)
        public Rectangle Bounds => new Rectangle(Position.X - spriteWidth * spriteScale / 2, Position.Y - spriteHeight * spriteScale / 2, spriteWidth * spriteScale, spriteHeight * spriteScale);
        private bool isFacingLeft = false;
        private const float ATTACK_RANGE = 100f; // Default attack range
        private const float EXPLOSION_RANGE = 150f; // Range for Bomb Puppet explosion
        private DateTime lastAttackTime = DateTime.MinValue;
        private const float ATTACK_COOLDOWN = 1000f; // 1 second between attacks
        public bool IsExploding { get; private set; } = false;
        public bool HasExploded { get; private set; } = false;
        private DateTime explosionStartTime;
        private const float EXPLOSION_DURATION = 1500f; // Changed from 500f to 1000f (1 second explosion)

        public Enemy(string type, Point position, SpriteAnimation idle, SpriteAnimation walk, SpriteAnimation attack, 
                    int health, float speed)
        {
            Type = type;
            Position = position;
            
            // Set sprite dimensions based on enemy type
            if (spriteConfigs.TryGetValue(type, out var config))
            {
                spriteWidth = config.width;
                spriteHeight = config.height;
                spriteScale = config.scale;
            }
            else
            {
                // Default values if type not found
                spriteWidth = 64;
                spriteHeight = 64;
                spriteScale = 1;
            }

            // Set attack timing configuration
            if (attackConfigs.TryGetValue(type, out var attackConfig))
            {
                attackCooldown = attackConfig.attackCooldown;
                attackDuration = attackConfig.attackDuration;
                explosionDelay = attackConfig.explosionDelay;
            }
            else
            {
                // Default values if type not found
                attackCooldown = 1000f;
                attackDuration = 500f;
                explosionDelay = 0f;
            }

            // Set animation frame configuration
            if (animationConfigs.TryGetValue(type, out var animConfig))
            {
                totalAttackFrames = animConfig.attackFrames;
                explosionFrame = animConfig.explosionFrame;
            }
            else
            {
                totalAttackFrames = 15;
                explosionFrame = 0;
            }

            // Initialize animations with correct dimensions
            IdleAnimation = idle;
            WalkAnimation = walk;
            AttackAnimation = attack;
            
            MaxHealth = health;
            CurrentHealth = health;
            Speed = speed;
            
            IdleAnimation.Start();
            // Reduce damage values
            Attack = type == "Bomb Puppet" ? 35 : 15; // Bomb does more damage but not instant kill
        }

        public void Update()
        {
            if (IsExploding)
            {
                TimeSpan explosionTime = DateTime.Now - explosionStartTime;
                if (explosionTime.TotalMilliseconds >= EXPLOSION_DURATION && HasExploded)
                {
                    IsActive = false;
                    return;
                }
            }

            // Update animation state
            switch (CurrentState)
            {
                case "Idle":
                    // Only start idle if not already playing
                    if (!IdleAnimation.IsPlaying)
                    {
                        WalkAnimation?.Stop();
                        AttackAnimation?.Stop();
                        IdleAnimation?.Start();
                    }
                    break;
                case "Walk":
                    // Only start walk if not already playing
                    if (!WalkAnimation.IsPlaying)
                    {
                        IdleAnimation?.Stop();
                        AttackAnimation?.Stop();
                        WalkAnimation?.Start();
                    }
                    break;
                case "Attack":
                    // Only start attack if not already playing
                    if (!AttackAnimation.IsPlaying)
                    {
                        IdleAnimation?.Stop();
                        WalkAnimation?.Stop();
                        AttackAnimation?.Start();
                    }

                    // Handle attack timing
                    if (DateTime.Now - lastAttackTime > TimeSpan.FromMilliseconds(100))
                    {
                        currentAttackFrame++;
                        lastAttackTime = DateTime.Now;
                    
                        if (Type == "Bomb Puppet")
                        {
                            if (currentAttackFrame == explosionFrame && !IsExploding)
                            {
                                StartExplosion();
                            }
                            if (!HasExploded)
                            {
                                currentAttackFrame = Math.Min(currentAttackFrame, explosionFrame);
                            }
                        }
                        else if (currentAttackFrame >= totalAttackFrames)
                        {
                            CurrentState = "Idle";
                            currentAttackFrame = 0;
                        }
                    }
                    break;
            }
        }

        public void MoveTowardsPlayer(Point playerPosition)
        {
            if (IsExploding) return; // Don't move while exploding

            // Calculate distance to player
            float dx = playerPosition.X - Position.X;
            float dy = playerPosition.Y - Position.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            // Update facing direction
            isFacingLeft = dx < 0;

            // Check if in attack range
            if (distance <= ATTACK_RANGE)
            {
                if (Type == "Bomb Puppet" && !IsExploding)
                {
                    StartExplosion();
                }
                else if (Type == "Slime")
                {
                    TryAttack();
                }
                CurrentState = "Attack";
                return;
            }

            // Normalize the direction
            if (distance > 0)
            {
                dx /= distance;
                dy /= distance;

                // Update position
                Position = new Point(
                    Position.X + (int)(dx * Speed),
                    Position.Y + (int)(dy * Speed)
                );

                // Update animation state
                CurrentState = "Walk";
            }
            else
            {
                CurrentState = "Idle";
            }
        }

        private void StartExplosion()
        {
            IsExploding = true;
            explosionStartTime = DateTime.Now;
        }

        private void TryAttack()
        {
            TimeSpan timeSinceLastAttack = DateTime.Now - lastAttackTime;
            if (timeSinceLastAttack.TotalMilliseconds >= attackCooldown)
            {
                CurrentState = "Attack";
                lastAttackTime = DateTime.Now;
            }
        }

        public bool ShouldDealDamage(Point playerPosition)
        {
            if (Type == "Bomb Puppet" && IsExploding)
            {
                // Only deal damage exactly at explosion frame
                if (!HasExploded && currentAttackFrame == explosionFrame)
                {
                    float distance = (float)Math.Sqrt(
                        Math.Pow(playerPosition.X - Position.X, 2) +
                        Math.Pow(playerPosition.Y - Position.Y, 2)
                    );
                    if (distance <= EXPLOSION_RANGE)
                    {
                        HasExploded = true;
                        return true;  // Deal damage only if player is within range
                    }
                    HasExploded = true;  // Mark as exploded even if player isn't hit
                }
                return false;
            }
            else if (Type == "Slime" && CurrentState == "Attack")
            {
                // Deal damage in middle of attack animation
                if (currentAttackFrame >= totalAttackFrames / 2 && currentAttackFrame <= totalAttackFrames * 2/3)
                {
                    float distance = (float)Math.Sqrt(
                        Math.Pow(playerPosition.X - Position.X, 2) +
                        Math.Pow(playerPosition.Y - Position.Y, 2)
                    );
                    return distance <= ATTACK_RANGE;
                }
            }
            return false;
        }

        public void Draw(Graphics g, Rectangle destRect)
        {
            var state = g.Save();
            try
            {
                if (isFacingLeft)
                {
                    g.TranslateTransform(Position.X, Position.Y);
                    g.ScaleTransform(-1, 1);
                    
                    Rectangle flippedRect = new Rectangle(
                        -spriteWidth * spriteScale / 2,
                        -spriteHeight * spriteScale / 2,
                        spriteWidth * spriteScale,
                        spriteHeight * spriteScale
                    );
                    DrawAnimation(g, flippedRect);
                }
                else
                {
                    Rectangle scaledRect = new Rectangle(
                        Position.X - spriteWidth * spriteScale / 2,
                        Position.Y - spriteHeight * spriteScale / 2,
                        spriteWidth * spriteScale,
                        spriteHeight * spriteScale
                    );
                    DrawAnimation(g, scaledRect);
                }
            }
            finally
            {
                g.Restore(state);
            }
        }

        private void DrawAnimation(Graphics g, Rectangle destRect)
        {
            switch (CurrentState)
            {
                case "Idle":
                    IdleAnimation?.DrawFrame(g, destRect);
                    break;
                case "Walk":
                    WalkAnimation?.DrawFrame(g, destRect);
                    break;
                case "Attack":
                    AttackAnimation?.DrawFrame(g, destRect);
                    break;
            }
        }

        public void Dispose()
        {
            IdleAnimation?.Dispose();
            WalkAnimation?.Dispose();
            AttackAnimation?.Dispose();
        }

        public bool TakeDamage(int damage)
        {
            CurrentHealth -= damage;
            if (CurrentHealth <= 0)
            {
                IsActive = false;
                return true; // Enemy died
            }
            return false;
        }
    }
}
