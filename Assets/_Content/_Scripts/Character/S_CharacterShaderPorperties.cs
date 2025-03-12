using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterShaderProperties : MonoBehaviour
{
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;

    private void OnEnable()
    {
        UpdateCharPositionShaderGlobalParameter();
        m_rsoCharacterPosition.OnChanged += UpdateCharPositionShaderGlobalParameter;
	}

    private void OnDisable()
    {
        m_rsoCharacterPosition.OnChanged -= UpdateCharPositionShaderGlobalParameter;
    }

    private void UpdateCharPositionShaderGlobalParameter()
    {
        Shader.SetGlobalVector("_CHARACTER_POSITION", m_rsoCharacterPosition.value);
    }
}