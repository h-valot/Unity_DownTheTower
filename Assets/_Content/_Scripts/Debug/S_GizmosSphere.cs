using UnityEngine;

public class GizmosSphere : MonoBehaviour
{
	[Header("Tweakable values")]
	[SerializeField] private Color color = Color.red;
	[SerializeField] private float radius = 0.25f;

#if UNITY_EDITOR

	public void OnDrawGizmos()
	{
		Gizmos.color = color;
		Gizmos.DrawWireSphere(transform.position, radius);
	}

#endif
}