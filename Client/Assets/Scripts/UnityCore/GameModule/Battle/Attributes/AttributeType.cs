namespace UnityCore.GameModule.Battle.Attributes
{
    public enum AttributeType
    {
        None = 0,
        
        MaxHP = 1,
        CurrentHP = 2,
        
        Attack = 10,
        Defense = 11,
        MagicAttack = 12,
        MagicDefense = 13,
        
        CriticalRate = 20,
        CriticalDamage = 21,
        
        MoveSpeed = 30,
        AttackSpeed = 31,
        JumpForce = 32,
        
        PhysicalResistance = 40,
        MagicResistance = 41,
        
        HealthRegeneration = 50,
        ManaRegeneration = 51,
    }
}
