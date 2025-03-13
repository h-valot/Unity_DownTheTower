using Sirenix.OdinInspector;
using UnityEngine;

public class TutoCollider : MonoBehaviour
{
	[FoldoutGroup("Tweakable values")][SerializeField] private TutoColliderType m_type;
	
	[FoldoutGroup("Scriptables")][SerializeField] private RSE_TutoColliderEnters m_rseTutoColliderEnters;

	private void OnTriggerEnter(Collider collider)
	{
		if (collider.TryGetComponent<CharacterMotor>(out var character))
		{
			m_rseTutoColliderEnters.Call(m_type);
		}
	}
}