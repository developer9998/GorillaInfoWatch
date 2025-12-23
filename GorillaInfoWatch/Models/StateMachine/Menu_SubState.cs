using GorillaInfoWatch.Behaviours.UI;

namespace GorillaInfoWatch.Models.StateMachine
{
    public class Menu_SubState(Watch watch, Menu_StateBase previousState) : Menu_StateBase(watch)
    {
        public Menu_StateBase previousState = previousState;
    }
}