namespace PsigenVision.FiniteStateMachine
{
    using UnityEngine;

    /// <summary>
    /// A predicate is a function that tests a condition and then returns a boolean value
    /// </summary>
    public interface IPredicate
    {
        public bool Evaluate();
    }
}
