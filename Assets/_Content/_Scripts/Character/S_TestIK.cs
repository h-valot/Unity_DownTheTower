using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TestIk : MonoBehaviour
{
	[Header("Internal references")]
	[Required("TestIk required an animator to work.")]
	[SerializeField] private Animator m_animator;
	[SerializeField] private Transform m_rightHandLiftTorchTarget = null;
	[SerializeField] private Transform m_rightHandAimTorchTarget = null;
	[SerializeField] private Transform m_leftFootObj;
	[SerializeField] private Transform m_rightFootObj;

	[Header("External references")]
	[SerializeField] private CharacterMotor m_characterMotor;

	// ---- PRIVATE VARIABLES ----
	private bool m_ikLiftTorch = false;
    private bool m_ikAimTorch = false;
	
    private void OnAnimatorIK()
    {
		// Assert: animator is null
        if (m_animator == null) return;

		if (m_ikLiftTorch) SetIK(m_rightHandLiftTorchTarget);
		if (m_ikAimTorch) SetIK(m_rightHandAimTorchTarget);

		// else
		// {
		//    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
		//    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
		//    animator.SetLookAtWeight(0);
		// }
    }

	private void SetIK(Transform transform)
	{
		// Assert: transform is null
		if (transform == null) return;

		m_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
		m_animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
		m_animator.SetIKPosition(AvatarIKGoal.RightHand, transform.position);
		m_animator.SetIKRotation(AvatarIKGoal.RightHand, transform.rotation);
	}

    private void Update()
    {
        // int layerMask = 1 << 8;

        // layerMask = ~layerMask;

        // RaycastHit hit;

        // Vector3 leftRaycast = Vector3.down;
        // UnityEngine.Debug.DrawRay(leftFootObj.position, leftRaycast, Color.red, 0.1f);

        // if (Physics.Raycast(leftFootObj.position, leftRaycast, out hit, 0.1f, layerMask))
        // {
        //    UnityEngine.Debug.DrawRay(leftFootObj.position, leftRaycast, Color.red, 100);
        //    Debug.Log("Found an object - distance: " + leftFootObj.position);
        // }

        if (m_characterMotor.HandObject != null)
		{
			m_ikLiftTorch = m_characterMotor.HandObject.Type == CraftType.TORCH;
        }
        else
        {
            m_ikLiftTorch = false;
        }

		m_ikAimTorch = m_characterMotor.IsAiming;
    }
}
