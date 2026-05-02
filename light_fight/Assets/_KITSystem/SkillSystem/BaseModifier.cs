namespace _KITSystem.SkillSystem
{
    [System.Serializable]
    public abstract class BaseModifier
    {
        public abstract ModifierType Type { get; }
    }
    
    [System.Serializable]
    public class StatModifier : BaseModifier
    {
        public string stat;
        public float additionValue;
        public float multiplierValue = 1;
        
        public override ModifierType Type => ModifierType.Stat;
    }

    [System.Serializable]
    public class StunModifier : BaseModifier
    {
        public float duration;
        public override ModifierType Type => ModifierType.Stun;
    }

    public enum ModifierType
    {
        Stat, 
        Stun,
    }
}