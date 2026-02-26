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

    // -------------------------------------------------------------------------
    // Animation helpers — write to AnimationTree parameters
    // -------------------------------------------------------------------------

    /// <summary>Request a movement-layer animation. Use "idle", "run", "walking", or "sneaking".</summary>
    protected static void SetMovement(Drone drone, string state)
        => drone.AnimTree.Set("parameters/movement/transition_request", state);

    /// <summary>
    /// Switch the is_crouching transition node. Use "yes" for the static crouch pose, "no" for Idle_A.
    /// Only relevant when movement is "idle".
    /// </summary>
    protected static void SetIsCrouching(Drone drone, string state)
        => drone.AnimTree.Set("parameters/is_crouching/transition_request", state);

    /// <summary>
    /// Set the timescale of the sneak_scale node. 0.0 freezes the animation, 1.0 plays normally.
    /// </summary>
    protected static void SetSneakTimescale(Drone drone, float scale)
        => drone.AnimTree.Set("parameters/sneak_scale/scale", (double)scale);

    /// <summary>
    /// Mirror the movement state into the attack_state transition node so the
    /// correct attack variant (idle / walking / sneaking / run) is always selected.
    /// Call this alongside every <see cref="SetMovement"/> call.
    /// </summary>
    protected static void SetAttackState(Drone drone, string state)
        => drone.AnimTree.Set("parameters/attack_state/transition_request", state);
}

