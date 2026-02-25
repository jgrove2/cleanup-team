public class HealthComponent
{
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public HealthComponent(int maxHealth)
    {
        MaxHealth = maxHealth;
        Health = maxHealth;
    }
    public HealthComponent() : this(1) { }
    public bool IsAlive() => Health > 0;
    public void TakeDamage(int damage)
    {
        Health -= damage;
        if ( Health < 0)
        {
            Health = 0;
        }
    }
    public void GainHealth(int amount)
    {
        Health += amount;
    }
    public void ResetHealth()
    {
        Health = MaxHealth;
    }
    public void SetMaxHealth(int maxHealth)
    {
        MaxHealth = maxHealth;
    }
}
