using UnityEngine;

public class Pendulum : MonoBehaviour
{
	[Header("Tweakable values")]
	public float m_Mass = 1f;
	public float m_GravityMagnitude = 9.81f;
	public float m_Drag = 0.03f;

	[Header("Internal references")]
	public Transform m_Pivot;
	public CharacterController m_Bob;

	[Header("Debug")]
	public Vector3 m_TensionDirection;
	public float m_TensionForce = 0f;
	public Vector3 m_Velocity = new Vector3();

	// Gizmo related variables
	private float m_RopeLength = 2f;
	private Vector3 m_BobStartingPosition;
	private float m_GravityForce = 0f;

	// Smooth update related variables
	private float m_FIXED_DELTA_TIME = 0.01f;

	private void Start()
	{
		m_BobStartingPosition = m_Bob.transform.position;
		m_RopeLength = Vector3.Distance(m_Pivot.position, m_Bob.transform.position);
		m_Velocity = Vector3.zero;
		m_FIXED_DELTA_TIME = Time.fixedDeltaTime;
	}

	private void FixedUpdate()
	{
		// Add gravity free fall
		m_GravityForce = m_Mass * m_GravityMagnitude;

		// Apply the gravity to `m_CurrentVelocity`
		m_Velocity += Vector3.down * m_GravityForce * m_FIXED_DELTA_TIME;

		// Cache pivot and bob positions
		Vector3 pivotPositionCache = m_Pivot.position;
		Vector3 bobPositionCache = m_Bob.transform.position;

		// Get bob's position after applying gravity force
		Vector3 auxiliaryMovementDelta = m_Velocity * m_FIXED_DELTA_TIME;
		float distanceAfterGravity = Vector3.Distance(pivotPositionCache, bobPositionCache + auxiliaryMovementDelta);

		// The bob acceleration is mesured in this statement. Returning an updated `m_CurrentVelocity`
		if (distanceAfterGravity > m_RopeLength
		|| Mathf.Approximately(distanceAfterGravity, m_RopeLength))
		{
			m_TensionDirection = (pivotPositionCache - bobPositionCache).normalized;

			// The nearest the bob is from the vertical point, the greatest the tension force will be.
			float inclinationAngle = Vector3.Angle(bobPositionCache - pivotPositionCache, Vector3.down);
			m_TensionForce = m_GravityForce * Mathf.Cos(Mathf.Deg2Rad * inclinationAngle);

			// Generate the counter force to make the bob stay within the circle: centripetal force
			float centripetalForce = m_Mass * Mathf.Pow(m_Velocity.magnitude, 2) / m_RopeLength;
			m_TensionForce += centripetalForce;

			// Apply the tension to `m_CurrentVelocity`
			m_Velocity += m_TensionDirection * m_TensionForce * m_FIXED_DELTA_TIME;
		}

		// Apply a counter velocity force: a drag
		m_Velocity -= m_Velocity * (m_Drag / m_GravityForce);

		// Apply velocity
		m_Bob.Move(m_Velocity * m_FIXED_DELTA_TIME);
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
		Vector3 auxilaryVelocity = .3f * m_Velocity;
		Gizmos.DrawRay(m_Bob.transform.position, auxilaryVelocity);
		Gizmos.DrawSphere(m_Bob.transform.position + auxilaryVelocity, .2f);

		// Yellow: Gravity
		Gizmos.color = new Color(1f, 1f, .2f);
		Vector3 gravity = .3f * m_GravityForce * Vector3.down;
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