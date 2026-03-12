# Finite State Machine (FSM) for Unity

## Overview
This repository contains a **modular implementation of a Finite State Machine (FSM)** tailored for Unity projects, _inspired by git-amend's state machine tutorial_ which can be found [here](https://youtu.be/NnH6ZK5jt7o). It provides a structured and reusable framework for managing state-based behaviors in areas like AI systems, gameplay mechanics, UI navigation, or player controllers.

---

## Core Features
- **State Management:** Encapsulate logic for individual states using the `IState` interface.
- **Conditional Transitions:** Handle state transitions with dynamic conditions via the `IPredicate` interface and its implementations.
- **Explicit Lifecycle Control:** States use the new `StateLifecycleMask` bitmask to define which lifecycle methods they use (`Awake`, `Start`, `Update`, `FixedUpdate`, `LateUpdate`). Unused methods are skipped during runtime, improving performance.
- **Separation of Logic:** State change logic is now encapsulated in `ProcessStateChange()` for better flexibility and clarity.
- **Flexible Extension:** Supports custom predicates like `FuncPredicate` and `ActionPredicate`, ensuring flexibility in transition behavior.
- **Global Transitions:** Support transitions that are independent of the current state.
- **Lifecycle-Aware States:** Manage state lifecycle with streamlined method names (e.g., `Awake`, `Start`, `Exit`).
- **Event-Driven Transitions:** Use the `ActionPredicate` class to trigger state transitions based on external events (e.g., button clicks, collisions).

---

## How It Works
The FSM consists of the following main components:

### 1. Components:
- **`IState` Interface:** Defines basic lifecycle methods for states and a new `LifecycleRequirements` property for specifying lifecycle method usage.
- **`IPredicate` Interface:** Represents the logic used to evaluate conditions for transitions.
- **`FuncPredicate` Class:** Allows user-defined transition conditions using `Func<bool>`.
- **`ActionPredicate` Class:** Handles event-driven transitions triggered by external `Action`s.
- **`ITransition` Interface:** Represents a state transition (`To` state and `Condition`).
- **`Transition` Class:** A concrete implementation for transitions between states.
- **`StateMachine` Class:** The central controller that manages state activation, transitions, and lifecycle.

### 2. General Workflow:
1. Define all required **states** by implementing the `IState` interface.
2. Define **transitions** by associating target states with conditions (`IPredicate`).
3. Instantiate a `StateMachine` and register states and their transitions.
4. Set the starting state with `SetStartingState()`.
5. Continuously call:
    - **`ProcessStateChange()`** to handle transitions between states.
    - **`Update()`, `FixedUpdate()`, `LateUpdate()`**, or other lifecycle methods as needed by the FSM.

---

## Key Changes in Version 2.0.0
     > **⚠ Important – Breaking Changes**  
     > Starting from version 2.0.0, state changes **must** be invoked via `ProcessStateChange()` instead of relying on `StateMachine.Update()`.

### 1. Refactored StateMachine Logic:
- `StateMachine.Update()` no longer handles state changes. **State changes must now be explicitly invoked via `ProcessStateChange()`**.
- Null checks for `current.State` have been replaced with an efficient local boolean (`isStateNull`) for lifecycle methods.

### 2. Expanded Lifecycle Functionality:
- Added new lifecycle methods to the `IState` interface:
    - `FixedUpdate()`: For physics-related updates (invoked in Unity’s `FixedUpdate`).
    - `LateUpdate()`: For logic that requires late-frame execution.
- Lifecycle method names have been simplified for consistency:
    - `OnAwake` -> `Awake`
    - `OnStart` -> `Start`
    - `OnUpdate` -> `Update`
- States must now explicitly declare the lifecycle methods they use by setting the `LifecycleRequirements` property with a `StateLifecycleMask`.

### 3. State Lifecycle Optimization:
- Introduced the `StateLifecycleMask` enum for fine-grained control over lifecycle methods.
- Lifecycle methods in the FSM now check the **`LifecycleRequirements`** bitmask of the current state before invoking a method. Unnecessary calls (e.g., methods not used by a state) are skipped during runtime, optimizing performance.

---

## Usage Example
Here’s an updated outline of how to use the FSM system in your Unity project:

## State Flow Example (Diagram)
   ```mermaid
   stateDiagram-v2
       [*] --> RunState
       RunState --> JumpState : JumpTrigger
       JumpState --> RunState : LandTrigger
       RunState --> DieState : CollisionTrigger
   ```

1. Define your states, specifying their lifecycle requirements:
```csharp
using PsigenVision.FiniteStateMachine;
using UnityEngine;

public class BaseState: IState
{
    public StateLifecycleMask LifecycleRequirements { get; }
    public event Action OnEnter;
    public event Action OnExit;
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
    
	public virtual void LateUpdate()
    {
        //noop
    }
    
	public virtual void FixedUpdate()
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
        LifeCycleRequirements = StateLifecycleMask.None; //Only the Enter phase is used, no lifecycle methods
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
        LifeCycleRequirements = StateLifecycleMask.None; //Only the Enter phase is used, no lifecycle methods
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
        LifeCycleRequirements = StateLifecycleMask.None; //Only the Enter phase is used, no lifecycle methods
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

2. Set up the `StateMachine`, ensuring `ProcessStateChange()` is invoked:
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
        
    //Run State Machine (Initialize and Process State Change)
    //NOTE: this would also where you call stateMachine.Update(), however no states currently use that lifecycle method
    void Update() => stateMachine.ProcessStateChange();
        
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
## Benefits
- **Improved Performance:** Lifecycle calls are skipped for unused methods, reducing overhead in states with minimal logic.
- **Modular and Scalable:** Easily extendable with new states, predicates, or transitions.
- **Reusability:** Shared logic can be reused across projects with no modifications.

---

## Contributing
Contributions are welcome! Feel free to open issues or submit pull requests if you have ideas for improvements.

---

## License
This FSM framework is licensed under the MIT License. See the [LICENSE](./LICENSE) file for details.

---

## Acknowledgements
This implementation was designed entirely under the guidance of git-amend, whose GitHub can be found at [adammyhre](https://github.com/adammyhre). See his finite state machine tutorial at [https://youtu.be/NnH6ZK5jt7o](https://youtu.be/NnH6ZK5jt7o).