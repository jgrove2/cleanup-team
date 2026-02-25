public interface IStackable : IItemComponent
{
    int MaxStack { get; set; }
    int CurrentStack { get; set; }
    bool IsFull => CurrentStack >= MaxStack;
}