namespace FormNavigation.Characters
{
    public class Ninja : Character
    {
        public Ninja()
        {
            Name = "Ninja";
            MaxHealth = 200;
            Attack = 20;
            Speed = 12;
            IdleFrames = 9;
            RunFrames = 6;
            SkillFrames = 11;
            SpriteWidth = 25;
            SpriteHeight = 25;
            SkillInterval = 50;
            SkillRepetitions = 1;
        }

        public override string GetDescription()
        {
            return "Swift assassin with high mobility and teleport skill.";
        }
    }

    public class Puppeteer : Character
    {
        public Puppeteer()
        {
            Name = "Puppeteer";
            MaxHealth = 175;
            Attack = 25;
            Speed = 10;
            IdleFrames = 4;
            RunFrames = 4;
            SkillFrames = 8;
            SpriteWidth = 25;
            SpriteHeight = 50;
            SkillInterval = 80;
            SkillRepetitions = 2;
        }

        public override string GetDescription()
        {
            return "Master of puppets with medium stats and summoning abilities.";
        }
    }

    public class Samurai : Character
    {
        public Samurai()
        {
            Name = "Samurai";
            MaxHealth = 250;
            Attack = 30;
            Speed = 8;
            IdleFrames = 4;
            RunFrames = 12;
            SkillFrames = 8;
            SpriteWidth = 50;
            SpriteHeight = 50;
            SkillInterval = 100;
            SkillRepetitions = 1;
        }

        public override string GetDescription()
        {
            return "Powerful warrior with high health and deadly sword skills.";
        }
    }

    public class Scarecrow : Character
    {
        public Scarecrow()
        {
            Name = "Scarecrow";
            MaxHealth = 180;
            Attack = 22;
            Speed = 11;
            IdleFrames = 4;
            RunFrames = 4;
            SkillFrames = 9;
            SpriteWidth = 25;
            SpriteHeight = 25;
            SkillInterval = 70;
            SkillRepetitions = 1;
        }

        public override string GetDescription()
        {
            return "Mysterious fighter with balanced stats and fear-inducing abilities.";
        }
    }

    public class Shaman : Character
    {
        public Shaman()
        {
            Name = "Shaman";
            MaxHealth = 160;
            Attack = 35;
            Speed = 9;
            IdleFrames = 4;
            RunFrames = 4;
            SkillFrames = 4;
            SpriteWidth = 25;
            SpriteHeight = 50;
            SkillInterval = 150;
            SkillRepetitions = 3;
        }

        public override string GetDescription()
        {
            return "Magical expert with high attack power and ritual-based skills.";
        }
    }
}
