using GorillaInfoWatch.Behaviours.UI;
using System;

namespace GorillaInfoWatch.Models.StateMachine
{
    public class Menu_StateBase(Watch watch) : State
    {
        public Watch Watch => watch;

        protected Watch watch = watch ?? throw new ArgumentNullException(nameof(watch));
    }
}
