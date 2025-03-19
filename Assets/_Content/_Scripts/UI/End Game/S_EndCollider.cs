using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndCollider : MonoBehaviour
{
    [FoldoutGroup("Scriptables")][SerializeField] private RSE_GameEnd m_rseGameEnd;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.TryGetComponent<CharacterMotor>(out var character))
        {
            m_rseGameEnd.Call();
        }
    }
}
