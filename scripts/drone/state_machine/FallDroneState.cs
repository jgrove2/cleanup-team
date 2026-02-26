using Godot;
using System;

public partial class FallDroneState : DroneStateMachine
{
	public override void Enter(Drone drone)
	{
		SetMovement(drone, "idle");
		SetAttackState(drone, "idle");
		GD.Print("Drone has entered the fall state.");
	}
    public override void PreUpdate(Drone drone)
    {
        base.PreUpdate(drone);

        if (drone.IsOnFloor())
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
