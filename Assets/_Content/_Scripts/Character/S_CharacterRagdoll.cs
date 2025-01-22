using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterRagdoll : MonoBehaviour
{
	[FoldoutGroup("Internal references")][SerializeField] private Light m_light;
	[FoldoutGroup("Internal references")][SerializeField] private MeshRenderer m_meshRenderer;
	[FoldoutGroup("Internal references")][SerializeField] private List<Rigidbody> m_rigidbodies;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Torch m_ssoTorch;

	private MaterialPropertyBlock m_propertyBlock;

	private void OnEnable()
	{
		m_rsoCharacterDeath.OnChanged += OnCharacterDied;
	}

	private void OnDisable()
	{
		m_rsoCharacterDeath.OnChanged -= OnCharacterDied;
	}

	public void Initialize(List<Transform> transforms, bool isCarryingLight)
	{
		for (int i = 0; i < transforms.Count; i++)
		{
			m_rigidbodies[i].position = transforms[i].position;
			m_rigidbodies[i].rotation = transforms[i].rotation;
		}

		if (isCarryingLight)
		{
			m_light.enabled = true;
			
			m_propertyBlock = new MaterialPropertyBlock();
			m_propertyBlock.SetColor("_lightColor", m_ssoTorch.LightColor);
			m_propertyBlock.SetFloat("_lightPercent", 1f);
			m_meshRenderer.SetPropertyBlock(m_propertyBlock);
		}
	}

	private void OnCharacterDied()
	{
		if (m_rsoCharacterDeath.value) return;

		Destroy(gameObject);
	}
}