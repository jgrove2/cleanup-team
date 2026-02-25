public class DeadDummyNPCState : DummyNPCStateMachine
{
    public override void Enter(DummyNPC npc)
    {
        npc.SetCollisionLayerValue(1, false);
        npc.SetCollisionMaskValue(1, false);
    }

    public override void Update(DummyNPC npc, double delta) { }
}
