using System;

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare
}

public enum ItemCategory
{
    Weapon,
    Consumable,
    Wearable,
    Quest
}

public enum EquipmentSlot
{
    Head,
    Body,
    Legs,
    Feet,
    LeftHand,
    RightHand,
    Bag
}

public abstract class BaseItem
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ItemRarity Rarity { get; set; }
    public ItemCategory Category { get; set; }
    public StatusEffect StatusEffect { get; set; }
    public ItemSize Size { get; protected set; }
    public BaseItem(string name, string description, ItemRarity rarity, ItemCategory category, StatusEffect statusEffect, ItemSize size = default)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Rarity = rarity;
        Category = category;
        StatusEffect = statusEffect;
        Size = size.Width > 0 ? size : ItemSize.S1x1;
    }
    public override string ToString()
    {
        return $"{Name}({Rarity} {Category}): - {Description}";
    }
}

public abstract class Weapon : BaseItem
{
    public Weapon(string name, string description, ItemRarity rarity, DamageEffect statusEffect, ItemSize size = default) : base(name, description, rarity, ItemCategory.Weapon, statusEffect, size)
    {}
}

public abstract class Consumable : BaseItem
{
    public Consumable(string name, string description, ItemRarity rarity, StatusEffect statusEffect, ItemSize size = default) : base(name, description, rarity, ItemCategory.Consumable, statusEffect, size)
    {
    }
}

public abstract class Wearable : BaseItem
{
    public EquipmentSlot Slot { get; protected set; }
    public Wearable(string name, string description, ItemRarity rarity, EquipmentSlot slot, StatusEffect statusEffect = null, ItemSize size = default) : base(name, description, rarity, ItemCategory.Wearable, statusEffect, size)
    {
        Slot = slot;
    }
}

