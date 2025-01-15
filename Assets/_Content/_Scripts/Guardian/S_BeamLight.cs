using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamLight : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private LayerMask m_layerToIgnore;
    [SerializeField] private float m_beamLength;

    [Header("Internal References")]
    [SerializeField] private Transform m_beamLightTransform;

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(transform.position, transform.up, out RaycastHit hit, m_beamLength, ~m_layerToIgnore))
        {
            m_beamLightTransform.localScale = Vector3.one * (hit.distance / m_beamLength);
        }
        else
        {
            m_beamLightTransform.localScale = Vector3.one;
        }
    }
}
