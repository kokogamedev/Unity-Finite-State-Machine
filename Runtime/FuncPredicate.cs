using System;

namespace PsigenVision.FiniteStateMachine
{
    /// <summary>
    /// Represents a predicate that evaluates a condition using a provided <see cref="Func{TResult}"/> delegate to return a boolean value.
    /// </summary>
    public class FuncPredicate: IPredicate
    {
        /// <summary>
        /// A delegate encapsulating a method that returns a boolean value.
        /// Used in the evaluation of conditions in predicates.
        /// </summary>
        private readonly Func<bool> func;

        /// <summary>
        /// Represents a predicate that evaluates a condition based on a user-defined delegate function.
        /// </summary>
        public FuncPredicate(Func<bool> func)
        {
            this.func = func;
        }

        /// <summary>
        /// Evaluates the predicate function and returns a boolean result indicating
        /// whether the condition defined by the predicate is met.
        /// </summary>
        /// <returns>A boolean value representing whether the condition is satisfied.</returns>
        public bool Evaluate() => func.Invoke();
    }
}