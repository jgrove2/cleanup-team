using System;
using System.Collections.Generic;

public class InventoryComponent
{
    public int Width { get; }
    public int Height { get; }

    private readonly InventoryEntry[,] _grid;
    private readonly List<InventoryEntry> _entries;

    public IReadOnlyList<InventoryEntry> Entries => _entries;

    public InventoryComponent(int width, int height)
    {
        Width = width;
        Height = height;
        _grid = new InventoryEntry[width, height];
        _entries = new List<InventoryEntry>();
    }

    public bool TryAdd(BaseItem item)
    {
        if (item is IStackable incoming)
        {
            foreach (var entry in _entries)
            {
                if (entry.Item.GetType() == item.GetType() && entry.Item is IStackable existing && !existing.IsFull)
                {
                    int space = existing.MaxStack - existing.CurrentStack;
                    int transfer = Math.Min(space, incoming.CurrentStack);
                    existing.CurrentStack += transfer;
                    incoming.CurrentStack -= transfer;
                    if (incoming.CurrentStack <= 0) return true;
                }
            }

            if (incoming.CurrentStack <= 0) return true;
        }

        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                if (CanFit(item, x, y))
                {
                    Place(item, x, y);
                    return true;
                }

        return false;
    }

    public bool TryPlace(BaseItem item, int x, int y)
    {
        if (!CanFit(item, x, y)) return false;
        Place(item, x, y);
        return true;
    }

    public bool Remove(BaseItem item)
    {
        var entry = FindEntry(item);
        if (entry == null) return false;

        _entries.Remove(entry);
        for (int dy = 0; dy < item.Size.Height; dy++)
            for (int dx = 0; dx < item.Size.Width; dx++)
                _grid[entry.X + dx, entry.Y + dy] = null;

        return true;
    }

    public bool CanFit(BaseItem item, int x, int y)
    {
        if (x < 0 || y < 0 || x + item.Size.Width > Width || y + item.Size.Height > Height)
            return false;

        for (int dy = 0; dy < item.Size.Height; dy++)
            for (int dx = 0; dx < item.Size.Width; dx++)
                if (_grid[x + dx, y + dy] != null) return false;

        return true;
    }

    public InventoryEntry GetAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height) return null;
        return _grid[x, y];
    }

    public bool Contains(BaseItem item) => FindEntry(item) != null;

    private void Place(BaseItem item, int x, int y)
    {
        var entry = new InventoryEntry(item, x, y);
        _entries.Add(entry);
        for (int dy = 0; dy < item.Size.Height; dy++)
            for (int dx = 0; dx < item.Size.Width; dx++)
                _grid[x + dx, y + dy] = entry;
    }

    private InventoryEntry FindEntry(BaseItem item)
    {
        foreach (var entry in _entries)
            if (ReferenceEquals(entry.Item, item)) return entry;
        return null;
    }
}
