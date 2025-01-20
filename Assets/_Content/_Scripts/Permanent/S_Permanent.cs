using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

public class Permanent : MonoBehaviour
{
	[Title("Permanent settings")]
	public CraftType Type;

	private Vector3 GROUND_RAY_OFFSET = new Vector3(0.0f, 0.25f, 0.0f);

	/// <summary>
	/// 	call when the player presses the craft input.
	/// </summary>
	public virtual void InitializePreview() 
	{ 

	}

	/// <summary>
	/// 	call every frame in late update.
	/// </summary>
	/// <param name="cameraTransform"></param>
	public virtual void PreviewThrow(Transform cameraTransform) 
	{ 

	}

    /// <summary>
    /// 	disables the throw preview.
    /// </summary>
    public virtual void DisablePreview()
	{

	}

	/// <summary>
	/// 	called when the player presses the placement input.
	/// </summary>
	/// <param name="cameraTransform">transform of the camera</param>
	/// <returns>true if the permanent has be succesfully placed</returns>
	public virtual bool Throw(Transform cameraTransform) 
	{
		return false;
	}

	public virtual void ToggleHandEffect() 
	{ 

	}

    public virtual bool StateInHand()
    {
		return false;
    }

    /// <summary>
    /// 	
    /// </summary>
    /// <param name="cameraTransform"></param>
    /// <param name="cameraOffsetAngle"></param>
    /// <param name="maxCameraDownwardClamp"></param>
    /// <returns></returns>
    protected Vector3 GetPositionRayDirection(Transform cameraTransform, float cameraOffsetAngle, float maxCameraDownwardClamp)
	{
		Vector3 offsetRay = Quaternion.AngleAxis(cameraOffsetAngle, cameraTransform.right) * cameraTransform.forward;

		float angleDifference = Vector3.SignedAngle(
			new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized,
			offsetRay.normalized,
			cameraTransform.right
		);

		if (maxCameraDownwardClamp < angleDifference)
		{
			offsetRay = Quaternion.AngleAxis(
				cameraOffsetAngle - (angleDifference - maxCameraDownwardClamp) * 0.5f,
				cameraTransform.right
			).normalized * cameraTransform.forward;
		}

		return offsetRay;
	}

	/// <summary>
	/// Compares the normal vector of the ground with a up vector.
	/// </summary>
	/// <param name="hit">Raycast hit info</param>
	/// <param name="maxGroundAngle">Max dot product tolerated angle</param>
	/// <returns>True if the dot product angle is less than the given one.</returns>
	protected bool IsGroundFlat(RaycastHit hit, float maxGroundAngle)
	{
		return Vector3.Dot(hit.normal, Vector3.up) >= maxGroundAngle;
	}

	/// <summary>
	/// Check that there is enough space above the premanent preview position.
	/// </summary>
	/// <param name="hit">Raycast hit info</param>
	/// <param name="maxHeight">Max height tolerated</param>
	/// <returns>True if the raycast of a length equals to the given height do not touch a collider.</returns>
	protected bool IsCeiling(RaycastHit hit, float maxHeight, LayerMask maskToIgnore)
	{
		return Physics.Raycast(hit.point + GROUND_RAY_OFFSET, Vector3.up, maxHeight - GROUND_RAY_OFFSET.y, maskToIgnore);
	}

	/// <summary>
	/// Check that there is space around the permanent preview.
	/// </summary>
	/// <param name="hit">Raycast hit info</param>
	/// <param name="cameraTransform">Transform of the camera</param>
	/// <param name="radius">Minimum tolerated distance from the preview permanent and a collider around it</param>
	/// <returns>True if the space does not contains any collider.</returns>
	protected bool IsSpaceAround(RaycastHit hit, Transform cameraTransform, float radius, LayerMask maskToIgnore)
	{
        //return Physics.Raycast(hit.point + GROUND_RAY_OFFSET,
        //Vector3.Normalize(new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z)), minDistanceFromWall, maskToIgnore);
		return Physics.SphereCast(new Vector3(hit.point.x, hit.point.y + (radius / 2) + 0.1f, hit.point.z), radius, transform.forward, out RaycastHit hitInfo, 0, maskToIgnore);
	}
}