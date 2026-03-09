using System.Collections.Generic;
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
    }

}