using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_AnimatorManager : MonoBehaviour
{
    
    // ---- External References ----

    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterMotor _characterMotor;

    // ---- Private variables ----

    private int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private int _isGroundedHash = Animator.StringToHash("IsGrounded");
    private int _isJumpingHash = Animator.StringToHash("IsJumping");

    void LateUpdate()
    {
        _animator.SetFloat(_moveSpeedHash, _characterMotor._moveSpeed);
        _animator.SetBool(_isGroundedHash, _characterMotor._isGrounded);
        _animator.SetBool(_isJumpingHash, _characterMotor._isJumping);
        Debug.Log(_characterMotor._isJumping.ToString());
    }
}
