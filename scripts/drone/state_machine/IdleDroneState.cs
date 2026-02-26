using Godot;
using System;
using System.Text;

public partial class IdleDroneState : DroneStateMachine
{
    public override void Enter(Drone drone)
    {
        SetMovement(drone, "idle");
        SetAttackState(drone, "idle");
        SetIsCrouching(drone, "no");

        if (!drone.IsOnFloor())
        {
            drone.stateManager.TransitionToState<FallDroneState>();
        }
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
            drone.stateManager.TransitionToState<JumpDroneState>();
            return;
        }

        if (WantsCrouch())
        {
            drone.stateManager.TransitionToState<SneakDroneState>();
            return;
        }

        if (hasInputDirection(drone))
        {
            if (WantsRun(drone))
                drone.stateManager.TransitionToState<RunDroneState>();
            else
                drone.stateManager.TransitionToState<WalkDroneState>();
            return;
        }
    }
}
