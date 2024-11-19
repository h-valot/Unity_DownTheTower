using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TestIk : MonoBehaviour
{
	[Header("Internal references")]
	[Required("TestIk required an animator to work.")]
	[SerializeField] private Animator _animator;
	[SerializeField] private Transform _rightHandLiftTorchTarget = null;
	[SerializeField] private Transform _rightHandAimTorchTarget = null;
	[SerializeField] private Transform _leftFootObj;
	[SerializeField] private Transform _rightFootObj;

	[Header("External references")]
	[SerializeField] private OldCharacterMotor _characterMotor;

	// ---- PRIVATE VARIABLES ----
	private bool _ikLiftTorch = false;
    private bool _ikAimTorch = false;
	
    private void OnAnimatorIK()
    {
		// Assert: animator is null
        if (_animator == null) return;

		if (_ikLiftTorch) SetIK(_rightHandLiftTorchTarget);
		if (_ikAimTorch) SetIK(_rightHandAimTorchTarget);

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

		_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
		_animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
		_animator.SetIKPosition(AvatarIKGoal.RightHand, transform.position);
		_animator.SetIKRotation(AvatarIKGoal.RightHand, transform.rotation);
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

        if (_characterMotor._craftInHand != null)
		{
			_ikLiftTorch = _characterMotor._craftInHand.Type == CraftType.TORCH;
        }
        else
        {
            _ikLiftTorch = false;
        }

		_ikAimTorch = _characterMotor._aiming;
    }
}
