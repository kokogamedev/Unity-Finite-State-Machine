using System.Collections.Generic;
using UnityEngine;

namespace PsigenVision.FiniteStateMachine
{
    /// <summary>
    /// Represents a finite state machine that manages transitions and states.
    /// </summary>
    public class StateMachine
    {
        /// <summary>
        /// Represents the current state node in the state machine.
        /// This holds the current state being executed as well as its associated transitions.
        /// </summary>
        private StateNode current;

        /// <summary>
        /// A dictionary containing mappings from a state's type to its corresponding state node.
        /// Used to manage all states and their transitions within the state machine.
        /// </summary>
        private Dictionary<System.Type, StateNode> nodes = new Dictionary<System.Type, StateNode>();

        /// <summary>
        /// A collection of transitions that can occur at any time, regardless of the current state.
        /// </summary>
        private HashSet<ITransition> anyTransitions = new HashSet<ITransition>();

        /// <summary>
        /// Invokes the OnAwake method of the current state's implementation, if a state is set.
        /// This method is intended to trigger initialization or setup logic of the current state
        /// before any further interactions or transitions.
        /// </summary>
        public void Awake()
        {
            current.State?.OnAwake();
        }

        /// <summary>
        /// Initializes and starts the current state of the state machine.
        /// </summary>
        /// <remarks>
        /// This method triggers the `OnStart` logic defined in the current state's implementation.
        /// It is part of the lifecycle management of the state machine and assumes the current state is already set.
        /// </remarks>
        public void Start()
        {
            current.State?.OnStart();
        }

        /// <summary>
        /// Updates the state machine by checking for valid transitions and executing the current state's update logic.
        /// If a valid transition is found, the state machine transitions to the target state.
        /// </summary>
        /// <remarks>
        /// The method first evaluates any transitions (global transitions), followed by transitions specific to the current state.
        /// If a transition is triggered, the state machine changes to the target state. Finally, the OnUpdate method of the current state is called.
        /// </remarks>
        public void Update()
        {
            var transition = GetTransition();
            if (transition != null)
                ChangeState(transition.To);
            current.State?.OnUpdate();
        }

        /// <summary>
        /// Sets the current state of the state machine. If the state doesn't already exist
        /// in the internal nodes dictionary, it adds the new state as a StateNode.
        /// </summary>
        /// <param name="state">The state to set as the current state of the state machine.</param>
        public void SetCurrentState(IState state, bool enterAfterSetting = false)
        {
            if (!nodes.TryGetValue(state.GetType(), out current))
                nodes.Add(state.GetType(), current = new StateNode(state));
            if (enterAfterSetting) current.State?.Enter();
        }

        public void SetStartingState(IState state) => SetCurrentState(state, true);

        /// <summary>
        /// Retrieves the current state of the state machine.
        /// </summary>
        /// <returns>The current state being executed within the state machine, or null if no state is set.</returns>
        public IState GetCurrentState() => current.State;

        /// <summary>
        /// Changes the current state of the state machine to the specified new state.
        /// Exits the current state, sets the new state as the current state,
        /// and triggers the "Enter" action on the new state.
        /// </summary>
        /// <param name="newState">The state to transition to. Must implement the <see cref="IState"/> interface.</param>
        void ChangeState(IState newState)
        {
            if (newState == current.State) return;
            
            current.State?.Exit();
            SetCurrentState(newState);
            current.State?.Enter();
        }

        /// <summary>
        /// Determines the next transition to be executed from the current state or from any available global transitions.
        /// It first evaluates transitions that are applicable from any state. If no such transition matches,
        /// it checks the transitions owned by the current state.
        /// </summary>
        /// <returns>
        /// The transition to be executed if one meets its condition; otherwise, returns null.
        /// </returns>
        ITransition GetTransition()
        {
            //Iterate over our any transitions first - if we find a transition in there such that it evaluates to ture, return that transition
            foreach (var transition in anyTransitions)
                if (transition.Condition.Evaluate())
                    return transition;
            
            //Otherwise, start looking through all transitions owned by the current state, and likewise return any should it evaluate to true
            foreach (var transition in current.Transitions)
                if (transition.Condition.Evaluate())
                    return transition;
            
            return null;
        }

        /// <summary>
        /// Adds a transition for a specific state, allowing it to transition based on a given condition or behavior.
        /// </summary>
        /// <param name="from">The state from which the transition originates.</param>
        /// <param name="transition">The transition to be added, which includes the destination state and its condition or predicate.</param>
        public void AddTransition(IState from, ITransition transition) => GetOrAddNode(from).AddTransition(transition);

        /// <summary>
        /// Adds a transition to the state machine from the specified source state to the target state
        /// with the specified condition.
        /// </summary>
        /// <param name="from">The source state from which the transition begins.</param>
        /// <param name="to">The target state to which the transition leads.</param>
        /// <param name="condition">The predicate that determines whether the transition should be triggered.</param>
        public void AddTransition(IState from,IState to, IPredicate condition) => GetOrAddNode(from).AddTransition(to, condition);
        
        //REGARDING METHODS BELOW: It is essential that every potentially added state must have a corresponding node as we access functionality through nodes
        /// <summary>
        /// Adds a transition that can occur from any state without specifying a source state.
        /// </summary>
        /// <param name="transition">The transition to be added to the state machine's collection of global transitions.</param>
        public void AddAnyTransition(ITransition transition)
        {
            AddNodeIfAbsent(transition.To);
            anyTransitions.Add(transition);
        }

        /// <summary>
        /// Adds a transition that can occur at any time, regardless of the current state.
        /// </summary>
        /// <param name="to">The state to transition to.</param>
        /// <param name="condition">The condition that must be met for this transition to occur.</param>
        public void AddAnyTransition(IState to, IPredicate condition) => anyTransitions.Add(new Transition(GetOrAddNode(to).State, condition));

        /// <summary>
        /// Retrieves the existing node associated with the specified state or creates a new one if it does not exist.
        /// </summary>
        /// <param name="state">The state for which the corresponding node is required.</param>
        /// <returns>The <see cref="StateNode"/> associated with the specified state.</returns>
        public StateNode GetOrAddNode(IState state)
        {
            if (!nodes.TryGetValue(state.GetType(), out var node))
            {
                node = new StateNode(state);
                nodes.Add(state.GetType(), node);
            }
            return node;
        }

        /// <summary>
        /// Adds a state node to the state machine if it does not already exist.
        /// </summary>
        /// <param name="state">The state to be associated with the node being added.</param>
        private void AddNodeIfAbsent(IState state) {if (!nodes.ContainsKey(state.GetType())) nodes.Add(state.GetType(), new StateNode(state));}
    }

    /// <summary>
    /// Represents a node within a finite state machine that encapsulates a specific state
    /// and manages all transitions originating from this state.
    /// </summary>
    public class StateNode
    {
        /// <summary>
        /// The state represented by this node, from which all transitions originate.
        /// </summary>
        public IState State { get; private set; } //The state represented by this node from which all transitions occur

        /// <summary>
        /// All possible transitions originating from the current state, including their destination states and conditions
        /// </summary>
        public HashSet<ITransition> Transitions { get; } //All possible transitions (which includes destination states and conditions)

        /// <summary>
        /// Adds a transition to the state machine originating from the specified state.
        /// </summary>
        /// <param name="from">The source state from which the transition originates.</param>
        /// <param name="transition">The transition to be added, which includes the destination state and optional condition.</param>
        public void AddTransition(ITransition transition)
        {
            Transitions.Add(transition);
        }

        /// <summary>
        /// Adds a transition to a state in the state machine. Transitions are used to define
        /// the conditions under which the state machine can move from one state to another.
        /// </summary>
        /// <param name="from">
        /// The originating state from which the transition begins.
        /// </param>
        /// <param name="transition">
        /// The transition, which includes the target state and an optional condition.
        /// </param>
        public void AddTransition(IState to, IPredicate condition)
        {
            Transitions.Add(new Transition(to, condition));
        }

        /// <summary>
        /// Represents a node in the state machine that encapsulates a particular state and its associated transitions.
        /// </summary>
        public StateNode(IState state)
        {
            State = state;
            Transitions = new HashSet<ITransition>();
        }
    }
}
