using Godot;

public class MovementComponent
{
    private readonly CharacterBody3D body;

    public float Speed { get; set; }

    /// <summary>The input direction computed during the last Update call.</summary>
    public Vector3 InputDirection { get; private set; }

    /// <summary>Magnitude of horizontal velocity after the last Update call.</summary>
    public float HorizontalSpeed => new Vector2(body.Velocity.X, body.Velocity.Z).Length();

    /// <summary>True when the last Update had a non-zero input direction.</summary>
    public bool IsMoving => InputDirection != Vector3.Zero;

    public MovementComponent(CharacterBody3D body, float speed)
    {
        this.body = body;
        Speed = speed;
    }

    /// <summary>
    /// Samples the input axes and returns the direction in world space relative
    /// to the body's current transform. Safe to call outside of Update.
    /// </summary>
    public Vector3 ComputeInputDirection()
    {
        Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
        return (body.Transform.Basis * new Vector3(input.X, 0, input.Y)).Normalized();
    }

    /// <summary>
    /// Accumulates gravity, applies horizontal movement from player input, then calls MoveAndSlide on the body.
    /// </summary>
    public void Update(double delta) => Update(delta, ComputeInputDirection());

    /// <summary>
    /// Accumulates gravity, applies horizontal movement from the given direction, then calls MoveAndSlide on the body.
    /// Pass <see cref="Vector3.Zero"/> to apply only gravity with no horizontal movement.
    /// </summary>
    public void Update(double delta, Vector3 direction)
    {
        InputDirection = direction;

        Vector3 velocity = body.Velocity;

        if (!body.IsOnFloor())
            velocity += body.GetGravity() * (float)delta;

        velocity.X = InputDirection.X * Speed;
        velocity.Z = InputDirection.Z * Speed;
        body.Velocity = velocity;

        body.MoveAndSlide();
    }
}
