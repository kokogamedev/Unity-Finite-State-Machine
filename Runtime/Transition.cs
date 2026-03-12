namespace PsigenVision.FiniteStateMachine
{
    public class Transition: ITransition
    {
        public IState To { get; }
        public IPredicate Condition { get; }

        public Transition(IState to, IPredicate condition)
        {
            //Implement Null Object Pattern - if IState is null, change it to the Null state object
            To = to;
            Condition = condition;
        }
    }
}