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
    [SerializeField] private SSO_Sound m_ssoFootstepRun;
    [SerializeField] private SSO_Sound m_ssoFootstepWalk;
    [SerializeField] private SSO_Sound m_ssoLanding;
    [SerializeField] private RSE_PlaySound m_rsePlaySound;
    [SerializeField] private RSE_PlayAt m_rsePlayAt;
    [SerializeField] private GameObject m_footLocation;



    // ---- PRIVATE VARIABLES ----
    private int m_moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private int m_verticalSpeed = Animator.StringToHash("VerticalSpeed");
    private int m_horizontalSpeed = Animator.StringToHash("HorizontalSpeed");
    private int m_isGroundedHash = Animator.StringToHash("IsGrounded");
    private int m_isJumpingHash = Animator.StringToHash("IsJumping");
    private int m_startAimingHash = Animator.StringToHash("StartAiming");
    private int m_isThrowingHash = Animator.StringToHash("IsThrowing");
    private int m_locomotionState = Animator.StringToHash("IsLocomotion");
    private int m_FallState = Animator.StringToHash("IsFall");
    private int m_RopeState = Animator.StringToHash("IsRope");
    private int m_isRopeAttachedHash = Animator.StringToHash("IsRopeAttached");

    private float m_moveSpeed;
    private bool m_ropeThrow;
    private bool m_ropeAttached;
    private BehaviorState m_currentState;

    void LateUpdate()
    {
        m_moveSpeed = Mathf.Abs(m_characterMotor.Rigidbody.velocity.magnitude);
        DetermineState();
        m_animator.SetFloat(m_moveSpeedHash, m_moveSpeed);
        m_animator.SetFloat(m_verticalSpeed, Mathf.Abs(m_characterMotor.Rigidbody.velocity.y));
        m_animator.SetFloat(m_horizontalSpeed, Mathf.Abs(new Vector3(m_characterMotor.Rigidbody.velocity.x, 0, m_characterMotor.Rigidbody.velocity.z).magnitude));
        m_animator.SetBool(m_isJumpingHash, m_characterMotor.m_hasJumped);
        m_animator.SetBool(m_isGroundedHash, m_characterMotor.m_isGrounded);
        m_animator.SetBool(m_isRopeAttachedHash, m_ropeAttached);
        m_animator.SetBool(m_isThrowingHash, m_ropeThrow);
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
        m_rseThrowRope.action += OnRopeThrow;
    }

    private void OnDisable()
    {
        m_rseRopeAttached.action -= OnRopeAttached;
        m_rseThrowRope.action -= OnRopeThrow;
    }
    private void OnRopeAttached()
    {
        m_ropeAttached = true;

    }

    private void OnRopeThrow(bool isThrow)
    {
        m_ropeThrow = isThrow;
    }
    private void OnResetAttach()
    {
        m_ropeAttached = false;
    }

    public void OnEventRopeAttached()
    {
        m_ropeAttached = false;
    }

    #region Animations Events

    private void OnAnimEventFootWalk(float speed)
    {
        if (1.5 < m_moveSpeed && m_moveSpeed < speed)
        {
            m_rsePlaySound.Call(m_ssoFootstepWalk);
        }

    }

    private void OnAnimEventFootRun(float speed)
    {
        if (m_moveSpeed > speed)
        {
            m_rsePlayAt.Call(m_ssoFootstepRun, m_footLocation.transform.position);
        }

    }

    private void OnAnimEventLanding()
    {
        m_rsePlayAt.Call(m_ssoLanding, m_footLocation.transform.position);
    }


    #endregion
}
