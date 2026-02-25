using Godot;
public partial class IdleNPCState : NPCStateMachine
{
    public override void Enter(NPCNavigation npc)
    {
        npc.SetAnimation("idle");
    }
    public override void PreUpdate(NPCNavigation npc)
    {
        if (!npc.IsOnFloor())
        {
            npc.stateManager.TransitionToState<FallNPCState>();
            return;
        }

        if (npc.PlayerInRange)
            npc.stateManager.TransitionToState<ChaseNPCState>();
    }

    public override void Update(NPCNavigation npc, double delta)
    {
        npc.Movement.Update(delta, Vector3.Zero);
    }
}
