using System;

public class HeallingEffect : StatusEffect
{
    public int HealAmount { get; protected set; }
    public bool IsOverTime { get; protected set; }
    public HeallingEffect(string name, int healAmount, bool isOvertime = false): base(name, StatusEffectType.Heal)
    {
        HealAmount = healAmount;
        IsOverTime = isOvertime;
    }
}