using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterGlow : MonoBehaviour
{
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Character m_ssoCharacter;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;

    private void OnEnable()
    {
		UpdateGlowGlobalParameters();

        m_rsoCharacterPosition.OnChanged += UpdateCharPositionShaderGlobalParameter;
        m_ssoCharacter.OnSSOChanged += UpdateGlowGlobalParameters;
	}

    private void OnDisable()
    {
        m_rsoCharacterPosition.OnChanged -= UpdateCharPositionShaderGlobalParameter;
        m_ssoCharacter.OnSSOChanged -= UpdateGlowGlobalParameters;
    }

    private void UpdateCharPositionShaderGlobalParameter()
    {
        Shader.SetGlobalVector("_GlowOrigin", transform.position);
    }

    private void UpdateGlowGlobalParameters()
    {
        Shader.SetGlobalFloat("_GlowHeight", m_ssoCharacter.GlowHeight);
        Shader.SetGlobalFloat("_GlowRadius", m_ssoCharacter.GlowRadius);
        Shader.SetGlobalFloat("_GlowStrength", m_ssoCharacter.GlowStrength);
        Shader.SetGlobalColor("_GlowColor", m_ssoCharacter.GlowColor);
    }
}