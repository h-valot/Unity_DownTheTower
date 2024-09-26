using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_TorchPointLight : MonoBehaviour
{
    [Header("Internal References")]
    [SerializeField] private Rigidbody _rigidbody;

    private void OnCollisionEnter(Collision _collision)
    {
        Debug.Log("Collision with the light");
        _rigidbody.constraints = ~RigidbodyConstraints.FreezePosition;
    }
}
