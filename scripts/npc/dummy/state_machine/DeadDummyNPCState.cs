using Godot;

public class DeadDummyNPCState : DummyNPCStateMachine
{
    public override void Enter(DummyNPC npc)
    {
        npc.SetCollisionLayerValue(1, false);
        npc.SetCollisionMaskValue(1, false);

        npc.AnimTree.Set("parameters/Transition/transition_request", "death");

        // Disable all hurtbox zones so dead dummies can't receive further hits.
        var hurtbox = npc.GetNodeOrNull("hurtbox");
        if (hurtbox != null)
        {
            foreach (var child in hurtbox.GetChildren())
            {
                if (child is Godot.Area3D zone)
                {
                    zone.CollisionLayer = 0;
                    zone.CollisionMask  = 0;
                }
            }
        }
    }

    public override void Update(DummyNPC npc, double delta) { }
}
