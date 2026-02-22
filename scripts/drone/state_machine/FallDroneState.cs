using Godot;
using System;

public partial class FallDroneState : DroneStateMachine
{
	public override void Enter(Drone drone)
	{
		GD.Print("Drone has entered the fall state.");
	}
    public override void PreUpdate(Drone drone)
    {
        if (drone.IsOnFloor())
        {
            drone.stateManager.TransitionToState(new IdleDroneState());
        }
    }
	public override void Update(Drone drone, double delta)
	{
		drone.Movement.Update(delta);
	}
}
