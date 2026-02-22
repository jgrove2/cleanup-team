using Godot;

public class CameraComponent
{
    private readonly CharacterBody3D body;
    private readonly Camera3D camera;

    public float Sensitivity { get; set; } = 0.003f;

    /// <summary>
    /// When true, mouse motion is ignored. Set by states that take control away
    /// from the player (e.g. a vaulting animation).
    /// </summary>
    public bool IsLocked { get; set; } = false;

    private float pitch = 0f;
    private static readonly float MaxPitch = Mathf.DegToRad(89f);

    public CameraComponent(CharacterBody3D body, Camera3D camera)
    {
        this.body = body;
        this.camera = camera;
    }

    /// <summary>
    /// Pass every unhandled InputEvent here. Handles mouse motion only when
    /// the mouse is captured and the component is not locked.
    /// </summary>
    public void HandleInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton { Pressed: true })
            Input.MouseMode = Input.MouseModeEnum.Captured;

        if (inputEvent.IsActionPressed("ui_cancel"))
            Input.MouseMode = Input.MouseModeEnum.Visible;

        if (IsLocked || Input.MouseMode != Input.MouseModeEnum.Captured)
            return;

        if (inputEvent is not InputEventMouseMotion mouseMotion)
            return;

        // Yaw: rotate the whole body so "forward" follows horizontal mouse movement.
        // MovementComponent reads Transform.Basis so this automatically affects movement.
        body.RotateY(-mouseMotion.Relative.X * Sensitivity);

        // Pitch: only the camera tilts vertically. Clamped to prevent flipping.
        pitch = Mathf.Clamp(pitch - mouseMotion.Relative.Y * Sensitivity, -MaxPitch, MaxPitch);
        camera.Rotation = new Vector3(pitch, 0f, 0f);
    }
}
