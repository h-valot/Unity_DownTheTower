using Sirenix.OdinInspector;
using UnityEngine;

public class RopeLine : MonoBehaviour
{
	[Title("Internal references")]
	[SerializeField] private LineRenderer m_lineRenderer;

	private Vector3[] m_positions;

	public void SetPositions(Vector3 from, Vector3 to)
	{
		m_positions = new Vector3[] { from, to };

		m_lineRenderer.positionCount = 2;
		m_lineRenderer.SetPositions(m_positions);
	}

	public void SetColor(Material material)
	{
		m_lineRenderer.sharedMaterial = material;
	}
}