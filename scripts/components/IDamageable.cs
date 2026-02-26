/// <summary>
/// Implemented by any character or entity that can receive weapon damage.
/// Both the health source and equipped armor are exposed so damage callers
/// can apply mitigation before committing the final hit.
/// </summary>
public interface IDamageable
{
    HealthComponent Health { get; }
    EquipmentComponent Equipment { get; }

    /// <summary>
    /// Apply a weapon hit to this entity.
    /// Implementations are responsible for computing armor mitigation and
    /// forwarding the final value to <see cref="HealthComponent.TakeDamage"/>.
    /// </summary>
    /// <param name="effect">The damage effect from the weapon's StatusEffect.</param>
    /// <param name="multiplier">Zone-based multiplier (e.g. 1.5 for head shots).</param>
    void ReceiveDamage(DamageEffect effect, float multiplier);
}
