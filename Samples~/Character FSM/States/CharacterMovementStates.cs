namespace PsigenVision.Controller.Character.FiniteStateMachine
{
	public class CharacterMovementStates
    {
        public GroundedState Ground;
        public FallingState Falling;
        public SlidingState Sliding;
        public RisingState Rising;
        public JumpingState Jumping;

        public CharacterMovementStates(ICharacterMovementCallbacks movement)
        {
            Ground = new GroundedState(movement);
            Falling = new FallingState(movement);
            Sliding = new SlidingState(movement);
            Rising = new RisingState(movement);
            Jumping = new JumpingState(movement);
        }
        public class GroundedState : BaseState {
            readonly ICharacterMovementCallbacks movement;

            public GroundedState(ICharacterMovementCallbacks movement) {
                this.movement = movement;
                //Stick with BaseState.LifecycleRequirements = StateLifecycleMask.None as only Enter is used
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
                //Stick with BaseState.LifecycleRequirements = StateLifecycleMask.None as only Enter is used
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
                //Stick with BaseState.LifecycleRequirements = StateLifecycleMask.None as only Enter is used
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
                //Stick with BaseState.LifecycleRequirements = StateLifecycleMask.None as only Enter is used
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
                //Stick with BaseState.LifecycleRequirements = StateLifecycleMask.None as only Enter is used
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