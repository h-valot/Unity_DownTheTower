using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_SoundTriggerBoxHandler : MonoBehaviour
{
    [SerializeField] private GameObject m_parent;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterMotor>())
        {

        }
    }
}
