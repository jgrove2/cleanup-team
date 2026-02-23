using Godot;

public class DroneStateMachine : StateMachineBase<Drone>
{
    protected static bool hasInputDirection(Drone drone)
        => drone.getInputDirection() != Vector3.Zero;

    /// <summary>
    /// Returns true when the player should be running.
    /// Default mode (toggle off): sprint held = run, released = walk.
    /// Toggled mode (toggle on):  sprint held = walk, released = run.
    /// </summary>
    protected static bool WantsRun(Drone drone)
    {
        bool sprintHeld = Input.IsActionPressed("sprint");
        return drone.IsWalkToggled ? !sprintHeld : sprintHeld;
    }

    protected static bool WantsJump() => Input.IsActionJustPressed("ui_accept");

    protected static bool WantsCrouch() => Input.IsActionPressed("crouch");
}

