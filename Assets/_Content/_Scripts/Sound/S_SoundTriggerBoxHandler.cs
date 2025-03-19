using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_SoundTriggerBoxHandler : MonoBehaviour
{
    [SerializeField] private S_SoundTriggerBoxManager m_parent;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterMotor>())
        {
            m_parent.TriggerEnter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CharacterMotor>())
        {
            m_parent.TriggerExit();
        }
    }
}
