# Overview of the Finite State Machine (FSM) System

The provided FSM system is a modular implementation designed for handling state-based logic in a structured and reusable manner. It enables developers to define states, transitions, and conditions for state changes (predicates), making it ideal for managing complex systems such as AI behavior, game mechanics, or UI navigation in Unity.

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

### Key Components and Their Responsibilities

1. **The `IState` Interface**
	- Represents an individual state in the machine. 
    - Defines basic lifecycle methods for states and a new `LifecycleRequirements` property for specifying lifecycle method usage.
	- Defines the lifecycle methods for a state:
		- `Enter()`: Triggered when the state is entered.
		- `Awake()`: Called during the initialization phase.
		- `Start()`: Called when a state starts execution.
		- `Update()`: Continuously executed during the state’s lifetime (usually per frame in the Update unity lifecycle method via the StateMachine.Update method).
        - `LateUpdate()`: Continuously executed during the state’s lifetime (usually per frame in the LateUpdate unity lifecycle method via the StateMachine.LateUpdate method).
        - `FixedUpdate()`: Continuously executed during the state’s lifetime (usually per physics frame in the FixedUpdate unity lifecycle method via the StateMachine.FixedUpdate method).
		- `Exit()`: Called when the current state is exited.

2. **The `IPredicate` Interface**
	- Represents a condition that evaluates to either `true` or `false`.
	- Acts as the logic for determining whether a transition between states should occur.

3. **The `FuncPredicate` Class**
	- Concrete implementation of `IPredicate`.
	- Encapsulates a condition using a `Func<bool>` delegate, allowing for flexible, user-defined logic to determine conditions for transitions.

4. **The `ITransition` Interface**
	- Represents a transition between states.
	- Consists of:
		- `To`: The target state to transition to.
		- `Condition`: A predicate (`IPredicate`) that determines if the transition should occur.

5. **The `Transition` Class**
	- A concrete implementation of `ITransition`.
	- Links a specific destination state (`To`) with a condition (`Condition`) for transitioning.

6. **The `StateMachine` Class**
	- The central component of the FSM system.
	- Manages states, transitions, and ensures the correct sequence of state lifecycle methods.

   #### Key Responsibilities:
	- Holds the current state **and** manages transitions via StateMachine.ProcessStateChange()
      - What Changed: State transition logic moved out of StateMachine.Update() into a new method, ProcessStateChange(), which decouples update logic from transition logic.
	- Allows for **global transitions** (`anyTransitions`), which are valid regardless of the current state.
	- State lifecycle management:
    - 
		- Calls `Awake()`, `Start()`, `Update()`, `LateUpdate()`, and `FixedUpdate()` methods of the current state given that that state (IState) specifies that it uses that lifecycle method (via bitmask `LifecycleRequirements` )
	- Provides methods for setting and retrieving the current state:
		- `SetCurrentState(IState state)`
		- `SetStartingState(IState state)`
		- `GetCurrentState()`
	- Handles transitions:
		- Uses `GetTransition()` to find a valid transition (evaluates predicates).
		- Calls `ChangeState()` to switch states when a valid condition is met.
        - Requires call to `StateMachine.ProcessStateChange()` to call any of the transition methods in the correct manner to initiate state changes

7. **General Workflow**
	- States are encapsulated as `IState` implementations that specify their own usage of Unity lifecycle methods 
	- Transitions between states depend on `Transitions` and their associated `Predicates`.
	- The `StateMachine` orchestrates the overall process, evaluating predicates, executing transition logic, and calling appropriate lifecycle methods for the active state given that state uses those lifecycle methods.

---

### Example Workflow

1. **Initialization**
	- States and transitions are defined.
	- The starting state is set using `SetStartingState()`.

2. **State Execution**
	- During whichever loop the user selects (`Update()`, `FixedUpdate`, `LateUpdate`, etc), the FSM (via a call to `ProcessStateChange()`):
		- Evaluates all global transitions as well as transitions specific to the current state.
		- Switches to a new state if a transition condition evaluates to `true`.

3. **State Switching**
	- When a transition occurs:
		- The current state’s `Exit()` method is called.
		- The target state is set as the active state.
		- The target state’s `Enter()` method is called.

4. **State Lifecycle**
	- The lifecycle of a state is managed by the FSM through its methods _based on each states internally specified LifecycleRequirements_ (meaning which lifecycle methods it actually uses):
		- `Awake()` (initialization)
		- `Start()` (beginning logic)
		- `Update()` (continuous logic)
        - `LateUpdate()` (continuous logic)
        - `FixedUpdate()` (continuous physics logic)

---

### Benefits of This FSM Implementation

- **Scalable and Modular**: The separation of responsibilities into interfaces and concrete implementations allows for easy extension and integration.
- **Dynamic Transitions**: Transition conditions can be defined flexibly using `FuncPredicate`, making the system versatile for various use cases.
- **Ease of Integration**: The system can be integrated into Unity projects directly and used for gameplay-related tasks such as AI behaviors and reactive systems.
- **Reusable Logic**: States and transitions are reusable, enabling developers to define them once and use them across multiple FSMs.
- **Improved Performance:** Lifecycle calls are skipped for unused methods, reducing overhead in states with minimal logic.

---

## Component Breakdown

Here’s a detailed breakdown of each component in isolation to help understand its role and how it contributes to the FSM system.

---

### 1. **`IState` Interface**

#### Purpose:
The `IState` interface defines a contract for all states in the FSM. It ensures that every state implements the necessary lifecycle methods, enabling consistent behavior and integration with the FSM's workflow.

#### Key Methods:
1. **`Enter()`**
	- Called when the FSM transitions INTO this state.
	- Used for initialization specific to the state.

2. **`Exit()`**
	- Called when the FSM transitions OUT of this state.
	- Used for cleanup or resource handling.

3. **`Awake()`**
	- An optional initialization step, usually invoked when the state is first created or set.
	- For setup or dependencies if needed.

4. **`Start()`**
	- Invoked when the state begins execution.
	- Ideal for logic that should run once when the state becomes active.

5. **`Update()`**
	- Continuously executed while the FSM resides in this state (commonly called once per frame).
	- Used for state-specific updates, e.g., AI logic or handling input.

6. **`FixedUpdate()`**
	- Invoked at a fixed time interval, making it ideal for physics calculations or other needs tied to Unity's physics system.

7. **`LateUpdate()`**
	- Called at the end of the frame after `Update()`.
	- Perfect for post-rendering logic, setting camera positions, or final adjustments.

#### Property:
- **`LifecycleRequirements`**
	- A `StateLifecycleMask` identifying which lifecycle methods the state requires.
	- Prevents unused methods from being redundantly invoked, improving FSM efficiency.

#### Static Methods:
- **`Is<T>(IState state)`**  
  Description: Checks if the provided state is of the specified type `T`.

- **`IsEither<T, U>(IState state)`**  
  Description: Checks if the state is of type `T` or `U`.

- **`IsAny<T, U, V>(IState state)`**  
  Description: Determines whether the state matches any of three types (`T`, `U`, `V`).


---

### 2. **`IPredicate` Interface**

#### Purpose:
The `IPredicate` interface abstracts conditional logic for state transitions by enforcing a single method, `Evaluate()`.

#### Key Method:
- **`Evaluate()`**
	- Returns a `bool` indicating if the condition is met.
	- Used as the foundation of state transitions to determine whether to switch to another state.

#### Use Cases:
- Implementations can capture dynamic conditions like:
	- If a timer has expired.
	- If a player's health is below a threshold.
	- If a key input was detected.

---

### 3. **`FuncPredicate` Class**

#### Purpose:
A concrete implementation of `IPredicate` that uses a delegate (`Func<bool>`) to evaluate conditions. This class offers flexibility, allowing developers to pass any custom logic for transition conditions at runtime.

#### Key Members:
- **`Func<bool>`**
	- The core delegate used to encapsulate condition logic.

- **`Evaluate()`**
	- Invokes the encapsulated delegate and returns the result.

#### Benefits:
- Reduces the need for complex predicate subclasses.
- Allows for dynamic, runtime-defined transition conditions.

**Example:**
```csharp
var transitionWhenReady = new FuncPredicate(() => character.IsReady);
```

---

### 4. `ActionPredicate` Class

#### Overview
The **`ActionPredicate`** is a new implementation of the `IPredicate` interface, designed to respond to events or external triggers. Unlike `FuncPredicate`, which evaluates a function to determine its truth value, `ActionPredicate` listens for an event and evaluates to `true` once the event has been triggered. This makes it particularly useful for handling event-driven transitions in the FSM.

---

#### Implementation

```csharp
using System;

namespace PsigenVision.FiniteStateMachine
{
    /// <summary>
    /// Represents a predicate that encapsulates an action and evaluates to true once the action has been invoked.
    /// </summary>
    public class ActionPredicate : IPredicate {
        public bool triggered;

        public ActionPredicate(ref Action eventReaction) => eventReaction += TriggerFlag;
        public bool Evaluate() {
            bool result = triggered;
            triggered = false;
            return result;
        }

        /// <summary>
        /// Sets the internal flag to true, signaling that the predicate's condition has been triggered.
        /// </summary>
        private void TriggerFlag() => triggered = true;
    }
}
```

---

#### Key Details
1. **Purpose:**
	- The `ActionPredicate` listens for an external trigger and evaluates to `true` when triggered.

2. **Key Properties:**
	- `triggered`: A flag that determines whether the predicate has been triggered.

3. **Key Methods:**
	- **`Evaluate()`**: Checks the value of `triggered`, resets it to `false`, and returns its value.
	- **`TriggerFlag()`**: An internal method that sets the `triggered` flag to `true`.

4. **Constructor:**
	- Takes a reference to an `Action` parameter and attaches the internal `TriggerFlag` method to it. This allows the predicate to react to external events.

---

#### Usage Example

Here’s an example of how to use `ActionPredicate` for event-based transitions:

```csharp
// Example: A character reacts to an external event (e.g., power-up pickup).

using UnityEngine;
using PsigenVision.FiniteStateMachine;

public class CharacterFSM : MonoBehaviour
{
    private StateMachine stateMachine;
    private States states;

    protected struct States
    {
        public IdleState Idle;
        public PoweredUpState PoweredUp;

        public States(IdleState idle, PoweredUpState poweredUp)
        {
            Idle = idle;
            PoweredUp = poweredUp;
        }
    }

    private Action onPowerUpCollected;
    private ActionPredicate powerUpPredicate;

    private void Start()
    {
        // Initialize FSM and predicates
        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        stateMachine = new StateMachine();

        // Initialize ActionPredicate with the external trigger
        powerUpPredicate = new ActionPredicate(ref onPowerUpCollected);

        // Define states
        states = new States(new IdleState(), new PoweredUpState());

        // Add a transition between states based on the ActionPredicate
        stateMachine.AddTransition(states.Idle, states.PoweredUp, powerUpPredicate);

        // Set the starting state
        stateMachine.SetStartingState(states.Idle);
    }

    private void Update()
    {
        stateMachine.Update();
    }

    private void OnPowerUpPickup()
    {
        // Simulate an external event triggering the predicate
        onPowerUpCollected?.Invoke();
    }
}

// Example States:
public class IdleState : IState
{
    public void Enter() { Debug.Log("Entered Idle State."); }
    public void OnAwake() { }
    public void OnStart() { }
    public void OnUpdate() { }
    public void Exit() { Debug.Log("Exiting Idle State."); }
}

public class PoweredUpState : IState
{
    public void Enter() { Debug.Log("Powered up!"); }
    public void OnAwake() { }
    public void OnStart() { }
    public void OnUpdate() { }
    public void Exit() { Debug.Log("Power-up ended."); }
}
```

---

#### Benefits of `ActionPredicate`
- **Event-Driven Transitions:** Ideal for scenarios where state transitions rely on isolated external events.
- **Reusable Logic:** Can be used with any `Action`-based event system, enabling flexibility in connections.
- **Seamless Integration:** Works cohesively with existing `IPredicate`-based transitions in the FSM framework.

---

#### IPredicate Update

##### Purpose
The `ActionPredicate` class is an implementation of the `IPredicate` interface that listens for an external trigger or event and evaluates to `true` when the event occurs. It is designed for event-driven FSM transitions.

##### Key Members
1. **`triggered`**
	- A `bool` flag that determines if the predicate has been triggered.

2. **Constructor**
	- Accepts a reference to an `Action` and attaches an internal trigger method.

3. **`Evaluate()`**
	- Checks the value of `triggered` and resets it to `false`.

4. **`TriggerFlag()`**
	- Sets the `triggered` flag to `true`. This is automatically invoked when the attached `Action` is triggered.

---

##### Example Usage

```csharp
Action onEventOccurrence;
ActionPredicate predicate = new ActionPredicate(ref onEventOccurrence);
onEventOccurrence?.Invoke(); // Triggers the predicate

bool result = predicate.Evaluate(); // Returns true
```

---

##### Benefits
- **Flexibility:** Enables event-based state transitions.
- **Ease of Use:** Directly integrates with existing `Action` mechanisms.
- **Reusability:** The same predicate can be reused across different states for modular design.

---

### 5. **`ITransition` Interface**

#### Purpose:
The `ITransition` interface defines a contract for transitions between states. It associates a destination state with a condition.

#### Key Properties:
1. **`To`**
	- The target state to which the FSM will transition.

2. **`Condition`**
	- An instance of `IPredicate` that must evaluate to `true` for the transition to occur.

---

### 6. **`Transition` Class**

#### Purpose:
A concrete implementation of `ITransition`. It binds a specific destination state to a condition, providing a reusable building block for state transitions.

#### Key Members:
- **`IState To`**
	- Represents the target state of the transition.

- **`IPredicate Condition`**
	- Encapsulates the conditional logic required to execute the transition.

**Example:**
```csharp
var transition = new Transition(nextState, new FuncPredicate(() => health <= 0));
```

---

### 7. **`StateNode` Class**

#### Purpose:
A wrapper for managing an individual state (`IState`) and its associated transitions in the FSM. Provides utility for adding transitions and evaluating the type of the encapsulated state.

#### Key Members:
1. **`State`**
	- The `IState` encapsulated by this node.

2. **`Transitions`**
	- A collection of transitions (`HashSet<ITransition>`) originating from the encapsulated state.
	- Each transition determines how the state can change based on conditions.

#### Key Methods:
- **`AddTransition(ITransition transition)`**
	- Adds a general transition to this state. The transition logic is independent of specific targets.

- **`AddTransition(IState to, IPredicate condition)`**
	- Adds a transition to a specific target state, with a condition determining when the transition is allowed.
```csharp
StateNode idleNode = new StateNode(new IdleState());
idleNode.AddTransition(attackState, new Condition(() => player.IsInRange()));
```

- **Type Evaluation Utility**:
	- **`Is<T>(StateNode state)`**
		- Checks if a state encapsulated in the `StateNode` is of type `T`.
  ```csharp
if (StateNode.Is<IdleState>(currentStateNode)) Debug.Log("State is IdleState");
```

	- **`IsEither<T, U>(StateNode state)`**
		- Checks if the encapsulated state is of type `T` or `U`.

	- **`IsAny<T, U, V>(StateNode state)`**
		- Determines if the encapsulated state matches **any** of the types `T`, `U`, or `V`.
```csharp
if (StateNode.IsAny<IdleState, AttackState, DeathState>(currentStateNode))
	Debug.Log("Current state is valid for combat behavior.");
```

#### Example Integration:
`StateNode` is typically utilized internally by the `StateMachine`. Here’s an example of how nodes might be created and interact with each other:
```csharp
// Idle and Attack states
IState idleState = new IdleState();
IState attackState = new AttackState();

// Create StateNodes
StateNode idleNode = new StateNode(idleState);
StateNode attackNode = new StateNode(attackState);

// Add transition
idleNode.AddTransition(attackState, new Condition(() => player.IsInRange()));
```

---

#### Summary:
The `StateNode` class serves as the **backbone** for structuring states and their allowable transitions in the FSM. It provides extensible utilities for adding transitions and evaluating state types, ensuring smooth and scalable state management.

---

### 8. **`StateMachine` Class**

#### Purpose:
The central controller that coordinates states, transitions, and their execution. It's responsible for managing the FSM's lifecycle, invoking states' methods, and executing transitions.

#### Key Members:
1. **`StateNode`**
	- An internal structure that associates an `IState` with its transitions.

2. **`current`**
	- The active `StateNode` and state in the FSM.

3. **`nodes`**
	- A dictionary mapping state types to `StateNode` instances, storing all managed states.

4. **`anyTransitions`**
	- A collection of global transitions that are independent of the current state.

5. **`isStateNull`**
	- A cached boolean value indicating whether the current state is `null`.
	- Improved performance by replacing redundant `null` checks.

5. **`CurrentState`**
    - A property for retriving the current active state

#### Key Methods:
- **`Awake()`**
	- Invokes the `Awake()` method of the current state if that state's `LifecycleRequirements` indicates it is used.

- **`Start()`**
	- Invokes the `Start()` method of the current state if that state's `LifecycleRequirements` indicates it is used.

- **`Update()`**
	- Continuously runs during the lifecycle of the FSM.
	- Invokes the `Update()` method of the current state if that state's `LifecycleRequirements` indicates it is used.
	- No longer responsible for checking state transitions (delegated to `ProcessStateChange()`).

- **`FixedUpdate()`**
	- Invokes `FixedUpdate()` on the current state if the state requires it, based on that state's `LifecycleRequirements`.

- **`ateUpdate()`**
	- Invokes `LateUpdate()` on the current state if the state requires it, based on that state's `LifecycleRequirements`.

- **`ProcessStateChange()`**
	- Encapsulates state transition logic.
	- Checks all transitions for validity and handles state changes accordingly.

- **`SetCurrentState(IState state, bool enterAfterSetting = false)`**
	- Safely sets the current state to a specific `IState`.
	- Updates the cached `isStateNull` value.
	- Optionally calls the `Enter()` method immediately after setting the state.

- **`SetStartingState(IState state)`**
	- Helper method to set the initial state and immediately enter it.

- **`IsState<T>()`**
	- Helper method for checking if the current state is of type `T`.

   ```csharp
   if (stateMachine.IsCurrentState<GroundedState>()) DoGroundedStateBehavior();
   ```

- **`IsStateEither<T, U>()`**  
  Description: Checks if the current state is of either two specified types.

- **`IsStateAny<T, U, V>()`**  
  Description: Determines if the active state matches any of three specified types.

- **`ChangeState(IState newState)`**
	- Handles the state transition process.
	- Calls `Exit()` on the current state, switches to the new state, and calls `Enter()` on the new state.

---

### 9. **General Workflow**

1. **Define States:**
	- Implement `IState` for all states required by the FSM.
	- Specify each state's `LifecycleRequirements` based on the lifecycle methods it uses.

2. **Define Transitions:**
	- Create `Transition` objects by associating a condition (`IPredicate`) with a target state (`IState`).

3. **Initialize StateMachine:**
	- Instantiate the `StateMachine`.
	- Add states and their transitions to it.
	- Set the starting state using `SetStartingState()`.

4. **Runtime Execution:**
	- Continuously call `Update()`, `FixedUpdate()`, and `LateUpdate()` on the FSM (commonly done in Unity's respective lifecycle methods).
	- The FSM evaluates transitions within `ProcessStateChange()` and updates the current state as needed.

---

### 10. Usage Example
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

### Summary

Each component in this FSM system works together to create a robust, reusable, and extensible solution for managing state-based behaviors in Unity. The modular structure allows for easy customization and fits a variety of use cases. By encapsulating states, transitions, and conditions separately, the system becomes both flexible and highly maintainable.