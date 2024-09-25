using UnityEngine;

public class PreRope : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private MeshRenderer _meshRenderer;

	[Header("Scriptable references")]
	[SerializeField] private RSO_PlayeGraphicsDirection _rsoPlayerTransform;
	[SerializeField] private RopeConfig _ropeConfig;

	private bool _isPlaceable = true;
	private Camera _camera;
	private Transform _lookDirection;

	private void Start()
	{
		_camera = Camera.main;
	}

	public void Initialize(Transform newLookDirection)
	{
		_lookDirection = newLookDirection;
	}

	private void Update()
	{
		UpdatePosition();
		CheckIfPlaceable();
	}

	private void UpdatePosition()
	{
		transform.position = _lookDirection.position + _lookDirection.forward * GetDistanceWithCamera();
		UpdateColor(CheckIfPlaceable());
	}

	private float GetDistanceWithCamera()
	{
		// get camera angle & clamp
		float angle = _camera.transform.rotation.eulerAngles.x;
		if (angle > 80 || angle < _ropeConfig.minCameraAngle) angle = _ropeConfig.minCameraAngle;
		else if (angle > _ropeConfig.maxCameraAngle) angle = _ropeConfig.maxCameraAngle;

		// convert camera angle value to distance from player value
		return _ropeConfig.maxDistFromPlayer - ((angle - _ropeConfig.minCameraAngle) * (_ropeConfig.maxDistFromPlayer - _ropeConfig.minDistFromPlayer) / (_ropeConfig.maxCameraAngle - _ropeConfig.minCameraAngle));
	}

	public Rope InstantiateRope()
	{
		if (!_isPlaceable) return null;

		Rope newRope = Instantiate(_ropeConfig.pfRope, transform.position, _lookDirection.rotation);
		newRope.Initialize();
		return newRope;
	}

	private bool CheckIfPlaceable()
	{
		// cast 1 = Check if there is ground under the ladder 
		// cast 2 = Check that there is enough room above
		return Physics.Raycast(transform.position + new Vector3(0, 0.25f, 0), Vector3.down, 0.5f) &&
			!Physics.Raycast(transform.position + new Vector3(0, 0.25f, 0), Vector3.up, _ropeConfig.maxHeight - 0.25f);
	}

	private void UpdateColor(bool newIsPlaceable)
	{
		if (newIsPlaceable == _isPlaceable) return;

		_isPlaceable = newIsPlaceable;
		if (!_isPlaceable)
		{
			_meshRenderer.material.SetFloat("_colorSwitch", 1f);
		}
		else
		{
			_meshRenderer.material.SetFloat("_colorSwitch", 0f);
		}
	}
}