using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
	[Header("External references")]
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterMotor _characterMotor;

    // ---- PRIVATE VARIABLES ----
    private int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private int _isGroundedHash = Animator.StringToHash("IsGrounded");
    private int _isJumpingHash = Animator.StringToHash("IsJumping");

    void LateUpdate()
    {
        _animator.SetFloat(_moveSpeedHash, _characterMotor._planarSpeed);
        _animator.SetBool(_isGroundedHash, _characterMotor._isGrounded);
        _animator.SetBool(_isJumpingHash, _characterMotor._currentState == AnimationState.JUMP);
    }
}