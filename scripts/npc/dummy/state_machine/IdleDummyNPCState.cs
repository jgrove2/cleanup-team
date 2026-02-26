using Godot;

public class IdleDummyNPCState : DummyNPCStateMachine
{
    public override void Enter(DummyNPC npc)
    {
        npc.AnimTree.Set("parameters/Transition/transition_request", "idle");
    }

    public override void PreUpdate(DummyNPC npc)
    {
        if (!npc.Health.IsAlive())
            npc.stateManager.TransitionToState<DeadDummyNPCState>();
    }

    public override void Update(DummyNPC npc, double delta) { }
}
