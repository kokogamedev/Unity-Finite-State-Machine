using System;
using UnityEngine;

namespace PsigenVision.Controller.Character
{
    /// <summary>
    /// Defines methods to track character movement events during gameplay.
    /// </summary>
    public interface ICharacterMovementCallbacks
    {
        void OnJumpStart();
        void OnFallStart();
        void OnGroundContactLost();
        void OnGroundContactRegained();
    }

    /// <summary>
    /// Declares events related to character movement within the gameplay context.
    /// </summary>
    public interface ICharacterMovementEvents
    {
        /// <summary>
        /// Event triggered whenever the character performs a jump action.
        /// </summary>
        /// <remarks>
        /// Provides the character's momentum as a <c>Vector3</c> parameter
        /// when the jump action begins. This can be useful for tracking
        /// directional movement and jump force in custom game logic.
        /// </remarks>
        event Action<Vector3> OnJump; //Passes character's momentum through as an argument

        /// <summary>
        /// Event triggered whenever the character regains contact with the ground after being in the air.
        /// </summary>
        /// <remarks>
        /// Provides the character's momentum upon landing as a <c>Vector3</c> parameter.
        /// This can be useful for calculating landing impact or integrating custom landing behaviors in game logic.
        /// </remarks>
        event Action<Vector3> OnLand; //Passes character's momentum through as an argument
    }

    /// <summary>
    /// Provides access to character kinematic properties such as velocities and momentum.
    /// </summary>
    public interface ICharacterKinematics
    {
        /// <summary>
        /// Represents the current linear velocity of the character.
        /// </summary>
        /// <remarks>
        /// Provides the direct velocity of the character as a <c>Vector3</c>, typically derived from its rigidbody kinematics.
        /// This value reflects the actual velocity in world space, encompassing all movement forces such as physics-based calculations or external impulses.
        /// </remarks>
        Vector3 LinearVelocity { get; } //refers to actual velocity derived from kinematics (rigidbody)

        /// <summary>
        /// Represents the velocity of the character derived from movement speed
        /// and movement direction, rather than physical kinematics or rigidbody calculations.
        /// </summary>
        /// <remarks>
        /// This property provides the calculated velocity based on the character's input
        /// and intended movement, distinguishing it from the actual kinematic velocity.
        /// It can be used to track or influence movement patterns independently of physics.
        /// </remarks>
        Vector3 MovementVelocity { get; } //refers to velocity derived from movement speed and movement direction

        /// <summary>
        /// Represents the linear momentum of the character in motion, expressed as a <c>Vector3</c>. Whether this momentum is local or global is determined by settings in the character controller.
        /// </summary>
        /// <remarks>
        /// Linear momentum is calculated as the product of the character's mass and its linear velocity.
        /// It is commonly used in physics calculations to determine the motion state or to simulate
        /// realistic interactions with other physical objects in the game environment.
        /// </remarks>
        Vector3 LinearMomentum { get; } //refers to momentum derived from kinematics - whether it is local or global is determined by internal settings and calculation  
    }
}