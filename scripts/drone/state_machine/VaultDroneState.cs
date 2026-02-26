using Godot;

public partial class VaultDroneState : DroneStateMachine
{
    private Tween tween;

    public override void Enter(Drone drone)
    {
        drone.Velocity = Vector3.Zero;
        SetMovement(drone, "idle");
        SetAttackState(drone, "idle");

        if (drone.VaultShouldCrouch)
        {
            drone.Crouch();
        }

        float verticalDistance = Mathf.Abs(drone.VaultTarget.Y - drone.GlobalPosition.Y);
        float duration = Mathf.Max(verticalDistance / drone.VaultSpeed, 0.05f);

        tween = drone.CreateTween();
        tween.SetTrans(Tween.TransitionType.Sine);
        tween.SetEase(Tween.EaseType.OutIn);
        tween.TweenProperty(drone, "global_position", drone.VaultTarget, duration);
    }

    public override void Update(Drone drone, double delta)
    {
        drone.CameraControl.IsLocked = true;
        if (tween != null && !tween.IsRunning())
        {
            tween = null;

            // MoveAndSlide was never called during the tween, so IsOnFloor() is still
            // stale from when we were falling. Nudge down so Godot registers floor contact
            // before the transition checks it.
            drone.Velocity = new Vector3(0, -0.1f, 0);
            drone.MoveAndSlide();

            if (drone.VaultShouldCrouch)
            {
                drone.stateManager.TransitionToState<SneakDroneState>();
            }
            else
            {
                drone.stateManager.TransitionToState<IdleDroneState>();
            }
        }
    }

    public override void Exit(Drone drone)
    {
        if (tween != null)
        {
            tween.Kill();
            tween = null;
        }

        drone.CameraControl.IsLocked = false;
    }
}