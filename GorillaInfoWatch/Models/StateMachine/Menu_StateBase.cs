using GorillaInfoWatch.Behaviours;
using System;

namespace GorillaInfoWatch.Models.StateMachine
{
    public class Menu_StateBase(InfoWatch watch) : State
    {
        public InfoWatch Watch => watch;

        protected InfoWatch watch = watch ?? throw new ArgumentNullException(nameof(watch));
    }
}
