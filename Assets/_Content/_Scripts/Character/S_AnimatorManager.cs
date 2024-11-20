using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
	[Header("External references")]
    [SerializeField] private Animator m_animator;
    [SerializeField] private OldCharacterMotor m_characterMotor;

    // ---- PRIVATE VARIABLES ----
    private int m_moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private int m_isGroundedHash = Animator.StringToHash("IsGrounded");
    private int m_isJumpingHash = Animator.StringToHash("IsJumping");

    void LateUpdate()
    {
        m_animator.SetFloat(m_moveSpeedHash, m_characterMotor.PlanarSpeed);
        m_animator.SetBool(m_isGroundedHash, m_characterMotor.IsGrounded);
        m_animator.SetBool(m_isJumpingHash, m_characterMotor._currentState == AnimationState.JUMP);
    }
}