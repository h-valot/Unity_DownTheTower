using Sirenix.OdinInspector;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
	[Title("External references")]
    [SerializeField] private Animator m_animator;
    [SerializeField] private CharacterMotor m_characterMotor;

    // ---- PRIVATE VARIABLES ----
    private int m_moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private int m_isGroundedHash = Animator.StringToHash("IsGrounded");
    private int m_isJumpingHash = Animator.StringToHash("IsJumping");

    void LateUpdate()
    {
        m_animator.SetFloat(m_moveSpeedHash, Mathf.Abs(m_characterMotor.Rigidbody.velocity.z) + Mathf.Abs(m_characterMotor.Rigidbody.velocity.x));
        m_animator.SetBool(m_isJumpingHash, m_characterMotor.m_hasJumped);
        m_animator.SetBool(m_isGroundedHash, m_characterMotor.m_isGrounded);
    }
}