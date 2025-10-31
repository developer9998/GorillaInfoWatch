using System;
using GorillaInfoWatch.Behaviours;

namespace GorillaInfoWatch.Models.StateMachine;

public class Menu_StateBase(InfoWatch watch) : State
{
    protected InfoWatch watch = watch ?? throw new ArgumentNullException(nameof(watch));
    public    InfoWatch Watch => watch;
}