using System.Collections.Generic;
using System.Linq;

public class Armor : Wearable
{
    public List<ProtectionEffect> ProtectionEffects { get; private set; }

    public Armor(
        string name,
        string description,
        ItemRarity rarity,
        EquipmentSlot slot,
        List<ProtectionEffect> protectionEffects,
        ItemSize size = default
    ) : base(name, description, rarity, slot, size: size)
    {
        ProtectionEffects = protectionEffects ?? new List<ProtectionEffect>();
    }

    public int GetProtection(ProtectionType type)
    {
        return ProtectionEffects
            .Where(e => e.ProtectionType == type)
            .Sum(e => e.ProtectionAmount);
    }
}
