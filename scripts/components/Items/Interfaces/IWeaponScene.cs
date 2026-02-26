/// <summary>
/// Implemented by the root Node3D of a weapon scene (e.g. BasicClubScene).
/// Allows the swing state machine to activate and deactivate the hitbox
/// without knowing the concrete weapon type.
/// </summary>
public interface IWeaponScene
{
    /// <summary>
    /// Provide the weapon data and the wielder node so the hitbox can read
    /// damage values and guard against self-hits.
    /// </summary>
    void InitializeWeapon(Weapon weaponData, Godot.Node wielder);

    /// <summary>Activate the hitbox — call at the start of a swing.</summary>
    void EnableHitbox();

    /// <summary>Deactivate the hitbox — call at the end of a swing.</summary>
    void DisableHitbox();
}
