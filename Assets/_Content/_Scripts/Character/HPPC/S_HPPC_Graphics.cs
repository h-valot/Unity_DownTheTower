using UnityEngine;

public class HPPC_Graphics : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] private Rigidbody _rigidbody;

    private void LateUpdate()
    {
        if(new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z) != Vector3.zero)
        {
            transform.localRotation = Quaternion.LookRotation(new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z), Vector3.up);
        }
    }
}
