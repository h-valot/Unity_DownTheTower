using UnityEngine;

public class CharacterGlow : MonoBehaviour
{
    [Header("Scriptables references")]
    [SerializeField] private OldCharacterConfig m_characterConfig;
	[Space(5)]
    [SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;

    private void OnEnable()
    {
		UpdateGlowGlobalParameters();

        m_rsoCharacterPosition.OnChanged += UpdateCharPositionShaderGlobalParameter;
        m_characterConfig.OnHierarchyChanged += UpdateGlowGlobalParameters;
	}

    private void OnDisable()
    {
        m_rsoCharacterPosition.OnChanged -= UpdateCharPositionShaderGlobalParameter;
        m_characterConfig.OnHierarchyChanged -= UpdateGlowGlobalParameters;
    }

    private void UpdateCharPositionShaderGlobalParameter()
    {
        Shader.SetGlobalVector("_GlowOrigin", transform.position);
    }

    private void UpdateGlowGlobalParameters()
    {
        Shader.SetGlobalFloat("_GlowHeight", m_characterConfig.glowHeight);
        Shader.SetGlobalFloat("_GlowRadius", m_characterConfig.glowRadius);
        Shader.SetGlobalFloat("_GlowStrength", m_characterConfig.glowStrength);
        Shader.SetGlobalColor("_GlowColor", m_characterConfig.glowColor);
    }
}