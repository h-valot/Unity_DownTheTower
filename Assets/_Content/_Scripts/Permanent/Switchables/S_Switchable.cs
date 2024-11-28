using Sirenix.OdinInspector;
using UnityEngine;

public class Switchable : MonoBehaviour
{
    [Title("Switchable")]
    [SerializeField] protected bool m_isReversed = false;

    public void SwitchBehavior(bool isLeverActive)
    {
        // In case there is a need to do the "on" behavior when the lever is "off"
        if (m_isReversed) isLeverActive = !isLeverActive;

        if (isLeverActive) ActivateMechanism();
        else DeactivateMechanism();
    }

    protected virtual void ActivateMechanism()
    {

    }

    protected virtual void DeactivateMechanism()
    {

    }
}