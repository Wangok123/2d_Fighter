namespace UnityCore.GameModule.Battle.Attributes
{
    public enum ModifierSource
    {
        None = 0,
        
        BaseValue = 1,
        Equipment = 10,
        Buff = 20,
        Debuff = 21,
        Skill = 30,
        Talent = 40,
        Achievement = 50,
        Temporary = 100,
    }
}
