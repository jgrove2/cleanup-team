public class Bag : Wearable
{
    public InventoryComponent Storage { get; }

    public Bag(
        string name,
        string description,
        ItemRarity rarity,
        int storageWidth = 5,
        int storageHeight = 5,
        ItemSize size = default
    ) : base(name, description, rarity, EquipmentSlot.Bag, size: size.Width > 0 ? size : ItemSize.S2x2)
    {
        Storage = new InventoryComponent(storageWidth, storageHeight);
    }
}
