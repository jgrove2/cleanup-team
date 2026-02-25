public readonly struct ItemSize
{
    public int Width { get; }
    public int Height { get; }

    public ItemSize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public static ItemSize S1x1 => new(1, 1);
    public static ItemSize S1x2 => new(1, 2);
    public static ItemSize S2x1 => new(2, 1);
    public static ItemSize S2x2 => new(2, 2);
    public static ItemSize S1x3 => new(1, 3);
    public static ItemSize S2x3 => new(2, 3);

    public override string ToString() => $"{Width}x{Height}";
}
