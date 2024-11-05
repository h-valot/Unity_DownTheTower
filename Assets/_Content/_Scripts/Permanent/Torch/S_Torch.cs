using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;

public class Torch : Permanent
{
    [Header("Internal References")]
    [SerializeField] private Light _light;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private TorchPointLight _torchPointLight;
    [SerializeField] private LineRenderer _aimPreview;
    [SerializeField] private Animator _brokenLightAnim;

    [Header("Scriptable References")]
	[SerializeField] private TorchConfig _torchConfig;
    [SerializeField] private CharacterConfig _characterConfig;
    [SerializeField] private RopeConfig _ropeConfig;
    [SerializeField] private RSO_CharacterPosition _characterPosition;

    // ----- PUBLIC VARIABLES -----
    [ReadOnly] public bool _isActive = false;

    // ----- PRIVATE VARIABLES -----
    private bool _isFalling = false;
    private bool _changedColor = false;
    private bool _isBroken = false;
    private bool _isHit = false;
    private float _throwStartPoint;
    private float _landedHeight = 9999999;

    #region monobehavior functions

    private void Start()
    {
        _light.color = _torchConfig.lightColor;
        _light.intensity = _torchConfig.lightIntensity;
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        _isActive = true;
        _aimPreview.useWorldSpace = true;
    }

    private void Update()
    {
        if (_isFalling && !_changedColor)
        {
            CheckLethalHeight();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_isFalling) return;
        _landedHeight = transform.position.y;
        CheckLethalRopeHeight();
        if (_changedColor && 
            !_isBroken &&
            !_isHit &&
            Vector3.Dot(collision.contacts[0].normal, new Vector3(0,1,0)) >= 0.8)
        {
            Instantiate(_torchConfig.torchHitSFX, transform.position, Quaternion.identity);
            _isHit = true;
        }
    }

    #endregion

    #region light

    /// <summary> Activate/Deactivate light on the torch </summary>
    public override void ToggleLight()
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

    /// <summary> Change the material from lit to unlit </summary>
    private IEnumerator SetMaterial(float duration, Material material)
    {
        yield return new WaitForSeconds(duration);
        _light.enabled = !_light.enabled;
        _meshRenderer.material = material;
    }

    #endregion

    #region preview

    /// <summary> Torch previsualisation with the camera's transform for the direction </summary>
    public override void PreviewThrow(Transform _cameraTransform)
    {
        _aimPreview.enabled = true;
        _aimPreview.positionCount = Mathf.CeilToInt(_torchConfig.previewLength / _torchConfig.previewSmoothing) + 1;

        // set up starting point and velocity
        Vector3 startPosition = transform.position;
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


    /// <summary> Stop the curve of the previsualisation if it collides with an object </summary>
    private bool CheckEndOfPreview(int pointNb, Vector3 pointPos)
    {
        Vector3 lastPosition = _aimPreview.GetPosition(pointNb - 1);
        if (Physics.Raycast(lastPosition, (pointPos - lastPosition).normalized, out var hit, (pointPos - lastPosition).magnitude, ~(_torchConfig.layersToIgnorePreview)))
        {
            _aimPreview.SetPosition(pointNb, hit.point);
            _aimPreview.positionCount = pointNb + 1;
            return true;
        }
        return false;
    }

    #endregion

    #region throwing
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
        _isFalling = true;
        _throwStartPoint = _characterPosition.value.y;

		StartCoroutine(WaitAndDestroyTorch(_torchConfig.groundedLightDuration));

        return true;
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

    private float CalculateLaunchForce(Transform _cameraTransform)
    {
        return _torchConfig.minLaunchForce +
            (Mathf.Clamp(SetUpCameraAngle(_cameraTransform), 0, _torchConfig.maxLaunchCameraAngle / 2) - _torchConfig.minLaunchCameraAngle) *
            (_torchConfig.maxLaunchForce - _torchConfig.minLaunchForce) /
            (_torchConfig.maxLaunchCameraAngle / 2 - _torchConfig.minLaunchCameraAngle);
    }

    private IEnumerator WaitAndDestroyTorch(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

    public override bool StateInHand()
    {
        return _isActive;
    }

    #endregion

    #region fall feedback

    private void CheckLethalHeight()
    {
        if (transform.position.y > _throwStartPoint ||
            (_landedHeight != 9999999 && Mathf.Round(_landedHeight) == Mathf.Round(transform.position.y))) return;

        if (_throwStartPoint - transform.position.y > _characterConfig.lethalHeight)
        {
            _light.DOColor(_torchConfig.deathColor, 0.5f);
            _changedColor = true;
        }
    }

    private void CheckLethalRopeHeight()
    {
        if (transform.position.y > _throwStartPoint || _isBroken) return;

        if (_throwStartPoint - transform.position.y > (_characterConfig.lethalHeight + _ropeConfig.maxLength))
        {
            _isBroken = true;
            Instantiate(_torchConfig.torchBreakSFX, transform.position, Quaternion.identity);

            if (_torchConfig.activateBreakAnim)
            {
                _brokenLightAnim.SetBool("isBroken", true);
                Destroy(gameObject, 3);
            }
            else Destroy(gameObject);
        }
    }

    #endregion

}