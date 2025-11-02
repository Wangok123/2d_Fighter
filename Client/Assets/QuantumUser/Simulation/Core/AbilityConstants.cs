namespace Quantum
{
    /// <summary>
    /// Shared constants for ability system identifiers.
    /// These IDs are used across multiple systems to identify specific abilities.
    /// </summary>
    public static class AbilityConstants
    {
        /// <summary>
        /// Movement ability identifiers
        /// </summary>
        public static class Movement
        {
            public const string DoubleJump = "movement_double_jump";
            public const string Dash = "movement_dash";
            public const string WallJump = "movement_wall_jump";
            public const string AirDash = "movement_air_dash";
            public const string Glide = "movement_glide";
        }

        /// <summary>
        /// Attack ability identifiers
        /// </summary>
        public static class Attack
        {
            public const string LightAttack = "attack_light";
            public const string HeavyAttack = "attack_heavy";
            public const string RangedAttack = "attack_ranged";
            public const string AreaAttack = "attack_area";
        }

        /// <summary>
        /// Defense ability identifiers
        /// </summary>
        public static class Defense
        {
            public const string Block = "defense_block";
            public const string Parry = "defense_parry";
            public const string Dodge = "defense_dodge";
            public const string Shield = "defense_shield";
        }

        /// <summary>
        /// Special ability identifiers
        /// </summary>
        public static class Special
        {
            public const string Ultimate = "special_ultimate";
            public const string Transformation = "special_transformation";
            public const string Summon = "special_summon";
        }
    }
}
