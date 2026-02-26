/// <summary>
/// Default attack-axis state — no attack is playing.
/// The upper-body blend weight is 0 so the movement layer is fully visible.
/// Transitions to <see cref="AttackMovingDroneState"/> when the player fires
/// a main attack while moving.
/// </summary>
public partial class IdleAttackDroneState : DroneAttackStateMachine
{
    public override void Enter(Drone drone)
    {
        // Return upper body to movement layer.
        SetAttackBlend(drone, 0.0f);
    }

    public override void PreUpdate(Drone drone)
    {
        if (!WantsAttack()) return;

        if (hasInputDirection(drone))
        {
            drone.attackStateManager.TransitionToState<AttackMovingDroneState>();
        }
        // TODO: idle attack and sneak attack states go here when ready.
    }
}
