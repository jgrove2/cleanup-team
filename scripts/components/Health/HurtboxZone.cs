using Godot;

/// <summary>
/// Attach to each zone Area3D on a character (e.g. "head", "body").
/// The weapon hitbox reads <see cref="DamageMultiplier"/> when an overlap occurs
/// and walks up to the scene root to find the <see cref="IDamageable"/> owner.
///
/// Collision setup (set in Inspector or project layer names):
///   Layer 2 — Hurtbox  (this node lives here)
///   Monitoring = false, Monitorable = true  (weapon detects us, not the reverse)
/// </summary>
[GlobalClass]
public partial class HurtboxZone : Area3D
{
    /// <summary>
    /// Damage multiplier applied when this zone is struck.
    /// Configure per-zone in the Inspector (e.g. head = 1.5, body = 1.0).
    /// </summary>
    [Export] public float DamageMultiplier { get; set; } = 1.0f;
}
