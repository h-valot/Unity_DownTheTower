using Sirenix.OdinInspector;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
	[Title("External references")]
    [SerializeField] private Animator m_animator;
    [SerializeField] private CharacterMotor m_characterMotor;
    [SerializeField] private RSO_CharacterState m_rsoCharacterState;

    // ---- PRIVATE VARIABLES ----
    private int m_moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private int m_isGroundedHash = Animator.StringToHash("IsGrounded");
    private int m_isJumpingHash = Animator.StringToHash("IsJumping");
    private int m_startAimingHash = Animator.StringToHash("StartAiming");
    private int m_locomotionState = Animator.StringToHash("IsLocomotion");
    private int m_FallState = Animator.StringToHash("IsFall");
    private int m_CraftState = Animator.StringToHash("IsCraft");
    private int m_RopeState = Animator.StringToHash("IsRope");
    private BehaviorState m_currentState;

    void LateUpdate()
    {
        DetermineState();
        m_animator.SetFloat(m_moveSpeedHash, Mathf.Abs(m_characterMotor.Rigidbody.velocity.z) + Mathf.Abs(m_characterMotor.Rigidbody.velocity.x));
        m_animator.SetBool(m_isJumpingHash, m_characterMotor.m_hasJumped);
        m_animator.SetBool(m_isGroundedHash, m_characterMotor.m_isGrounded);
        m_animator.SetBool(m_startAimingHash, m_characterMotor.m_startAiming);
    }

    private void DetermineState()
    {
        if (m_rsoCharacterState.value == BehaviorState.LOCOMOTION && m_rsoCharacterState.value != m_currentState)
        {
            m_animator.SetBool(m_locomotionState, true);
            m_animator.SetBool(m_FallState, false);
            m_animator.SetBool(m_CraftState, false);
            m_animator.SetBool(m_RopeState, false);
            m_currentState = m_rsoCharacterState.value;
        }
        else if (m_rsoCharacterState.value == BehaviorState.FALL && m_rsoCharacterState.value != m_currentState)
        {
            m_animator.SetBool(m_locomotionState, false);
            m_animator.SetBool(m_FallState, true);
            m_animator.SetBool(m_CraftState, false);
            m_animator.SetBool(m_RopeState, false);
            m_currentState = m_rsoCharacterState.value;
        }
        else if (m_rsoCharacterState.value == BehaviorState.ROPE && m_rsoCharacterState.value != m_currentState)
        {
            m_animator.SetBool(m_locomotionState, false);
            m_animator.SetBool(m_FallState, false);
            m_animator.SetBool(m_CraftState, false);
            m_animator.SetBool(m_RopeState, true);
            m_currentState = m_rsoCharacterState.value;
        }
        else if (m_rsoCharacterState.value == BehaviorState.CRAFT && m_rsoCharacterState.value != m_currentState)
        {
            m_animator.SetBool(m_locomotionState, false);
            m_animator.SetBool(m_FallState, false);
            m_animator.SetBool(m_CraftState, true);
            m_animator.SetBool(m_RopeState, false);
            m_currentState = m_rsoCharacterState.value;
        }
    }
}