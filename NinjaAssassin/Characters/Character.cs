namespace FormNavigation.Characters
{
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

        public abstract string GetDescription();
    }
}
