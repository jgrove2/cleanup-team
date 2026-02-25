using Godot;

public partial class SearchNPCState : NPCStateMachine
{
    private bool hasBegunNavigation;
    private float searchTimer;
    private const float SearchLookDuration = 3.0f; // seconds to look around
    private bool isLooking;

    public override void Enter(NPCNavigation npc)
    {
        hasBegunNavigation = false;
        isLooking = false;
        searchTimer = 0f;
        GD.Print("Entering Search State");
        if (npc.LastKnownPlayerPosition.HasValue)
            npc.NavigationAgent3D.TargetPosition = npc.LastKnownPlayerPosition.Value;
    }

    public override void PreUpdate(NPCNavigation npc)
    {
        if (!npc.IsOnFloor())
        {
            npc.stateManager.TransitionToState<FallNPCState>();
            return;
        }

        if (npc.PlayerPosition.HasValue)
        {
            npc.stateManager.TransitionToState<ChaseNPCState>();
            return;
        }

        if (!isLooking && npc.LastKnownPlayerPosition.HasValue)
            npc.NavigationAgent3D.TargetPosition = npc.LastKnownPlayerPosition.Value;
    }

    public override void Update(NPCNavigation npc, double delta)
    {
        var direction = Vector3.Zero;

        if (!npc.NavigationAgent3D.IsNavigationFinished())
        {
            hasBegunNavigation = true;
            var destination = npc.NavigationAgent3D.GetNextPathPosition();
            direction = (destination - npc.GlobalPosition).Normalized();
            npc.RotateToward(direction, delta);
            npc.SetAnimation("run");
        }
        else if (hasBegunNavigation)
        {
            // Arrived at last known position — look around before giving up
            isLooking = true;
            npc.SetAnimation("idle");
            searchTimer += (float)delta;

            // Slowly rotate to sweep the vision cone
            float sweepAngle = Mathf.Sin((float)searchTimer * 1.5f) * Mathf.DegToRad(80f);
            var lookDir = Vector3.Back.Rotated(Vector3.Up, sweepAngle);
            npc.RotateToward(npc.GlobalTransform.Basis * lookDir, delta);

            if (searchTimer >= SearchLookDuration)
            {
                npc.ClearLastKnownPosition();
                npc.stateManager.TransitionToState<IdleNPCState>();
            }
        }

        npc.Movement.Update(delta, direction);
    }
}