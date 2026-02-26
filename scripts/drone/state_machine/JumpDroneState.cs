using Godot;

public partial class JumpDroneState : DroneStateMachine
{
    public override void Enter(Drone drone)
    {
        SetMovement(drone, "idle");
        SetAttackState(drone, "idle");
        Vector3 velocity = drone.Velocity;
        velocity.Y = drone.JumpVelocity;
        drone.Velocity = velocity;
    }

    public override void PreUpdate(Drone drone)
    {
        base.PreUpdate(drone);

        // Hand off to fall once the drone starts descending.
        // FallDroneState already handles gravity, MoveAndSlide, and landing.
        if (drone.Velocity.Y < 0f)
        {
            drone.stateManager.TransitionToState<FallDroneState>();
        }
        if (WantsJump())
        {
            var (canVault, target, shouldCrouch) = drone.CheckVault();
            if (canVault)
            {
                drone.VaultTarget = target;
                drone.VaultShouldCrouch = shouldCrouch;
                drone.stateManager.TransitionToState<VaultDroneState>();
                return;
            }
        }
    }

    public override void Update(Drone drone, double delta)
    {
        // Gravity accumulation, horizontal movement from input, and MoveAndSlide
        // are all handled by MovementComponent, same as Walk/Run/Fall.
        drone.Movement.Update(delta);
    }
}
