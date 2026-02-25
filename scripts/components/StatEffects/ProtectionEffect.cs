public enum ProtectionType
{
    Physical
}

public abstract class ProtectionEffect : StatusEffect
{
    public int ProtectionAmount { get; protected set; }
    public ProtectionType ProtectionType { get; protected set; }

    protected ProtectionEffect(
        string name,
        int protectionAmount,
        ProtectionType protectionType
    ) : base(name, StatusEffectType.Protection)
    {
        ProtectionAmount = protectionAmount;
        ProtectionType = protectionType;
    }
}

public class PhysicalProtectionEffect : ProtectionEffect
{
    public PhysicalProtectionEffect(string name, int protectionAmount)
        : base(name, protectionAmount, ProtectionType.Physical)
    {
    }
}