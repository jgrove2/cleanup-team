using Godot;
using System;
using System.Text;

public partial class IdleDroneState : DroneStateMachine
{
    public override void Enter(Drone drone)
    {
        if (!drone.IsOnFloor())
        {
            drone.stateManager.TransitionToState(new FallDroneState());
        }
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

        if (hasInputDirection(drone))
        {
            drone.stateManager.TransitionToState(WantsRun(drone) ? new RunDroneState() : new WalkDroneState());
            return;
        }
    }
}
