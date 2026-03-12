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
        /// Retrieves the current state of the state machine.
        /// </summary>
        /// <returns>The current state being executed within the state machine, or null if no state is set.</returns>
        public IState CurrentState => current.State;

        private bool isStateNull = false;
        

        /// <summary>
        /// Executes the processing of a state change in the finite state machine.
        /// This method determines if a valid transition is available based on conditions,
        /// and initiates a state change if a transition is found. If no transition is valid,
        /// the current state remains unchanged.
        /// </summary>
        public void ProcessStateChange()
        {
            var transition = GetTransition();
            if (transition != null)
                ChangeState(transition.To);
        }

        /// <summary>
        /// Invokes the Awake method of the current state's implementation if it meets the lifecycle requirements.
        /// This method ensures that the state's Awake logic is triggered only when the current state's
        /// lifecycle mask includes the Awake flag and the state is not null.
        /// </summary>
        public void Awake()
        {
            if (!isStateNull && (current.State.LifecycleRequirements & StateLifecycleMask.Awake) != 0) 
                current.State.Awake();
        }

        /// <summary>
        /// Invokes the Start method of the current state's implementation if it meets the lifecycle requirements.
        /// This method ensures that the state's Start logic is triggered only when the current state's
        /// lifecycle mask includes the Start flag and the state is not null.
        /// </summary>
        public void Start()
        {
            if (!isStateNull && (current.State.LifecycleRequirements & StateLifecycleMask.Start) != 0) 
                current.State.Start();
        }
        
        /// <summary>
        /// Invokes the Update method of the current state's implementation if it meets the lifecycle requirements.
        /// This method ensures that the state's Update logic is triggered only when the current state's
        /// lifecycle mask includes the Update flag and the state is not null.
        /// </summary>
        public void Update()
        {
            if (!isStateNull && (current.State.LifecycleRequirements & StateLifecycleMask.Update) != 0) 
                current.State.Update();
        }

        /// <summary>
        /// Invokes the LateUpdate method of the current state's implementation,
        /// if a state is set and its LifecycleRequirements include LateUpdate.
        /// This method is designed to handle frame-dependent logic that needs
        /// to occur after the Update phase but before rendering.
        /// </summary>
        public void LateUpdate()
        {
            if (!isStateNull && (current.State.LifecycleRequirements & StateLifecycleMask.LateUpdate) != 0) 
                current.State.LateUpdate();
        }

        /// <summary>
        /// Invokes the FixedUpdate method of the current state's implementation if a state is set
        /// and the state's lifecycle requirements include the FixedUpdate flag.
        /// This is used for consistent physics-based updates in line with the Unity physics system.
        /// </summary>
        public void FixedUpdate()
        {
            if (!isStateNull && (current.State.LifecycleRequirements & StateLifecycleMask.FixedUpdate) != 0) 
                current.State.FixedUpdate();
        }

        /// <summary>
        /// Determines whether the specified state is the currently active state in the state machine.
        /// </summary>
        /// <param name="state">The type of the state to check against the current state.</param>
        /// <returns>True if the specified state type matches the current state type; otherwise, false.</returns>
        public bool IsState<T>() where T : IState => StateNode.Is<T>(current);

        /// <summary>
        /// Determines whether the current active state matches either of the two specified state types.
        /// </summary>
        /// <typeparam name="T">The first state type to compare against.</typeparam>
        /// <typeparam name="U">The second state type to compare against.</typeparam>
        /// <returns>True if the current state is of type <typeparamref name="T"/> or <typeparamref name="U"/>; otherwise, false.</returns>
        public bool IsStateEither<T, U>() where T : IState where U : IState => StateNode.IsEither<T, U>(current);

        /// <summary>
        /// Determines if the current state is of type T, U, or V.
        /// This method evaluates the active state against the specified types
        /// and returns true if it matches any of the provided types.
        /// </summary>
        /// <typeparam name="T">The first state type to check.</typeparam>
        /// <typeparam name="U">The second state type to check.</typeparam>
        /// <typeparam name="V">The third state type to check.</typeparam>
        /// <returns>True if the current state matches any of the specified types; otherwise, false.</returns>
        public bool IsStateAny<T, U, V>() where T : IState where U : IState where V : IState =>
            StateNode.IsAny<T, U, V>(current);

        /// <summary>
        /// Sets the specified state as the current state of the state machine.
        /// If the state does not exist in the state machine, it will be added.
        /// Optionally, the state's Enter method can be invoked immediately after setting it as the current state.
        /// </summary>
        /// <param name="state">The state to set as the current state of the state machine.</param>
        /// <param name="enterAfterSetting">Determines whether to invoke the state's Enter method after setting it. Defaults to false.</param>
        public void SetCurrentState(IState state, bool enterAfterSetting = false)
        {
            if (!nodes.TryGetValue(state.GetType(), out current))
                nodes.Add(state.GetType(), current = new StateNode(state));
            if (enterAfterSetting) current.State?.Enter();
            isStateNull = current.State == null; //cache whether the current state has been switched to null
        }

        public void SetStartingState(IState state) => SetCurrentState(state, true);

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
        /// Determines whether the state of the given StateNode is of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of state to check against.</typeparam>
        /// <param name="state">The StateNode containing the state to evaluate. Can be null.</param>
        /// <returns>True if the state within the provided StateNode is of the specified type; otherwise, false.</returns>
        public static bool Is<T>(StateNode state) where T : IState => IState.Is<T>(state.State);

        /// <summary>
        /// Determines whether the state node's current state is of type T or U.
        /// This method evaluates the state encapsulated by the given state node
        /// and checks if it matches either of the two specified state types.
        /// </summary>
        /// <typeparam name="T">The first type of state to check against.</typeparam>
        /// <typeparam name="U">The second type of state to check against.</typeparam>
        /// <param name="state">The state node to evaluate. If null, the method will return false.</param>
        /// <returns>True if the current state of the node is of type T or U; otherwise, false.</returns>
        public static bool IsEither<T, U>(StateNode state) where T : IState where U : IState =>
            IState.IsEither<T, U>(state.State);

        /// <summary>
        /// Determines whether the provided state is of one of the specified types T, U, or V.
        /// </summary>
        /// <typeparam name="T">The first type of state to evaluate.</typeparam>
        /// <typeparam name="U">The second type of state to evaluate.</typeparam>
        /// <typeparam name="V">The third type of state to evaluate.</typeparam>
        /// <param name="state">The state node to evaluate.</param>
        /// <returns>True if the state is of type T, U, or V; otherwise, false.</returns>
        public static bool IsAny<T, U, V>(StateNode state) where T : IState where U : IState where V : IState =>
            IState.IsAny<T, U, V>(state.State);

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
