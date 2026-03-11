using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace PsigenVision.FiniteStateMachine
{
    public interface IState
    {
        public void Enter();
        public void OnAwake();
        public void OnStart();
        public void OnUpdate();
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
    }
}