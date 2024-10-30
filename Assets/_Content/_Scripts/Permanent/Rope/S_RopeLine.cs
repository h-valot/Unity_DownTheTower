using UnityEngine;

public class RopeLine : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private LineRenderer _lineRenderer;

	[Header("Debugging")]
	public Vector3[] positions;

	public void SetPositions(Vector3 from, Vector3 to)
	{
		positions = new Vector3[] { from, to };

		_lineRenderer.positionCount = 2;
		_lineRenderer.SetPositions(positions);
	}

	public void SetColor(Material material)
	{
		_lineRenderer.sharedMaterial = material;
	}
}