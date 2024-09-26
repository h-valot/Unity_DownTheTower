using System.Collections;
using UnityEngine;

public class Torch : MonoBehaviour
{
    [Header("Internal References")]
    [SerializeField] private Light _light;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private MeshRenderer _meshRenderer;

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

	public void ToggleLight() 
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

    public void Throw(Vector3 direction)
    {
        if (!_isActive 
			|| !_torchConfig.canThrow) 
		{
			return;
		}

		gameObject.transform.parent = null;
		_rigidbody.constraints = RigidbodyConstraints.None;
		_rigidbody.velocity = direction * 10f;
		_isActive = false;

		StartCoroutine(WaitAndDestroyTorch(_torchConfig.groundedLightDuration));
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