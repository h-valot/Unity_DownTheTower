using System.Collections.Generic;
using System.Linq;
using EasyCurvedLine;
using Sirenix.OdinInspector;
using UnityEngine;

public class RopeGraphics : MonoBehaviour
{
	#region REFERENCES

	[FoldoutGroup("Internal references")][SerializeField] private Rope m_rope;
	[FoldoutGroup("Internal references")][SerializeField] private RopePhysic m_basePhysic;
	[FoldoutGroup("Internal references")][SerializeField] private RopeInteractable m_baseInteractable;
	[FoldoutGroup("Internal references")][SerializeField] private LineRenderer m_lineRenderer;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Rope m_ssoRope;

	#endregion

	#region VARIABLES

	private List<Vector3> m_points = new List<Vector3>();
	private List<RopePhysic> m_physics = new List<RopePhysic>();
	private List<RopeInteractable> m_interactables = new List<RopeInteractable>();

	private float m_colliderDiameter;
	private float m_triggerDiameter;

	private GradientAlphaKey[] m_gradientAlphaKey = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };
	private GradientColorKey[] m_gradientColorKey;
	private Gradient m_gradient = new Gradient();

	#endregion

	#region MONOBEHAVIOUR

	private void Start()
	{
		m_basePhysic.gameObject.SetActive(false);
		m_basePhysic.OnInteractedWithRef += OnInteracted;
		m_basePhysic.SphereCollider.radius = m_ssoRope.PhysicSphereRadius;
		m_basePhysic.SphereTrigger.radius = m_ssoRope.InteractableSphereRadius;

		m_baseInteractable.gameObject.SetActive(false);
		m_baseInteractable.OnInteractedWithRef += OnInteracted;
		m_baseInteractable.SphereTrigger.radius = m_ssoRope.InteractableSphereRadius;

		m_ssoRope.PfRopePhysic.SphereTrigger.radius = m_ssoRope.InteractableSphereRadius;
		m_ssoRope.PfRopePhysic.SphereCollider.radius = m_ssoRope.PhysicSphereRadius;
		m_ssoRope.PfRopeInteractable.SphereTrigger.radius = m_ssoRope.InteractableSphereRadius;
		m_triggerDiameter = m_ssoRope.InteractableSphereRadius * 2f;
		m_colliderDiameter = m_ssoRope.PhysicSphereRadius * 2f;
	}

	private void FixedUpdate()
	{
		// Assertion
		if (!m_rope.IsPlaced) return;

		HandlePoints();
	}

	private void LateUpdate()
	{
		// Assertion
		if (!m_rope.IsPlaced) return;

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

	#region GRAPHICS

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
			Physics.Raycast(middle, Vector3.down, out var RaycastHit);
			middle = RaycastHit.point.y > midOffset.y ? RaycastHit.point : midOffset;

			// Populate list
			m_points.AddUnique(start.CutDigits(2));
			m_points.AddUnique(middle.CutDigits(2));
			m_points.AddUnique(end.CutDigits(2));
		}
	}

	private void DrawRope()
	{
		// Generate smoothed points using a Bezier curve
		var points = new List<Vector3>();

		if (m_rope.IsConnected)
		{
			points = m_points.Duplicate();
		}
		else if (!m_rope.IsConnected && m_points.Count < 3)
		{
			points = m_physics.Select(s => s.transform.position).ToList();
		}
		else
		{
			points = m_points.Duplicate();
			int reducedPointAmount = points.Count - 2;
			for (int i = points.Count - 1; i >= reducedPointAmount; i--)
			{
				points.RemoveAt(i);
			}
			foreach (var segment in m_physics.Select(s => s.transform.position).ToList())
			{
				points.Add(segment);
			}
		}

		// Assert: SmoothLine function can't take less than 3 points
		if (points.Count < 3) return;

		Vector3[] smoothedPoints = LineSmoother.SmoothLine(points.ToArray(), m_ssoRope.LineSegmentSize);

		// Update line renderer settings
		m_lineRenderer.positionCount = smoothedPoints.Length;
		m_lineRenderer.SetPositions(smoothedPoints);
		m_lineRenderer.startWidth = m_ssoRope.LineWidth;
		m_lineRenderer.endWidth = m_ssoRope.LineWidth;

		// Set colors
		float lengthPercentage = Mathf.Clamp01(m_rope.GetTotalLength() / m_ssoRope.MaxLength);
		float midColorKeyTime = m_ssoRope.ropeGradient.colorKeys[1].time;

		if (lengthPercentage > midColorKeyTime)
		{
			// Handle mid color key
			m_gradientColorKey = new GradientColorKey[3];
			m_gradientColorKey[0] = new GradientColorKey(m_ssoRope.ropeGradient.Evaluate(0f), 0f);
			m_gradientColorKey[1] = new GradientColorKey(m_ssoRope.ropeGradient.Evaluate(midColorKeyTime), midColorKeyTime);
			m_gradientColorKey[2] = new GradientColorKey(m_ssoRope.ropeGradient.Evaluate(lengthPercentage), 1f);
		}
		else
		{
			// Default gradient right before the mid orange shows up
			m_gradientColorKey = new GradientColorKey[2];
			m_gradientColorKey[0] = new GradientColorKey(m_ssoRope.ropeGradient.Evaluate(0f), 0f);
			m_gradientColorKey[1] = new GradientColorKey(m_ssoRope.ropeGradient.Evaluate(lengthPercentage), 1f);
		}

		m_gradient.SetKeys(m_gradientColorKey, m_gradientAlphaKey);

		// Apply gradient to the line renderer
		m_lineRenderer.colorGradient = m_gradient;
	}

	private void SpawnPhysics()
	{
		// Determine how many physic segment needs to be instantiate
		Vector3 lineDirection = (m_rope.Folds[^1] - m_rope.Folds[^2]).normalized;
		float lineLength = (m_rope.Folds[^1] - m_rope.Folds[^2]).magnitude;
		int physicsAmount = Mathf.FloorToInt(lineLength / (m_colliderDiameter + m_ssoRope.PhysicJointOffset));
		physicsAmount = Mathf.Clamp(physicsAmount, 1, physicsAmount);

		// Populate physics
		for (int i = 0; i < physicsAmount; i++)
		{
			var newPhysic = Instantiate(m_ssoRope.PfRopePhysic, i == 0 ? m_basePhysic.transform : m_physics[^1].transform);
			newPhysic.transform.rotation = Quaternion.LookRotation(lineDirection);
			newPhysic.transform.position = m_rope.Folds[^2] + lineDirection * (lineLength / physicsAmount) * i;
			newPhysic.OnInteractedWithRef += OnInteracted;

			newPhysic.ToggleFree(i == physicsAmount - 1);
			newPhysic.SetLimit(m_colliderDiameter + m_ssoRope.PhysicJointOffset);

			m_physics.Add(newPhysic);
		}

		// Connect them together
		m_basePhysic.Connect(m_physics[0].Rigidbody);
		for (int i = 0; i < m_physics.Count - 1; i++)
		{
			m_physics[i].Connect(m_physics[i + 1].Rigidbody);
		}
	}


	private void SpawnInteractables()
	{
		for (int i = 0; i < m_rope.Folds.Count - 2; i++)
		{
			Vector3 lineDirection = (m_rope.Folds[i + 1] - m_rope.Folds[i]).normalized;
			float lineLength = (m_rope.Folds[i + 1] - m_rope.Folds[i]).magnitude;
			int interactableAmount = Mathf.FloorToInt(lineLength / m_triggerDiameter);
			interactableAmount = Mathf.Clamp(interactableAmount, 1, interactableAmount);

			for (int j = 0; j < interactableAmount; j++)
			{
				// Assert: The first interactable of the first line must be ignored and replaced by the base rope interactable.
				if (j == 0 && i == 0) j++;

				var newInteractable = Instantiate(m_ssoRope.PfRopeInteractable, j == 1 && i == 0 ? m_baseInteractable.transform : m_interactables[^1].transform);
				newInteractable.transform.rotation = Quaternion.LookRotation(lineDirection);
				newInteractable.transform.position = m_rope.Folds[i] + lineDirection * (lineLength / interactableAmount) * j;
				newInteractable.OnInteractedWithRef += OnInteracted;
				m_interactables.Add(newInteractable);
			}
		}
	}

	private void OnAttached()
	{
		m_basePhysic.gameObject.SetActive(false);
		m_baseInteractable.gameObject.SetActive(false);
	}

	private void OnDetached()
	{
		m_basePhysic.gameObject.SetActive(true);
		m_baseInteractable.gameObject.SetActive(true);
		m_basePhysic.transform.position = m_rope.Folds[^2];
		m_basePhysic.SetLimit(m_colliderDiameter + 0.1f);

		SpawnPhysics();
		SpawnInteractables();
	}

	private void OnInteracted(CharacterInteract characterInteract)
	{
		m_rope.Reattach(characterInteract);

		// Remove and destroy interactables  
		characterInteract.Remove(m_basePhysic);
		foreach (var physic in m_physics)
		{
			characterInteract.Remove(physic);
			Destroy(physic.gameObject);
		}
		m_physics.Clear();

		// Remove and destroy physics
		characterInteract.Remove(m_baseInteractable);
		foreach (var interactable in m_interactables)
		{
			characterInteract.Remove(interactable);
			Destroy(interactable.gameObject);
		}
		m_interactables.Clear();
	}

	#endregion
}