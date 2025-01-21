using Sirenix.OdinInspector;
using UnityEngine;

public class GizmosSphere : MonoBehaviour
{
	[Title("Tweakable values")]
	[SerializeField] private Color m_color = Color.red;
	[SerializeField] private float m_radius = 0.25f;
	[SerializeField] private GizmosShpereType m_type;

#if UNITY_EDITOR

	public void OnDrawGizmos()
	{
		Gizmos.color = m_color;
		if (m_type == GizmosShpereType.WIRE) Gizmos.DrawWireSphere(transform.position, m_radius);
		else Gizmos.DrawSphere(transform.position, m_radius);
	}

#endif
}