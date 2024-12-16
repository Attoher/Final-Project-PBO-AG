using System;
using System.Drawing;

namespace FormNavigation
{
    public class Enemy
    {
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
        public Rectangle Bounds => new Rectangle(Position.X - ENEMY_SIZE/2, Position.Y - ENEMY_SIZE/2, ENEMY_SIZE, ENEMY_SIZE);
        private bool isFacingLeft = false;

        public Enemy(string type, Point position, SpriteAnimation idle, SpriteAnimation walk, SpriteAnimation attack)
        {
            Type = type;
            Position = position;
            IdleAnimation = idle;
            WalkAnimation = walk;
            AttackAnimation = attack;
            
            // Start idle animation by default
            IdleAnimation.Start();

            // Initialize stats based on enemy type
            (MaxHealth, Attack, Speed) = Type switch
            {
                "Slime" => (50, 10, 3.0f),
                "Bomb Puppet" => (75, 15, 2.0f),
                _ => (50, 10, 3.0f)
            };
            CurrentHealth = MaxHealth;
        }

        public void Update()
        {
            switch (CurrentState)
            {
                case "Idle":
                    IdleAnimation?.Start();
                    WalkAnimation?.Stop();
                    AttackAnimation?.Stop();
                    break;
                case "Walk":
                    IdleAnimation?.Stop();
                    WalkAnimation?.Start();
                    AttackAnimation?.Stop();
                    break;
                case "Attack":
                    IdleAnimation?.Stop();
                    WalkAnimation?.Stop();
                    AttackAnimation?.Start();
                    break;
            }
        }

        public void MoveTowardsPlayer(Point playerPosition)
        {
            // Calculate direction to player
            float dx = playerPosition.X - Position.X;
            float dy = playerPosition.Y - Position.Y;
            
            // Update facing direction
            isFacingLeft = dx < 0;

            // Normalize the direction
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);
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
                        -destRect.Width/2,
                        -destRect.Height/2,
                        destRect.Width,
                        destRect.Height
                    );
                    DrawAnimation(g, flippedRect);
                }
                else
                {
                    DrawAnimation(g, destRect);
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
