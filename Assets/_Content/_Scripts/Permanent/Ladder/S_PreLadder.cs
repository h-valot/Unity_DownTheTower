using Unity.VisualScripting;
using UnityEngine;

public class PreLadder : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private MeshRenderer _placementMesh;

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
    }

    public void InstantiateLadder()
    {
        if (!_isPlaceable) return;

		Quaternion rotation = Quaternion.identity;
		rotation.eulerAngles = new Vector3(0, _camera.transform.rotation.eulerAngles.y, 0);
		Ladder newLadder = Instantiate(_ladderConfig.pfLadder, transform.position, rotation);
		newLadder.Initialize();
    }

	private void UpdatePosition()
	{
		if(Physics.Raycast(_camera.transform.position, _camera.transform.forward, out var hit, _ladderConfig.maxDistFromCamera))
        {
            _placementMesh.enabled = true;
			transform.position = hit.point;
			UpdateColor(IsGroundFlat(hit) && !IsCeiling());

        }
		else
		{
			_placementMesh.enabled = false;
		}
	}

	private bool IsGroundFlat(RaycastHit hit)
	{
		float product = Vector3.Dot(hit.normal, new Vector3(0, 1, 0));
		return (product >= _ladderConfig.maxGroundAngle);
	}


	private bool IsCeiling()
    {
        //Check that there is enough room above current position
        return Physics.Raycast(transform.position + new Vector3(0, 0.25f, 0), Vector3.up, _ladderConfig.maxHeight - 0.25f);
    }

    private void UpdateColor(bool newIsPlaceable)
    {
        if (newIsPlaceable == _isPlaceable) return;
        _isPlaceable = newIsPlaceable;
		if(!_isPlaceable)
		{
			_placementMesh.material.SetFloat("_colorSwitch", 1f);
		}
		else
		{
			_placementMesh.material.SetFloat("_colorSwitch", 0f);
		}
    }
}
