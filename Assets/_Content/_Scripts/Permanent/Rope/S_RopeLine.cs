using Sirenix.OdinInspector;
using UnityEngine;

public class RopeLine : MonoBehaviour
{
	[Title("Internal references")]
	[SerializeField] private LineRenderer m_lineRenderer;
	
	[HideInInspector] public Vector3[] Positions;

	public void SetPositions(Vector3 from, Vector3 to)
	{
		Positions = new Vector3[] { from, to };

		m_lineRenderer.positionCount = 2;
		m_lineRenderer.SetPositions(Positions);
	}

	public void SetColor(Material material)
	{
		m_lineRenderer.sharedMaterial = material;
	}
}