using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGlow : MonoBehaviour
{
    [Header("External references")]
    [SerializeField] private RSO_PlayeGraphicsDirection _rsoPlayerTranform;
    [SerializeField] private CharacterConfig _characterConfig;

    private void OnEnable()
    {
        _rsoPlayerTranform.OnChanged += UpdateCharPositionShaderGlobalParameter;
        _characterConfig.OnValueChanged += UpdateGlowGlobalParameters;

        Shader.SetGlobalFloat("_GlowHeight", _characterConfig.glowHeight);
        Shader.SetGlobalFloat("_GlowRadius", _characterConfig.glowRadius);
        Shader.SetGlobalFloat("_GlowStrength", _characterConfig.glowStrength);
        Shader.SetGlobalColor("_GlowColor", _characterConfig.glowColor);
    }

    private void OnDisable()
    {
        _rsoPlayerTranform.OnChanged -= UpdateCharPositionShaderGlobalParameter;
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
