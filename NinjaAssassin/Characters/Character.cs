using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;

namespace FormNavigation.Characters
{
    public class Bullet
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public bool IsActive { get; set; } = true;
        public int Damage { get; set; }
    }

    public interface IAttack
    {
        int GetDamage();
        void PerformAttack(Point origin, Point target, List<Bullet> bullets);
        float GetCooldown();
    }

    public interface IWeapon
    {
        void Draw(Graphics g, Point position, float angle, bool isFacingLeft);
        IAttack GetAttack();
    }

    public class Gun : IWeapon
    {
        private Image weaponSprite;
        private GunAttack attack;
        private const int GUN_WIDTH = 70;
        private const int GUN_HEIGHT = 36;
        private const int GUN_OFFSET = 35;

        public Gun()
        {
            attack = new GunAttack();
            LoadSprite();
        }

        private void LoadSprite()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/Weapons/Gun1.png");
            if (File.Exists(path))
            {
                weaponSprite = Image.FromFile(path);
            }
        }

        public void Draw(Graphics g, Point position, float angle, bool isFacingLeft)
        {
            if (weaponSprite == null) return;

            var state = g.Save();
            try
            {
                g.TranslateTransform(position.X, position.Y);
                g.RotateTransform(angle * 180f / (float)Math.PI);

                if (isFacingLeft)
                {
                    g.ScaleTransform(1, -1);
                }

                g.DrawImage(weaponSprite,
                    GUN_OFFSET,
                    -20,
                    GUN_WIDTH,
                    GUN_HEIGHT);
            }
            finally
            {
                g.Restore(state);
            }
        }

        public IAttack GetAttack() => attack;

        public void Dispose()
        {
            weaponSprite?.Dispose();
        }
    }

    public class GunAttack : IAttack
    {
        private const float BULLET_SPEED = 20f;
        private const int BULLET_DAMAGE = 25;
        private const float COOLDOWN = 500f;

        public int GetDamage() => BULLET_DAMAGE;
        public float GetCooldown() => COOLDOWN;

        public void PerformAttack(Point origin, Point target, List<Bullet> bullets)
        {
            float dx = target.X - origin.X;
            float dy = target.Y - origin.Y;
            float angle = (float)Math.Atan2(dy, dx);

            float velocityX = (float)Math.Cos(angle) * BULLET_SPEED;
            float velocityY = (float)Math.Sin(angle) * BULLET_SPEED;

            const int GUN_OFFSET = 95;
            float bulletStartX = origin.X + (GUN_OFFSET * (float)Math.Cos(angle));
            float bulletStartY = origin.Y + (GUN_OFFSET * (float)Math.Sin(angle));

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

    public abstract class Character
    {
        public string Name { get; protected set; }
        public int MaxHealth { get; protected set; }
        public int Attack { get; protected set; }
        public float Speed { get; protected set; }
        public int IdleFrames { get; protected set; }
        public int RunFrames { get; protected set; }
        public int SkillFrames { get; protected set; }
        public int SpriteWidth { get; protected set; }
        public int SpriteHeight { get; protected set; }
        public int SkillInterval { get; protected set; }
        public int SkillRepetitions { get; protected set; }

        protected IWeapon weapon;

        public IWeapon GetWeapon() => weapon;

        protected virtual void InitializeWeapon()
        {
            weapon = new Gun(); // Default weapon
        }

        public abstract string GetDescription();
    }
}
