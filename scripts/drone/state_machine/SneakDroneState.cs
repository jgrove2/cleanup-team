using Godot;

public partial class SneakDroneState : DroneStateMachine
{
    public override void Enter(Drone drone) => drone.Crouch();

    public override void Exit(Drone drone) => drone.Stand();

    public override void PreUpdate(Drone drone)
    {
        base.PreUpdate(drone);

        if (!drone.IsOnFloor())
        {
            drone.stateManager.TransitionToState<FallDroneState>();
            return;
        }

        if (!WantsCrouch() && drone.CanStand())
        {
            drone.stateManager.TransitionToState<IdleDroneState>();
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
        }
    }

    public override void Update(Drone drone, double delta)
    {
        drone.Movement.Update(delta);
    }
}
