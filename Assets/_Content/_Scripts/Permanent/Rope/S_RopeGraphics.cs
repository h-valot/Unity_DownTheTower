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
	private bool m_interactablesEnabled;
	private bool m_collisionsEnabled;
	private float m_colliderDiameter;
	private Vector3 m_lineDirection;
	private float m_lineLength;
	private int m_colliderAmount;
	private Vector3 m_cachedCurrentFold;
	private Gradient m_gradient = new Gradient();
	private GradientColorKey[] m_gradientColorKey = new GradientColorKey[3];
	private GradientAlphaKey[] m_gradientAlphaKey = new GradientAlphaKey[2];

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
		m_rope.OnAttached += OnPlaced;
	}

	private void OnDisable()
	{
		m_rope.OnDetached -= OnDetached;
		m_rope.OnAttached += OnPlaced;
	}

	private void HandlePoints()
	{
		m_lineDirection = (m_rope.CharacterPosition - m_rope.CurrentFold).normalized;
		m_lineLength = (m_rope.CharacterPosition - m_rope.CurrentFold).magnitude;
		m_colliderAmount = Mathf.FloorToInt(m_lineLength / m_colliderDiameter);

		// print($"m_lineDirection = {m_lineDirection} -- m_lineLength = {m_lineLength} -- m_colliderAmount = {m_colliderAmount} -- m_segments.Count = {m_segments.Count}");
		// - Instantiate new segment -
		if (m_colliderAmount > m_segments.Count)
		{
			if (m_segments.Count < 3)
			{
				// TODO - Third temporary point for bezier curves
				// return;
			}

			var newSegment = Instantiate(m_ssoRope.PfRopeSegment, m_segments[^1].transform);
			newSegment.OnInteractedWithRef += OnInteracted;

			// Add point at the new fold position
			if (m_cachedCurrentFold != m_rope.CurrentFold)
			{
				print("fold");
				newSegment.Freeze();
				newSegment.transform.position = m_rope.CurrentFold;
				m_cachedCurrentFold = m_rope.CurrentFold;
			}
			// Place in-between points
			else
			{
				print("in between");
				newSegment.transform.position = m_segments[^1].transform.position + m_lineDirection * (m_lineLength / m_colliderAmount);
			}

			newSegment.ToggleCollider(m_collisionsEnabled);
			newSegment.SetLimit(m_ssoRope.PfRopeSegment.ColliderRadius + 0.1f);
			m_segments.Add(newSegment);
		}

		// - Remove last segments to match the line -
		else if (m_colliderAmount < m_segments.Count && m_segments.Count > 1)
		{
			print("remove");
			Destroy(m_segments[^1].gameObject);
			m_segments.Remove(m_segments[^1]);
		}

		// - Connect all segments together -
		for (int i = 0; i < m_segments.Count; i++)
		{
			// Assertion
			if (m_segments[i].IsConnected) continue;

			if (i == m_segments.Count - 1)
			{
				m_segments[i].ToggleSpring(true);
				m_segments[i].SetLimit(m_ssoRope.PfRopeSegment.ColliderRadius * 2);
				m_segments[i].Connect(m_rope.CharacterRigidbody);
				continue;
			}

			m_segments[i].ToggleSpring(false);
			m_segments[i].Connect(m_segments[i + 1].Rigidbody);
		}
	}

	private void DrawRope()
	{
		// Generate smoothed points using a Bezier curve
		if (m_segments.Select(s => s.transform.position).ToList().Count < 3) return;
		Vector3[] smoothedPoints = LineSmoother.SmoothLine(m_segments.Select(s => s.transform.position).ToArray(), m_ssoRope.LineSegmentSize);

		// Update line renderer settingg
		m_lineRenderer.positionCount = smoothedPoints.Length;
		m_lineRenderer.SetPositions(smoothedPoints);
		m_lineRenderer.startWidth = m_ssoRope.LineWidth;
		m_lineRenderer.endWidth = m_ssoRope.LineWidth;

		// Set colors and alphas
		// TODO - Make the gradient dynamic
		m_gradientColorKey[0] = new GradientColorKey(m_ssoRope.SafeColor, 0f);
		m_gradientColorKey[1] = new GradientColorKey(m_ssoRope.MidColor, 0.75f);
		m_gradientColorKey[2] = new GradientColorKey(m_ssoRope.DangerColor, 1f);
		m_gradientAlphaKey[0] = new GradientAlphaKey(1f, 0f);
		m_gradient.SetKeys(m_gradientColorKey, m_gradientAlphaKey);

		// Apply gradient to the line renderer
		m_lineRenderer.colorGradient = m_gradient;
	}

	public void SpawnSegments()
	{
		// Spawn segments
		// for (int i = 0; i < m_points.Count - 1; i++)
		// {
		// 	Vector3 lineDirection = (m_points[i + 1] - m_points[i]).normalized;
		// 	float lineLength = (m_points[i + 1] - m_points[i]).magnitude;
		// 	int colliderAmount = Mathf.FloorToInt(lineLength / m_colliderDiameter);
		// 	colliderAmount = Mathf.Clamp(colliderAmount, 1, colliderAmount);

		// 	for (int j = 0; j < colliderAmount; j++)
		// 	{
		// 		// Assert: The first segment of the first line must be ignored and replaced by the base repe segment.
		// 		if (j == 0 && i == 0) j++;

		// 		var newSegment = Instantiate(m_ssoRope.PfRopeSegment, j == 1 && i == 0 ? m_baseSegment.transform : m_segments[^1].transform);
		// 		newSegment.transform.rotation = Quaternion.LookRotation(lineDirection);
		// 		newSegment.OnInteractedWithRef += OnInteracted;

		// 		// Set the first segment of the line as static
		// 		if (j == 0)
		// 		{
		// 			newSegment.Freeze();
		// 			newSegment.transform.position = m_points[i];
		// 		}

		// 		// Snap the position of the last segment of the last line 
		// 		else if (j == colliderAmount - 1 && i == m_points.Count - 2)
		// 		{
		// 			newSegment.Free();
		// 			newSegment.transform.position = m_points[i] + lineDirection * (lineLength / colliderAmount) * j;
		// 		}

		// 		// Place in-between segments
		// 		else
		// 		{
		// 			newSegment.transform.position = m_points[i] + lineDirection * (lineLength / colliderAmount) * j;
		// 		}

		// 		newSegment.ToggleCollider(i == m_points.Count - 1);
		// 		newSegment.SetLimit(m_ssoRope.PfRopeSegment.ColliderRadius + 0.1f);
		// 		m_segments.Add(newSegment);
		// 	}
		// }

		// // Connect them together
		// m_baseSegment.Joint.connectedBody = m_segments[0].Rigidbody;
		// for (int i = 0; i < m_segments.Count; i++)
		// {
		// 	m_segments[^1].Connect(m_segments[^1].Rigidbody);
		// }
	}

	private void OnPlaced()
	{
		m_baseSegment.gameObject.SetActive(true);
		m_cachedCurrentFold = m_rope.CurrentFold;
	}

	private void OnInteracted(CharacterInteract characterInteract)
	{
		ToggleCollisions(false);
		ToggleInteractables(false);

		m_rope.Reattach(characterInteract);
	}

	private void OnDetached()
	{
		ToggleCollisions(true);
		ToggleInteractables(true);

		SpawnSegments();
	}

	public void ToggleCollisions(bool isEnabled)
	{
		m_collisionsEnabled = isEnabled;
		foreach (var segment in m_segments)
		{
			segment.ToggleCollider(isEnabled);
		}
	}

	public void ToggleInteractables(bool isEnabled)
	{
		m_interactablesEnabled = isEnabled;
		foreach (var segment in m_segments)
		{
			segment.ToggleTrigger(isEnabled);
		}
	}
}