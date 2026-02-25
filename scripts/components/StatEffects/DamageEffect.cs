public enum DamageType
{
    Physical,
    Piercing
}

public class DamageEffect : StatusEffect
{
    public int DamageAmount { get; protected set; }
    public DamageType DamageType { get; protected set; }

    public DamageEffect(
        string name,
        int damageAmount,
        DamageType damageType = default
    ) : base(name, StatusEffectType.Damage)
    {
        DamageAmount = damageAmount;
        DamageType = damageType;
    }
}