using System;
using PsigenVision.FiniteStateMachine;

namespace PsigenVision.Controller.Character.FiniteStateMachine
{
    using UnityEngine;

    public class BaseState: IState
    {
        public event Action OnEnter;
        public event Action OnExit;

        public StateLifecycleMask LifecycleRequirements { get; } = StateLifecycleMask.None;
        public virtual void Enter()
        {
            OnEnter?.Invoke();
            //noop
        }

        public virtual void Awake()
        {
            //noop
        }

        public virtual void Start()
        {
            //noop
        }

        public virtual void Update()
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

        public virtual void Exit()
        {
            OnExit?.Invoke();
            //noop
        }

    }
}
