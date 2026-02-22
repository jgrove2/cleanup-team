using Godot;
using System;

public partial class Drone : CharacterBody3D
{
	public const float WalkSpeed = 3.0f;
	public const float RunSpeed = 6.0f;
	public const float JumpVelocity = 4.5f;

	public bool IsWalkToggled { get; set; } = false;

	public StateManager<Drone> stateManager;
	public MovementComponent Movement { get; private set; }
	public CameraComponent CameraControl { get; private set; }

	public override void _Ready()
	{
		Movement = new MovementComponent(this, WalkSpeed);
		CameraControl = new CameraComponent(this, GetNode<Camera3D>("Camera3D"));
		stateManager = new StateManager<Drone>(this);
		stateManager.TransitionToState(new IdleDroneState());
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		CameraControl.HandleInput(@event);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("walk_toggle"))
			IsWalkToggled = !IsWalkToggled;

		stateManager.Update(delta);
	}

	public Vector3 getInputDirection() => Movement.ComputeInputDirection();
}
