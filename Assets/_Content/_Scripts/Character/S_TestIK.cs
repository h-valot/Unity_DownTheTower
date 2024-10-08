using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CharacterMotor;

[RequireComponent(typeof(Animator))]
public class TestIk : MonoBehaviour
{
    [SerializeField] private CharacterMotor _characterMotor;
    protected Animator animator;

    private bool _ikLiftTorch = false;
    private bool _ikAimTorch = false;

    [SerializeField] private Transform _rightHandLiftTorchTarget = null;
    [SerializeField] private Transform _rightHandAimTorchTarget = null;
    [SerializeField] private Transform _leftFootObj;
    [SerializeField] private Transform _rightFootObj;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK()
    {
        if (animator)
        {
            if (_ikLiftTorch)
            {

                if(_rightHandLiftTorchTarget != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand,1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand,1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand,_rightHandLiftTorchTarget.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, _rightHandLiftTorchTarget.rotation);
                }

            }

            if (_ikAimTorch)
            {
                if(_rightHandAimTorchTarget != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, _rightHandAimTorchTarget.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, _rightHandAimTorchTarget.rotation);
                }
            }

            //else
            //{
            //    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            //    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            //    animator.SetLookAtWeight(0);
            //}
        }
    }

    // Update is called once per frame
    void Update()
    {
        //int layerMask = 1 << 8;

        //layerMask = ~layerMask;

        //RaycastHit hit;

        //Vector3 leftRaycast = Vector3.down;
        //UnityEngine.Debug.DrawRay(leftFootObj.position, leftRaycast, Color.red, 0.1f);

        //if (Physics.Raycast(leftFootObj.position, leftRaycast, out hit, 0.1f, layerMask))
        //{
        //    UnityEngine.Debug.DrawRay(leftFootObj.position, leftRaycast, Color.red, 100);
        //    Debug.Log("Found an object - distance: " + leftFootObj.position);
        //}
        if(_characterMotor._craftInHand != null)
        {
            if (_characterMotor._craftInHand._craftType == CraftType.Torch)
            {
                _ikLiftTorch = true;
            }
            else
            {
                _ikLiftTorch = false;
            }
        }
        else
        {
            _ikLiftTorch = false;
        }

        if (_characterMotor._currentState == AnimationState.AIM)
        {
            _ikAimTorch = true;
        }
        else
        {
            _ikAimTorch = false;
        }
    }
}
