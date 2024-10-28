using UnityEngine;

public class Pendulum : MonoBehaviour
{
	public GameObject m_Pivot;
	public GameObject m_Bob;
	public float m_Mass = 1f;

	[Header("Debug")]
	public float m_Alpha;

	private float m_RopeLength = 2f;
	private Vector3 m_BobStartingPosition;

	// You could define these in the `PendulumUpdate()` loop 
	// But we want them in the class scope so we can draw gizmos `OnDrawGizmos()`
	private Vector3 m_GravityDirection;
	private Vector3 m_TensionDirection;

	private Vector3 m_TangentDirection;
	private Vector3 m_PendulumSideDirection;

	private float m_TensionForce = 0f;
	private float m_GravityForce = 0f;

	// Keep track of the current velocity
	private Vector3 m_CurrentVelocity = new Vector3();

	// We use these to smooth between values in certain framerate situations in the `Update()` loop
	private Vector3 m_CurrentPosition;
	private Vector3 m_PreviousPosition;

	private float m_CurrentTime = 0f;
	private float m_Accumulator = 0f;

	private const float m_FIXED_DELTA_TIME = 0.01f;

	private void Start()
	{
		// Set the starting position for later use in the context menu reset methods
		m_BobStartingPosition = m_Bob.transform.position;

		// Get the initial rope length from how far away the bob is now
		m_RopeLength = Vector3.Distance(m_Pivot.transform.position, m_Bob.transform.position);
		m_CurrentVelocity = Vector3.zero;

		// Set the transition state
		m_CurrentPosition = m_Bob.transform.position;
	}

	private void Update()
	{
		// Fixed deltaTime rendering at any speed with smoothing
		float frameTime = Time.time - m_CurrentTime;
		m_CurrentTime = Time.time;

		m_Accumulator += frameTime;
		while (m_Accumulator >= m_FIXED_DELTA_TIME)
		{
			m_Accumulator -= m_FIXED_DELTA_TIME;

			// Other logic
			m_PreviousPosition = m_CurrentPosition;
			m_CurrentPosition = PendulumUpdate();
		}
		m_Alpha = m_Accumulator / m_FIXED_DELTA_TIME;

		Vector3 newPosition = m_CurrentPosition * m_Alpha + m_PreviousPosition * (1f - m_Alpha);
		m_Bob.transform.position = newPosition;
	}

	private Vector3 PendulumUpdate()
	{
		// Add gravity free fall
		m_GravityForce = m_Mass * Physics.gravity.magnitude;
		m_GravityDirection = Physics.gravity.normalized;
		m_CurrentVelocity += m_GravityDirection * m_GravityForce * m_FIXED_DELTA_TIME;

		Vector3 pivotPosition = m_Pivot.transform.position;
		Vector3 bobPosition = m_CurrentPosition;

		Vector3 auxiliaryMovementDelta = m_CurrentVelocity * m_FIXED_DELTA_TIME;
		float distanceAfterGravity = Vector3.Distance(pivotPosition, bobPosition + auxiliaryMovementDelta);

		// If at the end of the rope - it is always the case on this demo
		if (distanceAfterGravity > m_RopeLength 
		|| Mathf.Approximately(distanceAfterGravity, m_RopeLength))
		{
			m_TensionDirection = (pivotPosition - bobPosition).normalized;

			m_PendulumSideDirection = Quaternion.Euler(0f, 90f, 0f) * m_TensionDirection;
			m_PendulumSideDirection.Scale(new Vector3(1f, 0f, 1f));
			m_PendulumSideDirection.Normalize();

			m_TangentDirection = (-1f * Vector3.Cross(m_TensionDirection, m_PendulumSideDirection)).normalized;

			float inclinationAngle = Vector3.Angle(bobPosition - pivotPosition, m_GravityDirection);

			m_TensionForce = m_Mass * Physics.gravity.magnitude * Mathf.Cos(Mathf.Deg2Rad * inclinationAngle);
			float centripetalForce = m_Mass * Mathf.Pow(m_CurrentVelocity.magnitude, 2) / m_RopeLength;
			m_TensionForce += centripetalForce;

			m_CurrentVelocity += m_TensionDirection * m_TensionForce * m_FIXED_DELTA_TIME;
		}

		// Get the movement delta
		Vector3 movementDelta = Vector3.zero;
		movementDelta += m_CurrentVelocity * m_FIXED_DELTA_TIME;

		float distance = Vector3.Distance(pivotPosition, m_CurrentPosition + movementDelta);
		return GetPointOnLine(pivotPosition, m_CurrentPosition + movementDelta, distance <= m_RopeLength ? distance : m_RopeLength);
	}

	private Vector3 GetPointOnLine(Vector3 start, Vector3 end, float distanceFromStart)
	{
		return start + (distanceFromStart * Vector3.Normalize(end - start));
	}

	private void OnDrawGizmos()
	{
		// Assert: references are null
		if (m_Pivot == null) return;
		if (m_Bob == null) return;

		// Purple: Bob & Pivot
		Gizmos.color = new Color(.5f, 0f, .5f);
		Gizmos.DrawWireSphere(m_Pivot.transform.position, m_RopeLength);
		Gizmos.DrawWireCube(m_BobStartingPosition, new Vector3(.5f, .5f, .5f));

		// Blue: Auxilary
		Gizmos.color = new Color(.3f, .3f, 1f);
		Vector3 auxilaryVelocity = .3f * m_CurrentVelocity;
		Gizmos.DrawRay(m_Bob.transform.position, auxilaryVelocity);
		Gizmos.DrawSphere(m_Bob.transform.position + auxilaryVelocity, .2f);

		// Yellow: Gravity
		Gizmos.color = new Color(1f, 1f, .2f);
		Vector3 gravity = .3f * m_GravityForce * m_GravityDirection;
		Gizmos.DrawRay(m_Bob.transform.position, gravity);
		Gizmos.DrawSphere(m_Bob.transform.position + gravity, .2f);

		// Orange: Tension
		Gizmos.color = new Color(1f, .5f, .2f);
		Vector3 tension = .3f * m_TensionForce * m_TensionDirection;
		Gizmos.DrawRay(m_Bob.transform.position, tension);
		Gizmos.DrawSphere(m_Bob.transform.position + tension, .2f);

		// Red: Resultant
		Gizmos.color = new Color(1f, .3f, .3f);
		Vector3 resultant = gravity + tension;
		Gizmos.DrawRay(m_Bob.transform.position, resultant);
		Gizmos.DrawSphere(m_Bob.transform.position + resultant, .2f);
	}
}