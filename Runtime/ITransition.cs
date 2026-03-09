namespace PsigenVision.FiniteStateMachine
{
    using UnityEngine;

    public interface ITransition
    {
        IState To { get; }
        IPredicate Condition { get; }
    }
}
