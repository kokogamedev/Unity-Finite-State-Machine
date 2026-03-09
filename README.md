# Finite State Machine (FSM) for Unity

## Overview
This repository contains a modular implementation of a Finite State Machine (FSM) tailored for Unity projects _based on git-amend's own state machine implementation which can be created by following the tutorial located at https://youtu.be/NnH6ZK5jt7o_. It provides a structured and reusable framework for managing state-based behaviors, making it ideal for use cases like AI systems, gameplay mechanics, UI navigation, and player controllers.

---

## Core Features
- **State Management:** Encapsulate logic for individual states using the `IState` interface.
- **Conditional Transitions:** Handle state transitions with dynamic conditions via the `IPredicate` interface and its implementations.
- **Flexible Extension:** Custom transition conditions using the `FuncPredicate` class.
- **Global Transitions:** Support transitions that are independent of the current state.
- **Lifecycle Awareness:** Manage state lifecycle methods (`Enter`, `OnAwake`, `OnStart`, `OnUpdate`, `Exit`).

---

## How It Works
The FSM consists of the following main components:

### 1. Components:
- **`IState` Interface:** Defines basic lifecycle methods for states (e.g., `Enter()`, `Exit()`).
- **`IPredicate` Interface:** Represents the logic used to evaluate conditions for transitions.
- **`FuncPredicate` Class:** Allows custom predicates with user-defined conditions using `Func<bool>`.
- **`ITransition` Interface:** Represents a state transition (`To` state and `Condition`).
- **`Transition` Class:** Concrete implementation of transitions between states.
- **`StateMachine` Class:** The central controller that manages state activation, transitions, and lifecycle.

### 2. General Workflow:
1. Define all required **states** by implementing the `IState` interface.
2. Define **transitions** by associating target states with conditions (`IPredicate`).
3. Instantiate a `StateMachine` and register states and their transitions.
4. Set the starting state with `SetStartingState()`.
5. Continuously call `Update()` (e.g., in Unity’s `MonoBehaviour.Update` method) to evaluate transitions and execute the state logic.

---

## Benefits
- **Modular and Scalable:** Easily extendable with new states, predicates, or transitions.
- **Reusable Logic:** Write states and transition conditions once and reuse them across FSMs and projects.
- **Lightweight Integration:** Can be dropped into Unity projects seamlessly and used to control systems like AI or UI.

---

## Usage Example
Here’s an outline of how to use the FSM system in your Unity project:

1. Define your states:
```csharp
using PsigenVision.FiniteStateMachine;
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

public class JumpState: BaseState
{
    private IAnimate animator;
    private IApplyForce forceApplier;
    public JumpState(IAnimate animator, IApplyForce forceApplier): base()
    {
        this.animator = animator;
        this.forceApplier = forceApplier;
    }
    public override void Enter()
    {
        base.Enter();
        forceApplier.Jump();
        animator.SetTrigger(CharacterAnimationConfig.jumpTriggerHash);
    }
}

public class DieState: BaseState
{
    private IAnimate animator;
    private IPlayFX fxPlayer;
    public DieState(IAnimate animator, IPlayFX fxPlayer): base()
    {
        this.animator = animator;
        this.fxPlayer = fxPlayer;
    }
    public override void Enter()
    {
        base.Enter();
        //Communicate game over state to game manager
        GameStateManager.Instance.TriggerGameOver();
        //Handle animations related to entering the death state 
        animator.SetInt(CharacterAnimationConfig.deathTypeHash, 1); //death type 1 is the fall-backward death, which is most appropriate
        animator.SetBool(CharacterAnimationConfig.deathHash, true);
        //Handle FX (vfx and sfx) related to entering the death state
        fxPlayer.PlayDeathVFX();
    }

    public override void Exit()
    {
        base.Exit();
        //ensure the die animation is exited in the case that dying somehow transitions to something else
        animator.SetBool(CharacterAnimationConfig.deathHash, false);
        //ensure the die dying fx are stopped in the case that dying somehow transitions to something else
        fxPlayer.StopDeathVFX();
    }
}
    
public class RunState: BaseState
{
    IAnimate animator;
    private IManageMotion mover;
    private IPlayFX fxPlayer;
    private float startSpeed;
    public RunState(IAnimate animator, IManageMotion mover, IPlayFX fxPlayer): base()
    {
        this.animator = animator;
        this.mover = mover;
        this.fxPlayer = fxPlayer;
    }
    public override void Enter()
    {
        base.Enter();
        //Set all necessary parameters for animation
        animator.SetBool(CharacterAnimationConfig.staticHash, true);
        animator.SetFloat(CharacterAnimationConfig.speedHash, mover.Speed);
        startSpeed = animator.GetSpeed();
        animator.SetSpeed(mover.Speed);
        
        //Activate/deactivate necessary FX for vfx and sfx
        fxPlayer.PlayRunVFX();
    }

    public override void Exit()
    {
        base.Exit();
        //Reset animator to original pre-running speed
        animator.SetSpeed(startSpeed);
        //Stop playing any running vfx or sfx
        fxPlayer.StopRunVFX();
    }
}
    
```

2. Set up the `StateMachine`:
```csharp
using PsigenVision.FiniteStateMachine;
using UnityEngine;

public class CharacterFSM : MonoBehaviour
{
    private StateMachine stateMachine;
    private States states;
    protected struct States
    {
        public DieState Die;
        public JumpState Jump;
        public RunState Run;

        public States(DieState die, JumpState jump, RunState run)
        {
            Die = die;
            Jump = jump;
            Run = run;
        }
    }
    
    public CharacterInput input; //The input keybindings for the character (e.g. jump)
	private bool isOnGround = true;
    private bool isDead = false;
   
    
    //Initialize State Machine (externally)
    public void InitializeStateMachine(IAnimate animator, IPlayFX fxPlayer)
    {
        //Create State Machine
        stateMachine = new();
        //Create States
        states = new(
            new DieState(animator, fxPlayer), 
            new JumpState(animator, this), 
            new RunState(animator, this, fxPlayer));
        //Define Transitions
        stateMachine.AddTransition(states.Run, states.Jump, new FuncPredicate(TransitionRunToJump));
        stateMachine.AddTransition(states.Jump, states.Run, new FuncPredicate(TransitionJumpToRun));
        stateMachine.AddAnyTransition(states.Die, new FuncPredicate(TransitionToDie));
        
        //Set current state to running by default
        stateMachine.SetStartingState(states.Run);
    }
        
    //Run State Machine (Initialize and Update)
    void Update() => stateMachine.Update();
        
    #region State Machine Transition Methods
    //Define State Machine Transition Predicates
    
    private bool TransitionJumpToRun() => isOnGround;
    private bool TransitionRunToJump() => isOnGround && input.JumpOnKeyDown();
    private bool TransitionToDie() => isDead; //This is handled by OnCollisionEnter and requires no further checks
    #endregion

    #region Movement Methods
    /// <summary>
    /// Add jump impulse-force to character's rigidbody according to some jumpForce value 
    /// (this value is not physically-based - 2f is a benchmark force for a 1kg player)
    /// </summary>
    /// <param name="jumpForce"></param>
    public void Jump(float jumpForce)
    {
        rBody.AddForce(movementConfig.GetJumpForce(jumpForce) * Vector3.up, ForceMode.Impulse);
        isOnGround = false;
    }
    /// <summary>
    /// Add jump-impulse force ot character's rigidbody according to the character's current jump force
    /// </summary>
    public void Jump()
    {
        rBody.AddForce(movementConfig.JumpForce * Vector3.up, ForceMode.Impulse);
        isOnGround = false;
    }

    #endregion
    
    #region Collision Detection
    
    void OnCollisionEnter(Collision collision)
    {
        //Once character hits the ground, make the character enter a running state
        if (collision.gameObject.CompareTag(GameStateManager.GameTags.Ground)) isOnGround = true;
        //Once character hits an obstacle, make the character enter a death state
        else if (collision.gameObject.CompareTag(GameStateManager.GameTags.Obstacle))
        {
            OnCollision?.Invoke();
            isDead = true;
        }
    }
    
    #endregion
}
```

--- 

## Contributing
Contributions are welcome! Feel free to open issues or submit pull requests if you have ideas for improvements.

---

## License
This FSM framework is licensed under the MIT License. See the [LICENSE](./LICENSE) file for details.

---

## Acknowledgements
This implementation was designed entirely under the guidance of git-amend whose github can be found under the username adammyhre at the link https://github.com/adammyhre. See his finite state machine tutorial at https://youtu.be/NnH6ZK5jt7o.