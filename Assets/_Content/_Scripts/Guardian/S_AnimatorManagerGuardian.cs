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

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_PlaySound m_rsePlaySound;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_PlaySoundAt m_rsePlaySoundAt;

    // ---- PRIVATE VARIABLES ----
    private int m_moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private int m_inPursuitHash = Animator.StringToHash("InPursuit");

    private float m_moveSpeed;
    private bool m_inPursuit;

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
        }

        m_animator.SetFloat(m_moveSpeedHash, m_moveSpeed);
        m_animator.SetBool(m_inPursuitHash, m_inPursuit);
    }   

    #region ANIMATION EVENTS

    //private void OnAnimEventFootWalk(float speed)
    //{
    //    if (1.5 < m_moveSpeed && m_moveSpeed < speed)
    //    {
    //        m_rsePlaySoundAt.Call(m_ssoFootstepWalk, m_footLocation.transform.position);
    //    }
    //}

    //private void OnAnimEventFootRun(float speed)
    //{
    //    if (m_moveSpeed > speed)
    //    {
    //        m_rsePlaySoundAt.Call(m_ssoFootstepRun, m_footLocation.transform.position);
    //    }
    //}

    #endregion
}
