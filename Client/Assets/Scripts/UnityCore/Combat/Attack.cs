namespace Combat
{
    public class Attack
    {
        private readonly int _damage;
        private readonly bool _isCritical;
        
        public Attack(int damage, bool isCritical)
        {
            _damage = damage;
            _isCritical = isCritical;
        }
        
        public int Damage => _damage;
        public bool IsCritical => _isCritical;
    }
}