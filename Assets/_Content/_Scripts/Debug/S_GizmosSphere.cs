using Sirenix.OdinInspector;
using UnityEngine;

public class GizmosSphere : MonoBehaviour
{
	[Title("Tweakable values")]
	[SerializeField] private Color m_color = Color.red;
	[SerializeField] private float m_radius = 0.25f;

#if UNITY_EDITOR

	public void OnDrawGizmos()
	{
		Gizmos.color = m_color;
		Gizmos.DrawWireSphere(transform.position, m_radius);
	}

#endif
}