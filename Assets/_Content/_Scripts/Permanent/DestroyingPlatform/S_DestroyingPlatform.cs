using UnityEngine;

public class DestroyingPlatform : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var character))
        {
            Destroy(gameObject);
        }
    }
}