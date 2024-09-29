using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGlow : MonoBehaviour
{
    [Header("External references")]
    [SerializeField] private RSO_CharacterPosition _rsoCharacterPosition;
    [SerializeField] private FormerCharacterConfig _characterConfig;

    private void OnEnable()
    {
        _rsoCharacterPosition.OnChanged += UpdateCharPositionShaderGlobalParameter;
        _characterConfig.OnValueChanged += UpdateGlowGlobalParameters;

        Shader.SetGlobalFloat("_GlowHeight", _characterConfig.glowHeight);
        Shader.SetGlobalFloat("_GlowRadius", _characterConfig.glowRadius);
        Shader.SetGlobalFloat("_GlowStrength", _characterConfig.glowStrength);
        Shader.SetGlobalColor("_GlowColor", _characterConfig.glowColor);
    }

    private void OnDisable()
    {
        _rsoCharacterPosition.OnChanged -= UpdateCharPositionShaderGlobalParameter;
        _characterConfig.OnValueChanged -= UpdateGlowGlobalParameters;
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
