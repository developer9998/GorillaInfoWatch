namespace GorillaInfoWatch.Models.StateMachine;

public class StateMachine<T> where T : State
{
    public T CurrentState => currentState;
    public bool HasState => currentState is not null;

    protected T currentState;

    public void SwitchState(T newState)
    {
        if (newState is null)
            return;

        if (HasState)
            currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }

    public void Update()
    {
        if (HasState)
            currentState.Update();
    }
}
