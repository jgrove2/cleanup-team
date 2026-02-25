using Godot;

public partial class NPCNavigation : CharacterBody3D
{
    [Export] public float ChaseSpeed { get; set; } = 5.0f;
    [Export] public float TurnSpeed { get; set; } = 8.0f;
    [Export] public float VisionRange { get; set; } = 15.0f;
    [Export] public float VisionAngleDegrees { get; set; } = 90.0f;
    [Export] public bool DebugVision { get; set; } = false;
    [Export] public AnimationTree AnimationTree { get; set; } = null!;

    public NavigationAgent3D NavigationAgent3D { get; private set; } = null!;
    public MovementComponent Movement { get; private set; } = null!;
    public bool PlayerInRange { get; private set; }
    public Vector3? LastKnownPlayerPosition { get; private set; }
    public Vector3? PlayerPosition { get; private set; }
    public StateManager<NPCNavigation> stateManager = null!;

    private Area3D visionArea = null!;
    private Drone? trackedDrone;
    private ImmediateMesh debugMesh = null!;
    private string currentAnimation = "";

    public override void _Ready()
    {
        NavigationAgent3D = GetNode<NavigationAgent3D>("NavigationAgent3D");
        AnimationTree = GetNode<AnimationTree>("AnimationTree");
        Movement = new MovementComponent(this, ChaseSpeed);

        visionArea = GetNode<Area3D>("Detection_Range");
        visionArea.BodyEntered += OnBodyEntered;
        visionArea.BodyExited += OnBodyExited;

        var detectionShape = visionArea.GetNode<CollisionShape3D>("VisionRange");
        if (detectionShape.Shape is SphereShape3D sphere)
            sphere.Radius = VisionRange;

        debugMesh = new ImmediateMesh();
        var debugInstance = new MeshInstance3D();
        debugInstance.Mesh = debugMesh;

        var mat = new StandardMaterial3D();
        mat.ShadingMode = StandardMaterial3D.ShadingModeEnum.Unshaded;
        mat.VertexColorUseAsAlbedo = true;
        mat.NoDepthTest = true;
        debugInstance.MaterialOverride = mat;
        AddChild(debugInstance);

        stateManager = new StateManager<NPCNavigation>(this);
        stateManager.TransitionToState<IdleNPCState>();
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is Drone drone)
            trackedDrone = drone;
    }

    private void OnBodyExited(Node3D body)
    {
        if (body is Drone)
            trackedDrone = null;
    }

    public override void _PhysicsProcess(double delta)
    {
        var spotted = CanSeePlayer();
        if (spotted.HasValue)
        {
            PlayerInRange = true;
            LastKnownPlayerPosition = spotted;
            PlayerPosition = spotted;
        }
        else
        {
            PlayerInRange = false;
            PlayerPosition = null;
        }

        stateManager?.Update(delta);

        debugMesh.ClearSurfaces();
        if (DebugVision)
            DrawVisionDebug();
    }

    /// <summary>
    /// Only runs a cone + raycast check if the Area3D already
    /// has the drone inside it — no shape query every frame.
    /// </summary>
    public Vector3? CanSeePlayer()
    {
        if (trackedDrone == null)
            return null;

        var eyePosition = GlobalPosition + Vector3.Up * 1.5f;
        var forward = Transform.Basis.Z;
        var forwardFlat = new Vector3(forward.X, 0f, forward.Z).Normalized();

        var toPlayer = trackedDrone.GlobalPosition - eyePosition;
        var toPlayerFlat = new Vector3(toPlayer.X, 0f, toPlayer.Z).Normalized();
        var angleDeg = Mathf.RadToDeg(forwardFlat.AngleTo(toPlayerFlat));

        if (angleDeg > VisionAngleDegrees / 2f)
            return null;

        var spaceState = GetWorld3D().DirectSpaceState;
        var rayQuery = PhysicsRayQueryParameters3D.Create(
            eyePosition,
            trackedDrone.GlobalPosition + Vector3.Up * 1.0f
        );
        rayQuery.Exclude = new Godot.Collections.Array<Rid> { GetRid() };
        var result = spaceState.IntersectRay(rayQuery);

        if (result.Count == 0 || result["collider"].As<Node>() is Drone)
            return trackedDrone.GlobalPosition;

        return null;
    }

    /// <summary>
    /// Smoothly rotates the NPC around the Y axis to face <paramref name="worldDirection"/>.
    /// Uses <see cref="TurnSpeed"/> as the lerp rate. No-ops when direction is zero.
    /// </summary>
    public void RotateToward(Vector3 worldDirection, double delta)
    {
        if (new Vector2(worldDirection.X, worldDirection.Z) == Vector2.Zero)
            return;

        float targetAngle = Mathf.Atan2(worldDirection.X, worldDirection.Z);
        float newAngle = Mathf.LerpAngle(Rotation.Y, targetAngle, TurnSpeed * (float)delta);
        Rotation = new Vector3(0f, newAngle, 0f);
    }

    private void DrawVisionDebug()
    {
        float halfAngle = Mathf.DegToRad(VisionAngleDegrees / 2f);
        var coneColor = PlayerInRange ? Colors.OrangeRed : Colors.Yellow;
        var forward = Vector3.Back * VisionRange;
        var eyeOffset = Vector3.Up * 1.5f;

        debugMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

        AddDebugLine(eyeOffset, eyeOffset + forward, coneColor);
        AddDebugLine(eyeOffset, eyeOffset + forward.Rotated(Vector3.Up, halfAngle), coneColor);
        AddDebugLine(eyeOffset, eyeOffset + forward.Rotated(Vector3.Up, -halfAngle), coneColor);

        const int ArcSegments = 24;
        for (int i = 0; i < ArcSegments; i++)
        {
            float a0 = Mathf.Lerp(-halfAngle, halfAngle, (float)i / ArcSegments);
            float a1 = Mathf.Lerp(-halfAngle, halfAngle, (float)(i + 1) / ArcSegments);
            AddDebugLine(
                eyeOffset + forward.Rotated(Vector3.Up, a0),
                eyeOffset + forward.Rotated(Vector3.Up, a1),
                coneColor);
        }

        if (LastKnownPlayerPosition.HasValue)
        {
            var playerLineColor = PlayerInRange ? Colors.LimeGreen : Colors.Red;
            var localTarget = ToLocal(LastKnownPlayerPosition.Value);
            AddDebugLine(eyeOffset, localTarget, playerLineColor);
        }

        debugMesh.SurfaceEnd();
    }

    private void AddDebugLine(Vector3 from, Vector3 to, Color color)
    {
        debugMesh.SurfaceSetColor(color);
        debugMesh.SurfaceAddVertex(from);
        debugMesh.SurfaceSetColor(color);
        debugMesh.SurfaceAddVertex(to);
    }

    /// <summary>
    /// Requests a transition to <paramref name="animationName"/> on the AnimationTree.
    /// No-ops when the animation is already active so the tree is not spammed every frame.
    /// </summary>
    public void SetAnimation(string animationName)
    {
        if (currentAnimation == animationName || AnimationTree == null)
            return;

        currentAnimation = animationName;
        AnimationTree.Set("parameters/animations/transition_request", animationName);
    }

    public void ClearLastKnownPosition() => LastKnownPlayerPosition = null;
}