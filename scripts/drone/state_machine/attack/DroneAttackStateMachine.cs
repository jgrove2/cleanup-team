using Godot;

/// <summary>
/// Base class for all drone attack states.
/// Runs as a parallel axis alongside <see cref="DroneStateMachine"/> so that
/// attack animations can blend over movement animations independently.
/// </summary>
public class DroneAttackStateMachine : StateMachineBase<Drone>
{
    // -------------------------------------------------------------------------
    // Animation helpers — attack axis parameters
    // -------------------------------------------------------------------------

    /// <summary>
    /// Fire (or abort) the attack_moving OneShot node.
    /// Pass <see cref="AnimationNodeOneShot.OneShotRequest.Fire"/> to start it.
    /// </summary>
    protected static void FireAttackMoving(Drone drone)
        => drone.AnimTree.Set(
            "parameters/attack_moving/request",
            (int)AnimationNodeOneShot.OneShotRequest.Fire);

    /// <summary>
    /// Set the upper-body blend weight.
    /// 0.0 = full movement layer, 1.0 = full attack layer.
    /// </summary>
    protected static void SetAttackBlend(Drone drone, float blend)
        => drone.AnimTree.Set("parameters/attack_blend/blend_amount", (double)blend);

    /// <summary>
    /// Returns true while the attack_moving OneShot is still playing.
    /// Use this to detect when the animation has finished and transition back to idle.
    /// </summary>
    protected static bool IsAttackMovingActive(Drone drone)
        => (bool)drone.AnimTree.Get("parameters/attack_moving/active");

    protected static bool WantsAttack() => Input.IsActionJustPressed("main_attack");

    protected static bool hasInputDirection(Drone drone)
        => drone.getInputDirection() != Vector3.Zero;
}
