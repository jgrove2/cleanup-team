using Godot;
using System;

public partial class DummyNPC : CharacterBody3D, IDamageable
{
    [Export] public int MaxHealth { get; set; } = 100;
    [Export] public int InventoryWidth { get; set; } = 5;
    [Export] public int InventoryHeight { get; set; } = 5;

    public HealthComponent Health { get; private set; } = null!;
    public InventoryComponent Inventory { get; private set; } = null!;
    public EquipmentComponent Equipment { get; private set; } = null!;
    public AnimationTree AnimTree { get; private set; } = null!;
    public StateManager<DummyNPC> stateManager = null!;

    // Tracks the active hit-flash tween so a new hit cancels the previous one.
    private Tween _hitTween;

    public override void _Ready()
    {
        Health = new HealthComponent(MaxHealth);
        Inventory = new InventoryComponent(InventoryWidth, InventoryHeight);
        Equipment = new EquipmentComponent();
        Equipment.EquipInHand(new WeaponBasicClub(), HandRequirement.LeftOnly);

        var visual = GetNodeOrNull<EquipmentVisualComponent>("EquipmentVisualComponent");
        if (visual != null)
            visual.Initialize(Equipment, this);
        else
            GD.PushWarning("[DummyNPC] No EquipmentVisualComponent child found. Add it to the scene to enable weapon visuals.");

        AnimTree = GetNode<AnimationTree>("AnimationTree");
        stateManager = new StateManager<DummyNPC>(this);
        // Defer activation to the next frame so the AnimationTree fully
        // initialises its parameter table before the first state writes to it.
        CallDeferred(MethodName.StartAnimationTree);
    }

    private void StartAnimationTree()
    {
        AnimTree.Active = true;
        stateManager.TransitionToState<IdleDummyNPCState>();
    }

    public override void _PhysicsProcess(double delta)
    {
        stateManager?.Update(delta);
    }

    public void ReceiveDamage(DamageEffect effect, float multiplier)
    {
        if (!Health.IsAlive()) return;

        int raw = (int)Math.Round(effect.DamageAmount * multiplier);
        int armor = effect.DamageType == DamageType.Physical
            ? Equipment.GetTotalProtection(ProtectionType.Physical)
            : 0;
        Health.TakeDamage(Math.Max(0, raw - armor));

        if (Health.IsAlive())
            PlayHitAnimation();
    }

    /// <summary>
    /// Briefly blends the hit animation over idle, then fades back.
    /// Killing the previous tween on rapid hits restarts the flash cleanly.
    /// </summary>
    private void PlayHitAnimation()
    {
        _hitTween?.Kill();
        AnimTree.Set("parameters/hit blend/blend_amount", 1.0);
        _hitTween = CreateTween();
        _hitTween.TweenProperty(AnimTree, "parameters/hit blend/blend_amount", 0.0, 0.6);
    }
}
