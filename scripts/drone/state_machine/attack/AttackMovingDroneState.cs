/// <summary>
/// Attack-axis state active while the player is moving and has triggered an attack.
/// Blends the attack_moving OneShot over the movement layer on the upper body.
/// Returns to <see cref="IdleAttackDroneState"/> once the OneShot finishes.
/// </summary>
public partial class AttackMovingDroneState : DroneAttackStateMachine
{
    public override void Enter(Drone drone)
    {
        // Bring the attack layer fully in and fire the OneShot sequence.
        SetAttackBlend(drone, 1.0f);
        FireAttackMoving(drone);
    }

    public override void PreUpdate(Drone drone)
    {
        // OneShot sets 'active' to false when the animation finishes.
        if (!IsAttackMovingActive(drone))
        {
            drone.attackStateManager.TransitionToState<IdleAttackDroneState>();
        }
    }
}
