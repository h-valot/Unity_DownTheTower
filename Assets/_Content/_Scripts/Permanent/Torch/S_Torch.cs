using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Torch : Permanent
{
    [Header("Internal References")]
    [SerializeField] private Light _light;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private TorchPointLight _torchPointLight;
    [SerializeField] private LineRenderer _aimPreview;

    [Header("Scriptable References")]
	[SerializeField] private TorchConfig _torchConfig;



    // ----- PRIVATE VARIABLES -----
    private bool _isActive = false;

    private void Start()
    {
        _light.intensity = _torchConfig.lightIntensity;
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        _isActive = true;
        _aimPreview.useWorldSpace = true;
    }

	public override void ToggleInHand()
    {
        if (!_isActive) return;

		if (_light.enabled) 
		{
			StartCoroutine(SetMaterial(_torchConfig.extinguishDuration, _torchConfig.unlitMaterial));
		}
		else
		{
			StartCoroutine(SetMaterial(_torchConfig.lightStartupDuration, _torchConfig.litMaterial));
		}
    }

    public override void PreviewThrow(Transform _cameraTransform)
    {
        _aimPreview.enabled = true;
        _aimPreview.positionCount = Mathf.CeilToInt(_torchConfig.previewLength / _torchConfig.previewSmoothing) + 1;

        // set up starting point and velocity
        Vector3 startPosition = transform.position;
        Debug.Log(CalculateLaunchForce(_cameraTransform).ToString());
        Vector3 startVelocity = Quaternion.AngleAxis(-CalculateThrowAngleOffset(_cameraTransform), _cameraTransform.right) * _cameraTransform.forward * CalculateLaunchForce(_cameraTransform);

        // placing points along the line renderer
        int i = 0;
        _aimPreview.SetPosition(i, startPosition);
        for (float time = 0; time < _torchConfig.previewLength; time += _torchConfig.previewSmoothing)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            // defines placement over time using gravity as an accelerator
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

            _aimPreview.SetPosition(i, point);

            if (CheckEndOfPreview(i, point)) return;
        }
    }

    public override bool Throw(Transform _cameraTransform)
    {
        if (!_isActive || !_torchConfig.canThrow) 
		{
			return false;
		}

        _torchPointLight.SetIsInHand(false);
        _aimPreview.enabled = false;


        gameObject.transform.parent = null;
		_rigidbody.constraints = RigidbodyConstraints.None;
		_rigidbody.velocity = Quaternion.AngleAxis(-CalculateThrowAngleOffset(_cameraTransform), _cameraTransform.right) * _cameraTransform.forward * CalculateLaunchForce(_cameraTransform);
		_isActive = false;

		StartCoroutine(WaitAndDestroyTorch(_torchConfig.groundedLightDuration));

        return true;
    }

    private float CalculateLaunchForce(Transform _cameraTransform)
    {
        return _torchConfig.minLaunchForce + 
            (Mathf.Clamp(SetUpCameraAngle(_cameraTransform), 0, _torchConfig.maxLaunchCameraAngle / 2) - _torchConfig.minLaunchCameraAngle) * 
            (_torchConfig.maxLaunchForce - _torchConfig.minLaunchForce) / 
            (_torchConfig.maxLaunchCameraAngle / 2 - _torchConfig.minLaunchCameraAngle);
    }

    private float CalculateThrowAngleOffset(Transform _cameraTransform)
    {
        // OLD WAY OF CALCULATING ANGLE OFFSET
        //return _torchConfig.maxThrowAngleOffset + 
        //    (Mathf.Clamp(SetUpCameraAngle(_cameraTransform), 60, 130) - 60) * 
        //    (_torchConfig.minThrowAngleOffset - _torchConfig.maxThrowAngleOffset) / 
        //    (130 - 60);
        float cameraAngle = SetUpCameraAngle(_cameraTransform);
        return (-(cameraAngle * cameraAngle) + _torchConfig.maxLaunchCameraAngle * cameraAngle) / 200;
    }



    private float SetUpCameraAngle(Transform _cameraTransform)
    {
        // setting up the camera angle from just the eulerAngle from a value going from 0 to the difference between min and max camera angle
        float cameraAngle = _cameraTransform.rotation.eulerAngles.x + 60;
        if (cameraAngle > 250) cameraAngle = cameraAngle - 360;
        // setting the inverse since we want the launch force to be highest when the camera is at its lowest
        return _torchConfig.maxLaunchCameraAngle - cameraAngle;
    }

    private IEnumerator SetMaterial(float duration, Material material)
    {
        yield return new WaitForSeconds(duration);
        _light.enabled = !_light.enabled;
        _meshRenderer.material = material;
    }

    private IEnumerator WaitAndDestroyTorch(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

    private bool CheckEndOfPreview(int pointNb, Vector3 pointPos)
    {
        Vector3 lastPosition = _aimPreview.GetPosition(pointNb - 1);
        if(Physics.Raycast(lastPosition, (pointPos - lastPosition).normalized, out var hit, (pointPos - lastPosition).magnitude, ~(_torchConfig.layersToIgnorePreview)))
        {
            _aimPreview.SetPosition(pointNb, hit.point);
            _aimPreview.positionCount = pointNb + 1;
            return true;
        }
        return false;
    }
}