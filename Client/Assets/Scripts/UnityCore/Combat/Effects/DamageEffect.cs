namespace Combat.Effects
{
    public class DamageEffect : IEffect<Enemy>
    {
        public int DamageAmount = 10;
        
        public void Apply(Enemy target)
        {
            target.TakeDamage(DamageAmount);
        }

        public void Cancel()
        {
            // noop
        }
    }

    public class Enemy
    {
        public void TakeDamage(int damageAmount)
        {
            throw new System.NotImplementedException();
        }
    }
}