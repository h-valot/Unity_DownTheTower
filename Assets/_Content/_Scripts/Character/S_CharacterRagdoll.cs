using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterRagdoll : MonoBehaviour
{
	[FoldoutGroup("Internal references")][SerializeField] private TorchModel m_pfTorchModel;
	[FoldoutGroup("Internal references")][SerializeField] private List<Rigidbody> m_rigidbodies;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterLastPosition m_rsoCharacterLastPosition;
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Torch m_ssoTorch;

	private MaterialPropertyBlock m_propertyBlock;
	private TorchModel m_torchModel;

	private void OnEnable()
	{
		m_rsoCharacterDeath.OnChanged += OnCharacterDied;
	}

	private void OnDisable()
	{
		m_rsoCharacterDeath.OnChanged -= OnCharacterDied;
	}

	public void Initialize(List<Transform> transforms, bool isCarryingLight, Vector3 torchAnchor)
	{
		Vector3 characterVelocity = m_rsoCharacterPosition.value - m_rsoCharacterLastPosition.value;

		for (int i = 0; i < transforms.Count; i++)
		{
			m_rigidbodies[i].position = transforms[i].position;
			m_rigidbodies[i].rotation = transforms[i].rotation;
			m_rigidbodies[i].linearVelocity = characterVelocity;
		}

		// Assert: The backpack isn't equipped yet.
		if (torchAnchor != Vector3.zero) 
		{
			m_torchModel = Instantiate(m_pfTorchModel, torchAnchor, Quaternion.identity);

			if (isCarryingLight)
			{
				m_torchModel.Light.enabled = true;

				m_propertyBlock = new MaterialPropertyBlock();
				m_propertyBlock.SetColor("_lightColor", m_ssoTorch.LightColor);
				m_propertyBlock.SetFloat("_lightPercent", 1f);
				m_torchModel.MeshRenderer.SetPropertyBlock(m_propertyBlock);
			}
		}
	}

	private void OnCharacterDied()
	{
		if (m_rsoCharacterDeath.value) return;

		Destroy(m_torchModel.gameObject);
		Destroy(gameObject);
	}
}