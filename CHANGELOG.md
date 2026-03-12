# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-03-10

### This is the first release of *PsigenVision Finite State Machine*.

### Added
- **StateMachine**:
    - Central manager for coordinating states and transitions.
    - Lifecycle handling (`Awake`, `Start`, `Update`).
    - Support for global and local transitions.
- **IState Interface**:
    - Defines a contract for implementing state logic.
    - Lifecycle methods: `Enter`, `OnAwake`, `OnStart`, `OnUpdate`, `Exit`.
- **IPredicate Interface**:
    - Allows for encapsulating transition conditions with a single `Evaluate()` method.
- **FuncPredicate**:
    - A highly flexible predicate implementation that uses a `Func<bool>` delegate to define conditions dynamically.
- **ITransition Interface**:
    - Provides a binding between target states and their conditions.
- **Transition**:
    - Concrete implementation associating a destination state (`To`) with its condition (`Condition`).
- Modular and extensible system for managing complex state-based behavior in Unity.

### Notes
- This is the *first release* of the FSM package.
- The system has been tested with Unity's built-in Player Loop to demonstrate basic AI switching and gameplay mechanics.
- The package has no known critical issues at this stage but is subject to further testing and potential enhancements.
- Feedback and contributions are highly welcomed!

---

## [0.1.1] - 2026-03-10

### This is a small refactoring with the addition of a quality of life method.

### Refactored
- Removed `GetCurrentState()` method within `StateMachine` class, replacing it with the property `CurrentState`, which performs precisely the same function.

### Added
- Introduced generic method `IsCurrentState<T>()` to `StateMachine` class, which checks if the state machine's current state, `current`, is of type `T`

---

## [0.1.2] - 2026-03-10

### Moderate refactoring with the addition of state-checking quality-of-life methods.

### Refactored
- Renamed `IsCurrentState<T>()` within the `StateMachine` class to `IsState<T>()` for consistency.
- Moved the type-checking logic for `StateMachine.IsState<T>()` to the `IState` interface through a new static `IState.Is<T>(IState state)` method.

### Added
- **`IState` Interface**:
  - Added static generic methods:
    - `Is<T>(IState state)`: Checks if a state is of the specified type.
    - `IsEither<T, U>(IState state)`: Checks if a state is of either of the two specified types.
    - `IsAny<T, U, V>(IState state)`: Checks if a state matches any of the three specified types, supporting polymorphism and inheritance.

- **`StateNode` Class**:
  - Added static generic methods:
    - `Is<T>(StateNode state)`: Checks if the state encapsulated within a node is of the specified type.
    - `IsEither<T, U>(StateNode state)`: Checks if the state matches either of two types, using `IState.IsEither<T, U>`.
    - `IsAny<T, U, V>(StateNode state)`: Checks if the state matches any of three types, using `IState.IsAny<T, U, V>`.

- **`StateMachine` Class**:
  - Introduced new methods for evaluating the current `StateNode`:
    - `IsStateEither<T, U>()`: Determines if the current state is of either of the two types.
    - `IsStateAny<T, U, V>()`: Determines if the current state matches any of the three specified types.

---
## [0.1.3] - 2026-03-11
### Added
- **`ActionPredicate`**: Introduced a new `IPredicate` implementation for event-driven transitions in the FSM system.
  - The `ActionPredicate` reacts to external triggers (e.g., `Action` events) and transitions states when the event occurs.
  - Added usage examples and updated documentation showcasing how to integrate `ActionPredicate` in projects.

### Updated
- Updated the **README** and **Documentation** to include:
  - The purpose and implementation details of the `ActionPredicate`.
  - A step-by-step example of how to use it in combination with state transitions.
  - Benefits of leveraging `ActionPredicate` alongside traditional predicates.

### Fixed
- Improved consistency in predicate implementation and documentation across the FSM system.

---


