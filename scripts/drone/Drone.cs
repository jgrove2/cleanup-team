using Godot;
using System;

public partial class Drone : CharacterBody3D
{
	[Export]
	public float WalkSpeed { get; set; } = 3.0f;
	[Export]
	public float RunSpeed { get; set; } = 6.0f;
	[Export]
	public float JumpVelocity { get; set; } = 4.5f;
	[Export]
	public float CrouchScale { get; set; } = 0.6f;
	[Export]
	public float VaultSpeed { get; set; } = 2.0f;
    private float standingShapeHeight;
	private float standingShapePositionY;
	private CapsuleShape3D standingShapeCache;

	public bool IsWalkToggled { get; set; } = false;

	public StateManager<Drone> stateManager;
	public MovementComponent Movement { get; private set; }
	public CameraComponent CameraControl { get; private set; }
	public CollisionShape3D CollisionShape { get; private set; }

	public Vector3 VaultTarget { get; set; }
	public bool VaultShouldCrouch { get; set; }

	public override void _Ready()
	{
		Movement = new MovementComponent(this, WalkSpeed);
		CameraControl = new CameraComponent(this, GetNode<Camera3D>("Camera3D"));
		CollisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
		var originalCapsule = (CapsuleShape3D)CollisionShape.Shape;
		standingShapeHeight = originalCapsule.Height;
		standingShapePositionY = CollisionShape.Position.Y;
		standingShapeCache = new CapsuleShape3D { Height = standingShapeHeight, Radius = originalCapsule.Radius };
		stateManager = new StateManager<Drone>(this);
		stateManager.TransitionToState<IdleDroneState>();
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

	public void Crouch()
	{
		var capsule = (CapsuleShape3D)CollisionShape.Shape;
		capsule.Height = standingShapeHeight * CrouchScale;
		CollisionShape.Position = new Vector3(CollisionShape.Position.X, standingShapePositionY * CrouchScale, CollisionShape.Position.Z);
		Movement.Speed = WalkSpeed * 0.5f;
		CameraControl.SetHeightScale(CrouchScale);
	}

	public void Stand()
	{
		var capsule = (CapsuleShape3D)CollisionShape.Shape;
		capsule.Height = standingShapeHeight;
		CollisionShape.Position = new Vector3(CollisionShape.Position.X, standingShapePositionY, CollisionShape.Position.Z);
		CameraControl.SetHeightScale(1.0f);
	}

	public bool CanStand()
	{
		var query = new PhysicsShapeQueryParameters3D();
		query.Shape = standingShapeCache;
		query.Transform = new Transform3D(Basis.Identity, GlobalPosition + new Vector3(0, standingShapePositionY, 0));
		query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };
		return GetWorld3D().DirectSpaceState.IntersectShape(query, 1).Count == 0;
	}

	public Vector3 getInputDirection() => Movement.ComputeInputDirection();
    public (bool canVault, Vector3 targetPosition, bool shouldCrouch) CheckVault()
    {
        float capsuleTop = GlobalPosition.Y + standingShapePositionY + standingShapeHeight / 2f;
        float capsuleBottom = GlobalPosition.Y + standingShapePositionY - standingShapeHeight / 2f;

        // Cast from ~25% up from the bottom of the capsule
        float rayOriginY = capsuleBottom + standingShapeHeight * 0.25f;
        Vector3 rayStart = new Vector3(GlobalPosition.X, rayOriginY, GlobalPosition.Z);
        Vector3 forward = -Transform.Basis.Z;
        Vector3 rayEnd = rayStart + forward * 1.0f;

        var query = PhysicsRayQueryParameters3D.Create(rayStart, rayEnd);
        query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };
        var result = GetWorld3D().DirectSpaceState.IntersectRay(query);
        if (result.Count == 0) return (false, Vector3.Zero, false);

        Vector3 hitPoint = (Vector3)result["position"];
        float ledgeY = hitPoint.Y;

		// Ledge must be below the top 60% of the capsule, and above the capsule bottom
		float upperBound = capsuleTop - standingShapeHeight * 0.60f;
        float lowerBound = capsuleBottom;

        if (ledgeY > upperBound || ledgeY < lowerBound)
            return (false, Vector3.Zero, false);

		// Cast a ray downward from above the ledge to find the actual top surface Y,
		// since the horizontal ray only hit the side face, not the top.
		float capsuleHalfHeight = standingShapeHeight / 2f;
		Vector3 probeOrigin = new Vector3(hitPoint.X + forward.X * 0.1f, capsuleTop + 0.1f, hitPoint.Z + forward.Z * 0.1f);
		Vector3 probeEnd = new Vector3(probeOrigin.X, capsuleBottom, probeOrigin.Z);
		var downQuery = PhysicsRayQueryParameters3D.Create(probeOrigin, probeEnd);
		downQuery.Exclude = new Godot.Collections.Array<Rid> { GetRid() };
		var downResult = GetWorld3D().DirectSpaceState.IntersectRay(downQuery);
		if (downResult.Count == 0) return (false, Vector3.Zero, false);

		float ledgeSurfaceY = ((Vector3)downResult["position"]).Y;

		// Shape query uses capsule center in world space.
		// Vault target uses drone body origin (GlobalPosition), which is offset below capsule center by standingShapePositionY.
		Vector3 standShapeCenter = new Vector3(hitPoint.X, ledgeSurfaceY + capsuleHalfHeight, hitPoint.Z);
		Vector3 standPos = new Vector3(hitPoint.X, ledgeSurfaceY + capsuleHalfHeight - standingShapePositionY, hitPoint.Z);

		var standQuery = new PhysicsShapeQueryParameters3D();
		standQuery.Shape = standingShapeCache;
		standQuery.Transform = new Transform3D(Basis.Identity, standShapeCenter);
		standQuery.Exclude = new Godot.Collections.Array<Rid> { GetRid() };
		bool canStand =
			GetWorld3D().DirectSpaceState.IntersectShape(standQuery, 1).Count == 0;
		if (canStand)
		{
			GD.Print("Vault available: can stand at ledge.");
			return (true, standPos, false);
		}

		// Check crouch variant
		float crouchHeight = standingShapeHeight * CrouchScale;
		float crouchHalfHeight = crouchHeight / 2f;
		float crouchShapeOffsetY = standingShapePositionY * CrouchScale;
		Vector3 crouchShapeCenter = new Vector3(hitPoint.X, ledgeSurfaceY + crouchHalfHeight, hitPoint.Z);
		Vector3 crouchPos = new Vector3(hitPoint.X, ledgeSurfaceY + crouchHalfHeight - crouchShapeOffsetY, hitPoint.Z);

		var crouchShape = new CapsuleShape3D
		{
			Height = crouchHeight,
			Radius = standingShapeCache.Radius
		};
		var crouchQuery = new PhysicsShapeQueryParameters3D();
		crouchQuery.Shape = crouchShape;
		crouchQuery.Transform = new Transform3D(Basis.Identity, crouchShapeCenter);
		crouchQuery.Exclude = new Godot.Collections.Array<Rid> { GetRid() };
		bool canCrouch =
			GetWorld3D().DirectSpaceState.IntersectShape(crouchQuery, 1).Count == 0;
		if (canCrouch)
		{
			GD.Print("Vault available: can crouch at ledge.");
			return (true, crouchPos, true);
		}

        return (false, Vector3.Zero, false);
    }
}