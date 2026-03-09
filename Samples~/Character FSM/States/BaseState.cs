using System;
using PsigenVision.FiniteStateMachine;

namespace PsigenVision.Controller.Character.FiniteStateMachine
{
    using UnityEngine;

    public class BaseState: IState
    {
        public event Action OnEnter;
        public event Action OnExit;
        public virtual void Enter()
        {
            OnEnter?.Invoke();
            //noop
        }

        public virtual void OnAwake()
        {
            //noop
        }

        public virtual void OnStart()
        {
            //noop
        }

        public virtual void OnUpdate()
        {
            //noop
        }

        public virtual void Exit()
        {
            OnExit?.Invoke();
            //noop
        }
    }
}
