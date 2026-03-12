using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace PsigenVision.FiniteStateMachine
{
    public interface IState
    {
        /// <summary>
        /// Gets the lifecycle requirements for the state as a bitmask, represented by the <see cref="StateLifecycleMask"/> enumeration.
        /// </summary>
        /// <remarks>
        /// The lifecycle requirements define which lifecycle methods the state should implement or activate when managed
        /// within a finite state machine. This property is used to evaluate and determine applicable lifecycle stages such as
        /// Awake, Start, Update, FixedUpdate, and LateUpdate. It allows combining multiple lifecycle stages using bitwise operations
        /// for more complex state behavior configurations.
        /// </remarks>
        /// <returns>A bitmask value of type <see cref="StateLifecycleMask"/> representing the state lifecycle requirements.</returns>
        public StateLifecycleMask LifecycleRequirements { get; }
        public void Enter();
        public void Awake();
        public void Start();
        public void Update();
        void LateUpdate();
        void FixedUpdate();
        public void Exit();
        
        /// <summary>
        /// Determines whether the provided state is of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of state to check against.</typeparam>
        /// <param name="state">The state instance to evaluate.</param>
        /// <returns>True if the state is of the specified type; otherwise, false.</returns>
        public static bool Is<T>(IState state) where T : IState => state is T;

        /// <summary>
        /// Determines whether the provided state is of the specified type T or U.
        /// </summary>
        /// <typeparam name="T">The first type of state to check against.</typeparam>
        /// <typeparam name="U">The second type of state to check against.</typeparam>
        /// <param name="state">The state instance to evaluate.</param>
        /// <returns>True if the state is of type T or U; otherwise, false.</returns>
        public static bool IsEither<T, U>(IState state) where T : IState where U : IState => state is T or U;

        /// <summary>
        /// Determines whether the provided state is of the specified type T, U, or V.
        /// </summary>
        /// <typeparam name="T">The first type of state to check against.</typeparam>
        /// <typeparam name="U">The second type of state to check against.</typeparam>
        /// <typeparam name="V">The third type of state to check against.</typeparam>
        /// <param name="state">The state instance to evaluate.</param>
        /// <returns>True if the state is of type T, U, or V; otherwise, false.</returns>
        public static bool IsAny<T, U, V>(IState state) where T : IState where U : IState where V : IState =>
            state is T or U or V;
        
        public static NullState Null { get; } = new NullState();
    }

    /// <summary>
    /// Represents the lifecycle stages of a state in a finite state machine, expressed as a bitmask.
    /// </summary>
    /// <remarks>
    /// This enum is designed to specify the different lifecycle methods a state can implement.
    /// It supports bitwise operations to combine or evaluate multiple lifecycle stages.
    /// These lifecycle stages include methods such as Awake, Start, Update, FixedUpdate, and LateUpdate,
    /// as well as predefined combinations for convenient grouping like AllUpdates and All.
    /// </remarks>
    [Flags]
    public enum StateLifecycleMask
    {
        None        = 0,
        Awake       = 1 << 0, // 1
        Start       = 1 << 1, // 2
        Update      = 1 << 2, // 4
        FixedUpdate = 1 << 3, // 8
        LateUpdate  = 1 << 4, // 16
    
        // Convenient combinations
        AllUpdates = Update | FixedUpdate | LateUpdate,
        AllNonUpdates = Awake | Start,
        AllNonPhysicsUpdates = Update | LateUpdate, 
        AllNonPhysics = AllNonUpdates | AllNonPhysicsUpdates,
        All = AllUpdates | AllNonUpdates,
    }

    public class NullState : IState
    {
        public StateLifecycleMask LifecycleRequirements { get; }
        public void Enter()
        {
            //noop
        }

        public void Awake()
        {
            //noop
        }

        public void Start()
        {
            //noop
        }

        public void Update()
        {
            //noop
        }

        public void LateUpdate()
        {
            //noop
        }

        public void FixedUpdate()
        {
            //noop
        }

        public void Exit()
        {
            //noop
        }
    }
}