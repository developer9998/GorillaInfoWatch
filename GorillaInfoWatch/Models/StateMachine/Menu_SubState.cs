using GorillaInfoWatch.Behaviours;

namespace GorillaInfoWatch.Models.StateMachine;

public class Menu_SubState(InfoWatch watch, Menu_StateBase previousState) : Menu_StateBase(watch)
{
    public Menu_StateBase previousState = previousState;
}