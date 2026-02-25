using Godot;

public partial class DummyNPC : CharacterBody3D
{
    [Export] public int MaxHealth { get; set; } = 100;
    [Export] public int InventoryWidth { get; set; } = 5;
    [Export] public int InventoryHeight { get; set; } = 5;

    public HealthComponent Health { get; private set; } = null!;
    public InventoryComponent Inventory { get; private set; } = null!;
    public EquipmentComponent Equipment { get; private set; } = null!;
    public StateManager<DummyNPC> stateManager = null!;

    public override void _Ready()
    {
        Health = new HealthComponent(MaxHealth);
        Inventory = new InventoryComponent(InventoryWidth, InventoryHeight);
        Equipment = new EquipmentComponent();

        stateManager = new StateManager<DummyNPC>(this);
        stateManager.TransitionToState<IdleDummyNPCState>();
    }

    public override void _PhysicsProcess(double delta)
    {
        stateManager?.Update(delta);
    }
}
