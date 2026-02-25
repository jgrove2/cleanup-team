public class InventoryEntry
{
    public BaseItem Item { get; }
    public int X { get; }
    public int Y { get; }

    public InventoryEntry(BaseItem item, int x, int y)
    {
        Item = item;
        X = x;
        Y = y;
    }
}
