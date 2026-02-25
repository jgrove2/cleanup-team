using Godot;
public partial class ChaseNPCState : NPCStateMachine
{
    public override void Enter(NPCNavigation npc)
    {
        if (npc.LastKnownPlayerPosition.HasValue)
            npc.NavigationAgent3D.TargetPosition = npc.PlayerPosition.GetValueOrDefault(Vector3.Zero);
        npc.SetAnimation("run");
    }

    public override void PreUpdate(NPCNavigation npc)
    {
        if (!npc.IsOnFloor())
        {
            npc.stateManager.TransitionToState<FallNPCState>();
            return;
        }

        if (!npc.PlayerPosition.HasValue)
        {
            npc.stateManager.TransitionToState<SearchNPCState>();
            return;
        }

        npc.NavigationAgent3D.TargetPosition = npc.PlayerPosition.GetValueOrDefault(Vector3.Zero);
    }

    public override void Update(NPCNavigation npc, double delta)
    {
        // Always update target every frame so it tracks the player
        if (npc.PlayerPosition.HasValue)
            npc.NavigationAgent3D.TargetPosition = npc.PlayerPosition.Value;

        var direction = Vector3.Zero;

        // Use a direct vector to player instead of nav agent when close
        var toPlayer = npc.PlayerPosition.HasValue
            ? npc.PlayerPosition.Value - npc.GlobalPosition
            : Vector3.Zero;

        if (toPlayer.Length() > 1.0f) // Stop threshold — tune this
        {
            if (!npc.NavigationAgent3D.IsNavigationFinished())
            {
                var destination = npc.NavigationAgent3D.GetNextPathPosition();
                direction = (destination - npc.GlobalPosition).Normalized();
            }
            else
            {
                // Nav says finished but player is still far — drive directly
                direction = toPlayer.Normalized();
            }
        }

        npc.RotateToward(direction, delta);
        npc.Movement.Update(delta, direction);
    }
}
