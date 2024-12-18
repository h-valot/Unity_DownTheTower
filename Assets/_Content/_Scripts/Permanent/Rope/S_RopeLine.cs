using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

public class RopeLine : MonoBehaviour
{
	[Title("Internal references")]
	[SerializeField] private LineRenderer m_lineRenderer;
	
	[HideInInspector] public Vector3[] Positions;

	public void SetPositions(Vector3[] positions)
	{
		Positions = positions;

		m_lineRenderer.positionCount = Positions.Length;
		m_lineRenderer.SetPositions(Positions);
	}

	public void SetColor(Material material)
	{
		m_lineRenderer.sharedMaterial = material;
	}
}