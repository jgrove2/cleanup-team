using System;

[Flags]
public enum StatusEffectType
{
    None = 0,
    Heal = 1 << 0,
    Damage = 1 << 1,
    Protection = 1 << 2
}
public abstract class StatusEffect
{
    public string Name { get; protected set; }
    public StatusEffectType EffectType { get; set; }
    public StatusEffect(string name, StatusEffectType effectTypes)
    {
        EffectType = effectTypes;
        Name = name;
    }
    public bool HasEffect(StatusEffectType type) => EffectType.HasFlag(type);
    public void RemoveEffect(StatusEffectType type)
    {
        if (HasEffect(type))
        {
            EffectType &= ~type;
        }
    }
}