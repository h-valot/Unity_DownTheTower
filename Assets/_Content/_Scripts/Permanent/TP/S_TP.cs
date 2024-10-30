using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TP : MonoBehaviour
{
    [SerializeField] private GameObject _endTP;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var _playerCheckRef))
        {
            _playerCheckRef.transform.position = _endTP.transform.position;
            Physics.SyncTransforms();
            Debug.Log("tp");
        }
    }
}
