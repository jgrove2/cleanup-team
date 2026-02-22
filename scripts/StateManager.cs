public class StateManager<TActor> where TActor : class
{
    private StateMachineBase<TActor> currentState = null;
    private readonly TActor actor;

    public StateManager(TActor actor)
    {
        this.actor = actor;
    }

    public void TransitionToState(StateMachineBase<TActor> newState)
    {
        currentState?.Exit(actor);
        currentState = newState;
        currentState.Enter(actor);
    }

    public void Update(double delta)
    {
        currentState?.PreUpdate(actor);
        currentState?.Update(actor, delta);
    }
}
