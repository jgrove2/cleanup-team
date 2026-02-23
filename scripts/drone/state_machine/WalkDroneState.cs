using Godot;

public partial class WalkDroneState : DroneStateMachine
{
    public override void Enter(Drone drone)
    {
        if (!drone.IsOnFloor())
        {
            drone.stateManager.TransitionToState<FallDroneState>();
            return;
        }

        drone.Movement.Speed = drone.WalkSpeed;
    }

    public override void PreUpdate(Drone drone)
    {
        base.PreUpdate(drone);

        if (!drone.IsOnFloor())
        {
            drone.stateManager.TransitionToState<FallDroneState>();
            return;
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
            drone.stateManager.TransitionToState<JumpDroneState>();
            return;
        }

        if (WantsCrouch())
        {
            drone.stateManager.TransitionToState<SneakDroneState>();
            return;
        }

        if (!hasInputDirection(drone))
        {
            drone.stateManager.TransitionToState<IdleDroneState>();
            return;
        }

        if (WantsRun(drone))
        {
            drone.stateManager.TransitionToState<RunDroneState>();
            return;
        }
    }

    public override void Update(Drone drone, double delta)
    {
        drone.Movement.Update(delta);
    }
}

