using System;
using System.Collections.Generic;

public class StateManager<TActor> where TActor : class
{
    private StateMachineBase<TActor> currentState = null;
    private readonly TActor actor;
    private readonly Dictionary<Type, StateMachineBase<TActor>> stateCache = new();

    public StateManager(TActor actor)
    {
        this.actor = actor;
    }

    public void TransitionToState<TState>() where TState : StateMachineBase<TActor>, new()
    {
        Type key = typeof(TState);
        if (!stateCache.TryGetValue(key, out StateMachineBase<TActor> next))
        {
            next = new TState();
            stateCache[key] = next;
        }

        currentState?.Exit(actor);
        currentState = next;
        currentState.Enter(actor);
    }

    public void Update(double delta)
    {
        currentState?.PreUpdate(actor);
        currentState?.Update(actor, delta);
    }
}
