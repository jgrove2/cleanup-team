using Godot;

public partial class RunDroneState : DroneStateMachine
{
    public override void Enter(Drone drone)
    {
        if (!drone.IsOnFloor())
        {
            drone.stateManager.TransitionToState(new FallDroneState());
            return;
        }

        drone.Movement.Speed = Drone.RunSpeed;
    }

    public override void PreUpdate(Drone drone)
    {
        if (!drone.IsOnFloor())
        {
            drone.stateManager.TransitionToState(new FallDroneState());
            return;
        }

        if (WantsJump())
        {
            drone.stateManager.TransitionToState(new JumpDroneState());
            return;
        }

        if (!hasInputDirection(drone))
        {
            drone.stateManager.TransitionToState(new IdleDroneState());
            return;
        }

        if (!WantsRun(drone))
        {
            drone.stateManager.TransitionToState(new WalkDroneState());
            return;
        }
    }

    public override void Update(Drone drone, double delta)
    {
        drone.Movement.Update(delta);
    }
}

