using System.Collections;
using UnityEngine;

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
        Vector3 startVelocity = Quaternion.AngleAxis(-_torchConfig.throwAngleOffset, _cameraTransform.right) * _cameraTransform.forward * _torchConfig.launchForce;

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
		_rigidbody.velocity = Quaternion.AngleAxis(-_torchConfig.throwAngleOffset, _cameraTransform.right) * _cameraTransform.forward * _torchConfig.launchForce;
		_isActive = false;

		StartCoroutine(WaitAndDestroyTorch(_torchConfig.groundedLightDuration));

        return true;
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