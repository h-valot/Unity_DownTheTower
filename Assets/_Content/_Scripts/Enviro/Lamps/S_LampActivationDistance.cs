using UnityEngine;
using Sirenix.OdinInspector;

public class LampActivationDistance : MonoBehaviour
{
    [FoldoutGroup("Scriptables")][SerializeField] private SSO_Game m_ssoGame;
    [FoldoutGroup("Scriptables")][SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;

    [FoldoutGroup("Internal References")][SerializeField] private Light m_light;

    [FoldoutGroup("Config")][SerializeField] private float m_LightIntensity;

    private void OnEnable()
    {
        m_rsoCharacterPosition.OnChanged += UpdateLightIntensity;
        UpdateLightIntensity();
    }

    private void OnDisable()
    {
        m_rsoCharacterPosition.OnChanged -= UpdateLightIntensity;
    }

    private void UpdateLightIntensity()
    {
        if ((m_rsoCharacterPosition.value - transform.position).magnitude > m_ssoGame.DinstanceLightDeactivate)
        {
            if (m_light.enabled) m_light.enabled = false;
        }
        else
        {
            if (!m_light.enabled) m_light.enabled = true;
            m_light.intensity = Matha.RemapClamped(m_ssoGame.DinstanceLightDeactivate, m_ssoGame.DinstanceLightDeactivate - m_ssoGame.DinstanceLightDeactivate * (1f - m_ssoGame.DistancePercentLightFade), 0f, m_LightIntensity, (m_rsoCharacterPosition.value - transform.position).magnitude);
        }
    }
}
