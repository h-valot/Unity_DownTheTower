using UnityEngine;

public class CharacterGlow : MonoBehaviour
{
    [Header("Scriptables references")]
    [SerializeField] private OldCharacterConfig _characterConfig;
	[Space(5)]
    [SerializeField] private RSO_CharacterPosition _rsoCharacterPosition;

    private void OnEnable()
    {
		UpdateGlowGlobalParameters();

        _rsoCharacterPosition.OnChanged += UpdateCharPositionShaderGlobalParameter;
        _characterConfig.OnHierarchyChanged += UpdateGlowGlobalParameters;
	}

    private void OnDisable()
    {
        _rsoCharacterPosition.OnChanged -= UpdateCharPositionShaderGlobalParameter;
        _characterConfig.OnHierarchyChanged -= UpdateGlowGlobalParameters;
    }

    private void UpdateCharPositionShaderGlobalParameter()
    {
        Shader.SetGlobalVector("_GlowOrigin", transform.position);
    }

    private void UpdateGlowGlobalParameters()
    {
        Shader.SetGlobalFloat("_GlowHeight", _characterConfig.glowHeight);
        Shader.SetGlobalFloat("_GlowRadius", _characterConfig.glowRadius);
        Shader.SetGlobalFloat("_GlowStrength", _characterConfig.glowStrength);
        Shader.SetGlobalColor("_GlowColor", _characterConfig.glowColor);
    }
}