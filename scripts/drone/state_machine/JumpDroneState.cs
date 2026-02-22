using Godot;

public partial class JumpDroneState : DroneStateMachine
{
    public override void Enter(Drone drone)
    {
        Vector3 velocity = drone.Velocity;
        velocity.Y = Drone.JumpVelocity;
        drone.Velocity = velocity;
    }

    public override void PreUpdate(Drone drone)
    {
        // Hand off to fall once the drone starts descending.
        // FallDroneState already handles gravity, MoveAndSlide, and landing.
        if (drone.Velocity.Y < 0f)
        {
            drone.stateManager.TransitionToState(new FallDroneState());
        }
    }

    public override void Update(Drone drone, double delta)
    {
        // Gravity accumulation, horizontal movement from input, and MoveAndSlide
        // are all handled by MovementComponent, same as Walk/Run/Fall.
        drone.Movement.Update(delta);
    }
}
