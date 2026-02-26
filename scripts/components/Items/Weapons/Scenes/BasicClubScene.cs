using Godot;

/// <summary>
/// Root script for the BasicClub weapon scene (res://scenes/items/weapons/basic_club.tscn).
/// Implements <see cref="IWeaponScene"/> so the swing state can control the hitbox
/// without knowing the concrete weapon type.
///
/// In the Inspector, assign the child <c>Hitbox (Area3D / WeaponHitbox)</c> to the
/// <see cref="Hitbox"/> export.
/// </summary>
public partial class BasicClubScene : Node3D, IWeaponScene
{
    [Export] public WeaponHitbox Hitbox { get; set; }

    public void InitializeWeapon(Weapon weaponData, Node wielder)
    {
        if (Hitbox == null)
        {
            GD.PushError("[BasicClubScene] Hitbox is not assigned in the Inspector.");
            return;
        }
        Hitbox.Initialize(weaponData, wielder);
    }

    public void EnableHitbox()  => Hitbox?.EnableHitbox();
    public void DisableHitbox() => Hitbox?.DisableHitbox();
}
