public class StateMachineBase<TActor> where TActor : class
{
    public virtual void Enter(TActor actor) { }
    public virtual void Update(TActor actor, double delta) { }
    public virtual void Exit(TActor actor) { }
    public virtual void PreUpdate(TActor actor) { }
}
