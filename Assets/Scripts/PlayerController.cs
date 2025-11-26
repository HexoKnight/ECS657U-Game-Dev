using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

using KinematicCharacterController;

// lots of the initial code adapted from ExampleCharacterController in:
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
    }

    public void PostGroundingUpdate(float deltaTime)
    {
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        switch (_playerState)
        {
            case PlayerState.Normal:
                // Handle jump-related values
                {
                    // Handle jumping pre-ground grace period
                    if (_input.jump && _timeSinceJumpRequested > JumpPreGroundingGraceTime)
                    {
                        _input.jump = false;
                    }

                    if (allowJumpingWhenSliding ? _motor.GroundingStatus.FoundAnyGround : _motor.GroundingStatus.IsStableOnGround)
                    {
                        // If we're on a ground surface, reset jumping values
                        if (!_jumpedThisFrame)
                        {
                            _jumpConsumed = false;
                        }
                        _timeSinceLastAbleToJump = 0f;
                    }
                    else
                    {
                        // Keep track of time since we were last able to jump (for grace period)
                        _timeSinceLastAbleToJump += deltaTime;
                    }
                }

                // Handle uncrouching
                // if (_isCrouching && !_shouldBeCrouching)
                // {
                //     // Do an overlap test with the character's standing height to see if there are any obstructions
                //     _motor.SetCapsuleDimensions(0.5f, 2f, 1f);
                //     if (_motor.CharacterOverlap(
                //         _motor.TransientPosition,
                //         _motor.TransientRotation,
                //         _probedColliders,
                //         _motor.CollidableLayers,
                //         QueryTriggerInteraction.Ignore) > 0)
                //     {
                //         // If obstructions, just stick to crouching dimensions
                //         _motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
                //     }
                //     else
                //     {
                //         // If no obstructions, uncrouch
                //         MeshRoot.localScale = new Vector3(1f, 1f, 1f);
                //         _isCrouching = false;
                //     }
                // }

                break;
        }
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }
}