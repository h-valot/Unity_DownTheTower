using UnityEngine;

public class TP : MonoBehaviour
{
    [SerializeField] private GameObject _endTP;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var character))
        {
            character.transform.position = _endTP.transform.position;
            Physics.SyncTransforms();
            Debug.Log("tp");
        }
    }
}
