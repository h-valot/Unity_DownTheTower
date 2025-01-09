using System.Collections.Generic;
using System.Linq;
using EasyCurvedLine;
using Sirenix.OdinInspector;
using UnityEngine;

public class RopeGraphics : MonoBehaviour
{
	[Title("Internal references")]
	[SerializeField] private Rope m_rope;
	[SerializeField] private LineRenderer m_lineRenderer;
	[SerializeField] private RopeSegment m_baseSegment;
	
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Rope m_ssoRope;

	[Title("Debug")]
	[ShowInInspector] private List<RopeSegment> m_segments = new List<RopeSegment>();
	[ShowInInspector] private List<Vector3> m_points = new List<Vector3>();
	private float m_colliderDiameter;
	private Gradient m_gradient = new Gradient();
	private GradientColorKey[] m_gradientColorKey = new GradientColorKey[2];
	private GradientAlphaKey[] m_gradientAlphaKey = new GradientAlphaKey[2];

	#region MONOBEHAVIOUR

	private void Start()
	{
		m_baseSegment.gameObject.SetActive(false);
		m_baseSegment.OnInteractedWithRef += OnInteracted;
		m_baseSegment.SphereTrigger.radius = m_ssoRope.InteractableSphereRadius;
		m_ssoRope.PfRopeSegment.SphereTrigger.radius = m_ssoRope.InteractableSphereRadius;

		m_colliderDiameter = m_ssoRope.PfRopeSegment.ColliderRadius * 2f;
		m_segments.Add(m_baseSegment);

		ToggleCollisions(false);
		ToggleInteractables(false);
	}

	private void FixedUpdate()
	{
		// Assertion
		if (!m_rope.IsPlaced) return;

		HandlePoints();
		DrawRope();
	}

	private void OnEnable()
	{
		m_rope.OnDetached += OnDetached;
		m_rope.OnAttached += OnAttached;
	}

	private void OnDisable()
	{
		m_rope.OnDetached -= OnDetached;
		m_rope.OnAttached -= OnAttached;
	}

	#endregion

	private void HandlePoints()
	{
		m_points = new List<Vector3>();

		for (int i = 0; i < m_rope.Folds.Count; i++)
		{
			// Get start and end points
			Vector3 start = m_rope.Folds[i];
			Vector3 end = i == m_rope.Folds.Count - 1 ? m_rope.CharacterPosition : m_rope.Folds[i + 1];

			// Base calculation to get the best middle point
			float distance = (end - start).magnitude;
			Vector3 direction = (end - start).normalized;
			float dot = Mathf.Abs(Vector3.Dot(direction, Vector3.forward)); // 0 = perpendicular

			// Get middle point
			Vector3 middle = start + direction * (distance / 2);
			Vector3 midOffset = middle + Vector3.down * (1 - dot) * m_ssoRope.MiddlePointDownOffsetModifier * distance;
			Physics.Raycast(middle, Vector3.down, out var hitInfo);
			Vector3 midRaycastHit = hitInfo.point;
			middle = midRaycastHit.y > midOffset.y ? midRaycastHit : midOffset;

			m_points.AddUnique(start.CutDigits(2));
			m_points.AddUnique(middle.CutDigits(2));
			m_points.AddUnique(end.CutDigits(2));
		}
	}

	private void DrawRope()
	{
		// Generate smoothed points using a Bezier curve
		Vector3[] points = m_rope.IsConnected ? m_points.ToArray() : m_segments.Select(s => s.transform.position).ToArray();
		if (points.Length < 3) return;
		Vector3[] smoothedPoints = LineSmoother.SmoothLine(points, m_ssoRope.LineSegmentSize);

		// Update line renderer settings
		m_lineRenderer.positionCount = smoothedPoints.Length;
		m_lineRenderer.SetPositions(smoothedPoints);
		m_lineRenderer.startWidth = m_ssoRope.LineWidth;
		m_lineRenderer.endWidth = m_ssoRope.LineWidth;

		// Set colors and alphas
		float lengthPercentage = Mathf.Clamp01(m_rope.GetTotalLength() / m_ssoRope.MaxLength);

		m_gradientColorKey[0] = new GradientColorKey(m_ssoRope.ropeGradient.Evaluate(0f), 0f);
		m_gradientColorKey[1] = new GradientColorKey(m_ssoRope.ropeGradient.Evaluate(lengthPercentage), 1f);

		m_gradientAlphaKey[0] = new GradientAlphaKey(1f, 0f);
		m_gradientAlphaKey[1] = new GradientAlphaKey(1f, 1f);
		m_gradient.SetKeys(m_gradientColorKey, m_gradientAlphaKey);

		// Apply gradient to the line renderer
		m_lineRenderer.colorGradient = m_gradient;
	}

	public void SpawnSegments()
	{
		// Spawn segments
		for (int i = 0; i < m_points.Count - 1; i++)
		{
			Vector3 lineDirection = (m_points[i + 1] - m_points[i]).normalized;
			float lineLength = (m_points[i + 1] - m_points[i]).magnitude;
			int colliderAmount = Mathf.FloorToInt(lineLength / m_colliderDiameter);
			colliderAmount = Mathf.Clamp(colliderAmount, 1, colliderAmount);

			for (int j = 0; j < colliderAmount; j++)
			{
				// Assert: The first segment of the first line must be ignored and replaced by the base repe segment.
				if (j == 0 && i == 0) j++;

				var newSegment = Instantiate(m_ssoRope.PfRopeSegment, j == 1 && i == 0 ? m_baseSegment.transform : m_segments[^1].transform);
				newSegment.transform.rotation = Quaternion.LookRotation(lineDirection);
				newSegment.OnInteractedWithRef += OnInteracted;

				// Set the first segment of the line as static
				if (j == 0)
				{
					newSegment.Freeze();
					newSegment.transform.position = m_points[i];
				}

				// Snap the position of the last segment of the last line 
				else if (j == colliderAmount - 1 && i == m_points.Count - 2)
				{
					newSegment.Free();
					newSegment.transform.position = m_points[i] + lineDirection * (lineLength / colliderAmount) * j;
				}

				// Place in-between segments
				else
				{
					newSegment.transform.position = m_points[i] + lineDirection * (lineLength / colliderAmount) * j;
				}

				newSegment.ToggleCollider(i == m_points.Count - 1);
				newSegment.SetLimit(m_ssoRope.PfRopeSegment.ColliderRadius + 0.1f);
				m_segments.Add(newSegment);
			}
		}

		// Connect them together
		m_baseSegment.Joint.connectedBody = m_segments[0].Rigidbody;
		for (int i = 0; i < m_segments.Count; i++)
		{
			m_segments[^1].Connect(m_segments[^1].Rigidbody);
		}
	}

	private void OnAttached()
	{
		m_baseSegment.gameObject.SetActive(true);
	}

	private void OnDetached()
	{
		ToggleCollisions(true);
		ToggleInteractables(true);

		SpawnSegments();
	}

	private void OnInteracted(CharacterInteract characterInteract)
	{
		ToggleCollisions(false);
		ToggleInteractables(false);

		m_rope.Reattach(characterInteract);

		foreach (var segment in m_segments)
		{
			Destroy(segment.gameObject);
		}
		m_segments.Clear();
	}

	public void ToggleCollisions(bool isEnabled)
	{
		foreach (var segment in m_segments)
		{
			segment.ToggleCollider(isEnabled);
		}
	}

	public void ToggleInteractables(bool isEnabled)
	{
		foreach (var segment in m_segments)
		{
			segment.ToggleTrigger(isEnabled);
		}
	}
}