using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.VFX;

public class S_Firefly : MonoBehaviour
{
    [FoldoutGroup("Scriptables")][SerializeField] private SSO_Game m_ssoGame;
    [FoldoutGroup("Scriptables")][SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;

    [FoldoutGroup("Internal References")][SerializeField] private VisualEffect m_VFXsystem;

    private void OnEnable()
    {
        m_rsoCharacterPosition.OnChanged += UpdateStateVFX;
        UpdateStateVFX();
    }

    private void OnDisable()
    {
        m_rsoCharacterPosition.OnChanged -= UpdateStateVFX;
    }

    private void UpdateStateVFX()
    {
        if ((m_rsoCharacterPosition.value - transform.position).magnitude > m_ssoGame.DinstanceLightDeactivate)
        {
            if (m_VFXsystem.enabled) m_VFXsystem.enabled = false;
        }
        else
        {
            if (!m_VFXsystem.enabled) m_VFXsystem.enabled = true;
        }
    }
}
