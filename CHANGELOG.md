# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-02-28

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