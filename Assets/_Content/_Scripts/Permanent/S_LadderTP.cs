using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class LadderTP : MonoBehaviour
{
    private Vector3 _teleportTo;
    private bool _playerIsIn = false;
    private CharacterMotor _character;
    private bool _isBottomTP;

    private void Update()
    {
        if (_playerIsIn)
        {
            if (_isBottomTP)
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    _character.transform.position = _teleportTo;
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    _character.transform.position = _teleportTo;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var character))
        {
            _playerIsIn = true;
            _character = character;
        }
    }

    public void SetVariables(Vector3 origin, Vector3 destination, bool isBottomTP)
    {
        transform.position = origin;
        _teleportTo = destination;
        _isBottomTP = isBottomTP;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var character))
        {
            _playerIsIn = false;
        }
    }
}
