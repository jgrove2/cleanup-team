using Godot;
public partial class FallNPCState : NPCStateMachine
{
    public override void Enter(NPCNavigation npc)
    {
        npc.SetAnimation("idle");
    }

    public override void PreUpdate(NPCNavigation npc)
    {
        if (!npc.IsOnFloor())
            return;

        if (npc.PlayerPosition.HasValue)
            npc.stateManager.TransitionToState<ChaseNPCState>();
        else if (npc.LastKnownPlayerPosition.HasValue)
            npc.stateManager.TransitionToState<SearchNPCState>();
        else
            npc.stateManager.TransitionToState<IdleNPCState>();
    }

    public override void Update(NPCNavigation npc, double delta)
    {
        npc.Movement.Update(delta, Vector3.Zero);
    }
}
