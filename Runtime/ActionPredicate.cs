using System;

namespace PsigenVision.FiniteStateMachine
{
    /// <summary>
    /// Represents a predicate that encapsulates an action and evaluates to true once the action has been invoked.
    /// </summary>
    public class ActionPredicate : IPredicate {
        public bool triggered;

        public ActionPredicate(ref Action eventReaction) => eventReaction += TriggerFlag;
        public bool Evaluate() {
            bool result = triggered;
            triggered = false;
            return result;
        }

        /// <summary>
        /// Sets the internal flag to true, signaling that the predicate's condition has been triggered.
        /// </summary>
        private void TriggerFlag() => triggered = true;
    }
}