using System.Collections;
using UnityEngine;

public class Torch : Permanent
{
    [Header("Internal References")]
    [SerializeField] private Light _light;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private TorchPointLight _torchPointLight;

	[Header("Scriptable References")]
	[SerializeField] private TorchConfig _torchConfig;

    // ----- PRIVATE VARIABLES -----
    private bool _isActive = false;

    private void Start()
    {
        _light.intensity = _torchConfig.lightIntensity;
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        _isActive = true;
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

    public override bool Throw(Transform _cameraTransform)
    {
        if (!_isActive || !_torchConfig.canThrow) 
		{
			return false;
		}

        _torchPointLight.SetIsInHand(false);


        gameObject.transform.parent = null;
		_rigidbody.constraints = RigidbodyConstraints.None;
		_rigidbody.velocity = _cameraTransform.forward * 10f;
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
}