using System.Collections.Generic;

public enum HandRequirement
{
    Either,
    LeftOnly,
    RightOnly,
    BothHands
}

public class EquipmentComponent
{
    private static readonly HashSet<EquipmentSlot> ArmorSlots = [EquipmentSlot.Head, EquipmentSlot.Body, EquipmentSlot.Legs, EquipmentSlot.Feet];
    private static readonly HashSet<EquipmentSlot> HandSlots = [EquipmentSlot.LeftHand, EquipmentSlot.RightHand];

    private readonly Dictionary<EquipmentSlot, Wearable> _armorSlots;
    private readonly Dictionary<EquipmentSlot, BaseItem> _handSlots;
    private Wearable _bagSlot;

    public IReadOnlyDictionary<EquipmentSlot, Wearable> EquippedArmor => _armorSlots;
    public IReadOnlyDictionary<EquipmentSlot, BaseItem> EquippedInHands => _handSlots;
    public Wearable EquippedBag => _bagSlot;

    public EquipmentComponent()
    {
        _armorSlots = new Dictionary<EquipmentSlot, Wearable>();
        _handSlots = new Dictionary<EquipmentSlot, BaseItem>();
    }

    public bool EquipArmor(Wearable item)
    {
        if (!ArmorSlots.Contains(item.Slot)) return false;
        if (_armorSlots.ContainsKey(item.Slot)) return false;
        _armorSlots[item.Slot] = item;
        return true;
    }

    public bool EquipInHand(BaseItem item, HandRequirement requirement)
    {
        return requirement switch
        {
            HandRequirement.LeftOnly  => TryEquipHandSlot(EquipmentSlot.LeftHand, item),
            HandRequirement.RightOnly => TryEquipHandSlot(EquipmentSlot.RightHand, item),
            HandRequirement.BothHands => TryEquipBothHands(item),
            HandRequirement.Either    => TryEquipHandSlot(EquipmentSlot.LeftHand, item)
                                      || TryEquipHandSlot(EquipmentSlot.RightHand, item),
            _ => false
        };
    }

    private bool TryEquipHandSlot(EquipmentSlot slot, BaseItem item)
    {
        if (_handSlots.ContainsKey(slot)) return false;
        _handSlots[slot] = item;
        return true;
    }

    private bool TryEquipBothHands(BaseItem item)
    {
        if (_handSlots.ContainsKey(EquipmentSlot.LeftHand) || _handSlots.ContainsKey(EquipmentSlot.RightHand)) return false;
        _handSlots[EquipmentSlot.LeftHand] = item;
        _handSlots[EquipmentSlot.RightHand] = item;
        return true;
    }

    public Wearable UnequipArmor(EquipmentSlot slot)
    {
        if (!_armorSlots.TryGetValue(slot, out var item)) return null;
        _armorSlots.Remove(slot);
        return item;
    }

    public BaseItem UnequipHand(EquipmentSlot slot)
    {
        if (!_handSlots.TryGetValue(slot, out var item)) return null;
        _handSlots.Remove(slot);

        // Two-handed items share the same reference in both slots — clear the other slot too
        var otherSlot = slot == EquipmentSlot.LeftHand ? EquipmentSlot.RightHand : EquipmentSlot.LeftHand;
        if (_handSlots.TryGetValue(otherSlot, out var otherItem) && ReferenceEquals(item, otherItem))
            _handSlots.Remove(otherSlot);

        return item;
    }

    public Wearable GetEquippedArmor(EquipmentSlot slot)
    {
        _armorSlots.TryGetValue(slot, out var item);
        return item;
    }

    public BaseItem GetEquippedInHand(EquipmentSlot slot)
    {
        _handSlots.TryGetValue(slot, out var item);
        return item;
    }

    public bool IsTwoHanded(EquipmentSlot slot)
    {
        if (!HandSlots.Contains(slot)) return false;
        var otherSlot = slot == EquipmentSlot.LeftHand ? EquipmentSlot.RightHand : EquipmentSlot.LeftHand;
        return _handSlots.TryGetValue(slot, out var item)
            && _handSlots.TryGetValue(otherSlot, out var otherItem)
            && ReferenceEquals(item, otherItem);
    }

    public bool EquipBag(Wearable bag)
    {
        if (_bagSlot != null) return false;
        _bagSlot = bag;
        return true;
    }

    public Wearable UnequipBag()
    {
        var bag = _bagSlot;
        _bagSlot = null;
        return bag;
    }

    public Wearable GetEquippedBag() => _bagSlot;

    public bool IsSlotOccupied(EquipmentSlot slot)
    {
        if (slot == EquipmentSlot.Bag) return _bagSlot != null;
        if (ArmorSlots.Contains(slot)) return _armorSlots.ContainsKey(slot);
        if (HandSlots.Contains(slot)) return _handSlots.ContainsKey(slot);
        return false;
    }

    public int GetTotalProtection(ProtectionType type)
    {
        int total = 0;
        foreach (var item in _armorSlots.Values)
        {
            if (item is Armor armor)
                total += armor.GetProtection(type);
        }
        return total;
    }
}
