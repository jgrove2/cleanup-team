using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Attach to the Hitbox Area3D inside every weapon scene.
/// Detects overlapping <see cref="HurtboxZone"/> nodes and routes damage to the
/// character root that implements <see cref="IDamageable"/>.
///
/// Collision setup (set in Inspector or project layer names):
///   Layer 3 — Hitbox   (this node lives here)
///   Mask  2 — Hurtbox  (only detect HurtboxZone Area3Ds)
///   Monitoring = true, Monitorable = false
///
/// The CollisionShape3D child must start disabled (Disabled = true).
/// Call <see cref="EnableHitbox"/> / <see cref="DisableHitbox"/> from the swing state.
/// </summary>
[GlobalClass]
public partial class WeaponHitbox : Area3D
{
    private Weapon _weaponData;
    private Node _wielder;
    private CollisionShape3D _shape;

    // Tracks which hurtbox Area3D instances have already been hit this swing
    // to prevent the same target taking multiple hits as shapes stay overlapping.
    private readonly HashSet<ulong> _hitThisSwing = new();

    public override void _Ready()
    {
        // Find the first CollisionShape3D child to use as the hitbox shape.
        foreach (var child in GetChildren())
        {
            if (child is CollisionShape3D cs)
            {
                _shape = cs;
                break;
            }
        }

        if (_shape == null)
        {
            GD.PushError($"[WeaponHitbox] No CollisionShape3D child found on '{Name}'. Add one as a child.");
            return;
        }

        // Start disabled — the swing state enables it at the right moment.
        _shape.Disabled = true;

        AreaEntered += OnAreaEntered;
    }

    /// <summary>
    /// Call once after the weapon scene is instantiated and before the first swing.
    /// </summary>
    public void Initialize(Weapon weapon, Node wielder)
    {
        _weaponData = weapon ?? throw new ArgumentNullException(nameof(weapon));
        _wielder = wielder;
    }

    /// <summary>Enable the collision shape and reset the per-swing hit tracker.</summary>
    public void EnableHitbox()
    {
        if (_shape == null) return;
        _hitThisSwing.Clear();
        _shape.Disabled = false;
    }

    /// <summary>Disable the collision shape.</summary>
    public void DisableHitbox()
    {
        if (_shape == null) return;
        _shape.Disabled = true;
    }

    // -------------------------------------------------------------------------
    // Signal handler
    // -------------------------------------------------------------------------

    private void OnAreaEntered(Area3D area)
    {
        // Only process HurtboxZone areas.
        if (area is not HurtboxZone zone) return;

        // One hit per zone per swing.
        ulong id = zone.GetInstanceId();
        if (_hitThisSwing.Contains(id)) return;

        // Prevent hitting the wielder's own hurtbox.
        // Area3D.Owner is the scene root, which matches the wielder node.
        if (_wielder != null && zone.Owner == _wielder) return;

        // Walk to the IDamageable character root.
        // Works for both layouts:
        //   Drone  — zone is a direct child of CharacterBody3D (Owner == root)
        //   Dummy  — zone is under hurtbox Node3D; Owner still == scene root CharacterBody3D
        var target = zone.Owner as IDamageable;
        if (target == null) return;

        if (_weaponData.StatusEffect is not DamageEffect effect) return;

        _hitThisSwing.Add(id);
        target.ReceiveDamage(effect, zone.DamageMultiplier);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            AreaEntered -= OnAreaEntered;
        base.Dispose(disposing);
    }
}
