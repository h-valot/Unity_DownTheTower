using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerChangeStateGuardian : MonoBehaviour
{
    [SerializeField] private List<GameObject> _guardians;
    [SerializeField] private bool _setActifGuardian;

    private void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var _playerCheckRef))
        {
            ChangeState();
        }
    }

    private void ChangeState()
    {
        for (int i = 0; i < _guardians.Count; i++)
        {
            if (_setActifGuardian == true)
            {
                Guardian _guardianref = _guardians[i].GetComponent<Guardian>();
                _guardianref._isActif = true;
            }

            else
            {
                Guardian _guardianref = _guardians[i].GetComponent<Guardian>();
                _guardianref._isActif = false;
            }
        }
    }
        private bool SetActifGuardian()
    {
        if (_setActifGuardian == true)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
