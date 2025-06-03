using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class AnimatorManagerGuardian : MonoBehaviour
{
	[FoldoutGroup("Tweakable values")][SerializeField] private SSO_Sound m_ssoFootstepRun;
	[FoldoutGroup("Tweakable values")][SerializeField] private SSO_Sound m_ssoFootstepWalk;

    [FoldoutGroup("External references")][SerializeField] private GuardianMotor m_guardianMotor;
    [FoldoutGroup("External references")][SerializeField] private NavMeshAgent m_guardianAgent;

	[FoldoutGroup("Internal references")][SerializeField] private GameObject m_footLocation;
	[FoldoutGroup("Internal references")][SerializeField] private Animator m_animator;

    [FoldoutGroup("Scriptable")][SerializeField] private RSE_GuardianKill m_rseGuardianKill;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_PlaySound m_rsePlaySound;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_PlaySoundAt m_rsePlaySoundAt;

    // ---- PRIVATE VARIABLES ----
    private int m_moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private int m_inPursuitHash = Animator.StringToHash("InPursuit");
    private int m_startKillingHash = Animator.StringToHash("StartKilling");

    private float m_moveSpeed;
    private bool m_inPursuit;
    private bool m_startKill;

    void LateUpdate()
    {
        m_moveSpeed = Mathf.Abs(m_guardianAgent.velocity.magnitude);
        if (m_guardianMotor.m_currentState == GuardianBehaviorState.AGGRO)
        {
            m_inPursuit = true;           
        }
        else
        {
            m_inPursuit = false;
            m_startKill = false;
        }

        m_animator.SetFloat(m_moveSpeedHash, m_moveSpeed);
        m_animator.SetBool(m_inPursuitHash, m_inPursuit);
        m_animator.SetBool(m_startKillingHash, m_startKill);
    }   

    private void OnEnable()
    {
        m_rseGuardianKill.action += Onkill;
    }

    private void OnDisable()
    {
        m_rseGuardianKill.action -= Onkill;
    }

    private void Onkill()
    {
        m_startKill = true;
    }

    #region ANIMATION EVENTS

    private void OnAnimEventFootWalk()
    {
        if (!m_inPursuit)
        {
            m_rsePlaySoundAt.Call(m_ssoFootstepWalk, m_footLocation.transform.position);
        }
        else
        {
            m_rsePlaySoundAt.Call(m_ssoFootstepRun, m_footLocation.transform.position);
        }
    }

    #endregion
}
