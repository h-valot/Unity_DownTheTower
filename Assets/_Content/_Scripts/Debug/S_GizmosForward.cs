using Sirenix.OdinInspector;
using UnityEngine;

public class GizmosForward : MonoBehaviour
{
	[Title("Tweakable values")]
	[SerializeField] private Color m_color = Color.red;
	[SerializeField] private float m_length = 0.25f;

#if UNITY_EDITOR

	public void OnDrawGizmos()
	{
		Gizmos.color = m_color;
		Gizmos.DrawRay(transform.position, transform.forward * m_length);
	}

#endif
}