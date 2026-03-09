namespace PsigenVision.Controller.Character.FiniteStateMachine
{
    public class CharacterStates
    {
        public class GroundedState : BaseState {
            readonly ICharacterMovementCallbacks movement;

            public GroundedState(ICharacterMovementCallbacks movement) {
                this.movement = movement;
            }

            public override void Enter() 
            {
                base.Enter();
                movement.OnGroundContactRegained();
            }
        }

        public class FallingState : BaseState {
            readonly ICharacterMovementCallbacks movement;

            public FallingState(ICharacterMovementCallbacks movement) {
                this.movement = movement;
            }

            public override void Enter() 
            {
                base.Enter();
                movement.OnFallStart();
            }
        }

        public class SlidingState : BaseState {
            readonly ICharacterMovementCallbacks movement;

            public SlidingState(ICharacterMovementCallbacks movement) {
                this.movement = movement;
            }

            public override void Enter() 
            {
                base.Enter();
                movement.OnGroundContactLost();
            }
        }

        public class RisingState : BaseState {
            readonly ICharacterMovementCallbacks movement;

            public RisingState(ICharacterMovementCallbacks movement) {
                this.movement = movement;
            }

            public override void Enter() 
            {
                base.Enter();
                movement.OnGroundContactLost();
            }
        }

        public class JumpingState : BaseState {
            readonly ICharacterMovementCallbacks movement;

            public JumpingState(ICharacterMovementCallbacks movement) {
                this.movement = movement;
            }

            public override void Enter() 
            {
                base.Enter();
                movement.OnGroundContactLost();
                movement.OnJumpStart();
            }
        }
    }
}