using UnityEngine;

public class TP : MonoBehaviour
{
    [SerializeField] private Transform m_arrivalPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var character))
        {
			character.SetCharacterPosition(m_arrivalPoint.position, Quaternion.identity);
        }
    }
}
