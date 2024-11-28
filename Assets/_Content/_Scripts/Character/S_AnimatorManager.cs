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
		// TODO - Update animation values
    }
}