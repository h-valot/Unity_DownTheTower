using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterGlow : MonoBehaviour
{
	[FoldoutGroup("SSO")][SerializeField] private SSO_Character m_ssoCharacter;

	[FoldoutGroup("RSO")][SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;

    private void OnEnable()
    {
		UpdateGlowGlobalParameters();

        m_rsoCharacterPosition.OnChanged += UpdateCharPositionShaderGlobalParameter;
        m_ssoCharacter.OnConfigChanged += UpdateGlowGlobalParameters;
	}

    private void OnDisable()
    {
        m_rsoCharacterPosition.OnChanged -= UpdateCharPositionShaderGlobalParameter;
        m_ssoCharacter.OnConfigChanged -= UpdateGlowGlobalParameters;
    }

    private void UpdateCharPositionShaderGlobalParameter()
    {
        Shader.SetGlobalVector("_GlowOrigin", transform.position);
    }

    private void UpdateGlowGlobalParameters()
    {
        Shader.SetGlobalFloat("_GlowHeight", m_ssoCharacter.glowHeight);
        Shader.SetGlobalFloat("_GlowRadius", m_ssoCharacter.glowRadius);
        Shader.SetGlobalFloat("_GlowStrength", m_ssoCharacter.glowStrength);
        Shader.SetGlobalColor("_GlowColor", m_ssoCharacter.glowColor);
    }
}