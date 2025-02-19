using Sirenix.OdinInspector;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
	[Title("External references")]
    [SerializeField] private Animator m_animator;
    [SerializeField] private CharacterMotor m_characterMotor;
    [SerializeField] private RSO_CharacterState m_rsoCharacterState;
    [SerializeField] private RSE_ThrowRope m_rseThrowRope;
    [SerializeField] private RSE_RopeAttached m_rseRopeAttached;

    // ---- PRIVATE VARIABLES ----
    private int m_moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private int m_isGroundedHash = Animator.StringToHash("IsGrounded");
    private int m_isJumpingHash = Animator.StringToHash("IsJumping");
    //private int m_startAimingHash = Animator.StringToHash("StartAiming");
    private int m_isThrowingHash = Animator.StringToHash("IsThrowing");
    private int m_locomotionState = Animator.StringToHash("IsLocomotion");
    private int m_FallState = Animator.StringToHash("IsFall");
    //private int m_CraftState = Animator.StringToHash("IsCraft");
    private int m_RopeState = Animator.StringToHash("IsRope");
    private int m_isRopeAttachedHash = Animator.StringToHash("IsRopeAttached");

    private bool m_ropeAttached;
    private BehaviorState m_currentState;

    void LateUpdate()
    {
        DetermineState();
        m_animator.SetFloat(m_moveSpeedHash, Mathf.Abs(m_characterMotor.Rigidbody.velocity.magnitude));
        m_animator.SetBool(m_isJumpingHash, m_characterMotor.m_hasJumped);
        m_animator.SetBool(m_isGroundedHash, m_characterMotor.m_isGrounded);
        m_animator.SetBool(m_isRopeAttachedHash, m_ropeAttached);
        //m_animator.SetBool(m_startAimingHash, m_characterMotor.m_startAiming);
        //m_animator.SetBool(m_isThrowingHash, m_rseThrowRope.value);
        if (m_characterMotor.IsRopeValid == false)
        {
            m_ropeAttached = false;
        }

    }

    private void DetermineState()
    {
        if (m_rsoCharacterState.value == BehaviorState.LOCOMOTION && m_rsoCharacterState.value != m_currentState)
        {
            m_animator.SetBool(m_locomotionState, true);
            m_animator.SetBool(m_FallState, false);
            m_animator.SetBool(m_RopeState, false);
            m_currentState = m_rsoCharacterState.value;
            OnResetAttach();
        }
        else if (m_rsoCharacterState.value == BehaviorState.FALL && m_rsoCharacterState.value != m_currentState)
        {
            m_animator.SetBool(m_locomotionState, false);
            m_animator.SetBool(m_FallState, true);
            m_animator.SetBool(m_RopeState, false);
            m_currentState = m_rsoCharacterState.value;
            OnResetAttach();
        }
        else if (m_rsoCharacterState.value == BehaviorState.ROPE && m_rsoCharacterState.value != m_currentState)
        {
            m_animator.SetBool(m_locomotionState, false);
            m_animator.SetBool(m_FallState, false);
            m_animator.SetBool(m_RopeState, true);
            m_currentState = m_rsoCharacterState.value;
        }
        else if (m_rsoCharacterState.value == BehaviorState.CRAFT && m_rsoCharacterState.value != m_currentState)
        {
            m_animator.SetBool(m_locomotionState, false);
            m_animator.SetBool(m_FallState, false);
            m_animator.SetBool(m_RopeState, false);
            m_currentState = m_rsoCharacterState.value;
            OnResetAttach();
        }
    }


    private void OnEnable()
    {
        m_rseRopeAttached.action += OnRopeAttached;
    }

    private void OnDisable()
    {
        m_rseRopeAttached.action -= OnRopeAttached;
    }
    private void OnRopeAttached()
    {
        m_ropeAttached = true;
    }
    private void OnResetAttach()
    {
        m_ropeAttached = false;
    }
}
