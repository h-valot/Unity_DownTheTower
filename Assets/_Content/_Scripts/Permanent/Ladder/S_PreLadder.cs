using UnityEngine;

public class PreLadder : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private MeshRenderer _meshToColor;

	[Header("Scriptable references")]
	[SerializeField] private RSO_CharacterPosition _rsoPlayerTransform;
	[SerializeField] private LadderConfig _ladderConfig;

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
        if (angle > 80 || angle < _ladderConfig.minCameraAngle) angle = _ladderConfig.minCameraAngle;
        else if (angle > _ladderConfig.maxCameraAngle) angle = _ladderConfig.maxCameraAngle;

        // convert camera angle value to distance from player value
        return _ladderConfig.maxDistFromPlayer - ((angle - _ladderConfig.minCameraAngle) * (_ladderConfig.maxDistFromPlayer - _ladderConfig.minDistFromPlayer) / (_ladderConfig.maxCameraAngle - _ladderConfig.minCameraAngle));
    }

    public void InstantiateLadder()
    {
        if (!_isPlaceable) return;

		Ladder newLadder = Instantiate(_ladderConfig.pfLadder, transform.position, _lookDirection.rotation);
		newLadder.Initialize();
    }

    private bool CheckIfPlaceable()
    {
        // cast 1 = Check if there is ground under the ladder 
		// cast 2 = Check that there is enough room above
        return Physics.Raycast(transform.position + new Vector3(0, 0.25f, 0), Vector3.down, 0.5f) &&
            !Physics.Raycast(transform.position + new Vector3(0, 0.25f, 0), Vector3.up, _ladderConfig.maxHeight - 0.25f);
    }

    private void UpdateColor(bool newIsPlaceable)
    {
        if (newIsPlaceable == _isPlaceable) return;

		_isPlaceable = newIsPlaceable;
		if(!_isPlaceable)
		{
			_meshToColor.material.SetFloat("_colorSwitch", 1f);
		}
		else
		{
			_meshToColor.material.SetFloat("_colorSwitch", 0f);
		}
    }
}
