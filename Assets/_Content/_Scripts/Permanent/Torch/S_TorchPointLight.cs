using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchPointLight : MonoBehaviour
{
    [Header("Internal References")]
    [SerializeField] private Rigidbody _rigidbody;

    // ----- PRIVATE VARIABLES -----
    private bool _isInHand;

    private void Start()
    {
        _isInHand = true;
    }

    private void OnCollisionEnter(Collision _collision)
    {
        if (!_isInHand)
        {
            _rigidbody.constraints = ~RigidbodyConstraints.FreezePosition;
        }
    }

    public void SetIsInHand(bool _bool)
    {
        _isInHand = _bool;
    }
}
