using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : Permanent
{
    [Header("Internal References")]
    [SerializeField] private Light _light;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private LineRenderer _aimPreview;
    [SerializeField] private Transform _torchTop;
    [SerializeField] private Transform _pointLightBase;
    [SerializeField] private SphereCollider _lightCollider;

    [Header("External References")]
	[SerializeField] private TorchConfig _torchConfig;
    [SerializeField] private RSO_TorchManager _rsoTorchManager;
    [SerializeField] private CharacterConfig _characterConfig;
    [SerializeField] private RopeConfig _ropeConfig;
    [SerializeField] private RSO_CharacterPosition _rsoCharacterPosition;

    // ----- PUBLIC VARIABLES -----
    [ReadOnly] public bool _isInHand = false;

    // ----- PRIVATE VARIABLES -----
    private Vector3 _lastPosition;
    private LayerMask _layerMask;

    private MaterialPropertyBlock _propertyBlock;

    private bool _islit;
    private bool _hasChangedColor;
    private bool _HasPlayedHitSound;
    private bool _isDeactivate;

    private float _lightPercent;
    private Color _emitColor;

    #region monobehavior functions

    private void Awake()
    {
        _isInHand = true;
        _isDeactivate = false;
        _hasChangedColor = false;
        _HasPlayedHitSound = false;

        //Collisions
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        _lightCollider.radius = _torchConfig.lightOffsetDistance;
        _layerMask |= (1 << LayerMask.NameToLayer("Default"));
        _layerMask |= (1 << LayerMask.NameToLayer("Collision_NoRaycast"));

        //Preview
        _aimPreview.useWorldSpace = true;

        //Visual
        _propertyBlock = new MaterialPropertyBlock();
        _light.color = _torchConfig.baseColor;
        _light.intensity = _torchConfig.lightIntensity;
        _propertyBlock.SetColor("_lightColor", _torchConfig.baseColor);
        _meshRenderer.SetPropertyBlock(_propertyBlock);
    }

    private void Start()
    {
        if(_torchConfig.startLit)
        {
            _islit = true;
            _lightPercent = 1f;
            _propertyBlock.SetFloat("_lightPercent", _lightPercent);
            _meshRenderer.SetPropertyBlock(_propertyBlock);
            _torchTop.transform.localPosition = new Vector3(_torchTop.transform.localPosition.x, _torchConfig.topTorchOffsetDistance, _torchTop.transform.localPosition.z);
        }

        _rsoTorchManager.value.AddNewTorchToList(this);

        _rsoCharacterPosition.OnChanged += UpdateTorchFeedback;
    }

    private void Update()
    {
        if (!_isInHand && _islit)
        {
            if (HasMoved())
            {
                UpdateTorchFeedback();
            }
        }
    }

    private void LateUpdate()
    {
        // TODO: torch
        // [ ] Prevent the penetration test to fire if the torch is immobile.
        // [ ] Use trigger enter and exit to prevent penetration test when there is no collider in range.

        if (!_isInHand && _islit)
        {
            Vector3 _lightOffset = Vector3.zero;
            Collider[] _hitColliders = Physics.OverlapSphere(_pointLightBase.position, _torchConfig.lightOffsetDistance, _layerMask);
            if (_hitColliders.Length > 0)
            {
                foreach (Collider _otherCollider in _hitColliders)
                {
                    Vector3 otherPosition = _otherCollider.gameObject.transform.position;
                    Quaternion otherRotation = _otherCollider.gameObject.transform.rotation;

                    Vector3 direction;
                    float distance;

                    bool overlapped = Physics.ComputePenetration(
                        _lightCollider, transform.position, transform.rotation,
                        _otherCollider, otherPosition, otherRotation,
                        out direction, out distance
                    );

                    if (overlapped)
                    {
                        _lightOffset += direction * distance;
                    }

                    _lightOffset = Vector3.ClampMagnitude(_lightOffset, _torchConfig.lightOffsetDistance);
                    _light.transform.position = _pointLightBase.position + _lightOffset;

                }
            }
            else
            {
                _light.transform.position = _pointLightBase.position;
            }

        }
    }   

    private void OnCollisionEnter(Collision collision)
    {
        if (!_isInHand)
        {
            //if it collide with a flat surface it increase drag to prevent the torch from rolling for eternity
            if (Vector3.Dot(collision.contacts[0].normal, new Vector3(0, 1, 0)) >= 0.8)
            {
                _rigidbody.drag = 1f;
                _rigidbody.angularDrag = 1f;
            }
            if (!_HasPlayedHitSound && _rigidbody.velocity.magnitude > _torchConfig.minimalSpeedForHitSound)
            {
                _HasPlayedHitSound = true;
                if (_isDeactivate)
                {
                    Instantiate(_torchConfig.torchBreakSFX, transform.position, Quaternion.identity);
                    DOTween.Sequence().AppendInterval(_torchConfig.deactivatingTime).SetId(gameObject.GetInstanceID()).OnComplete(() => { _HasPlayedHitSound = false; });
                }
                else
                {
                    Instantiate(_torchConfig.torchHitSFX, transform.position, Quaternion.identity);
                    DOTween.Sequence().AppendInterval(_torchConfig.timeBetweenHitSound).SetId(gameObject.GetInstanceID()).OnComplete(() => { _HasPlayedHitSound = false; });
                }
            }
        }
    }

    #endregion

    #region light

    /// <summary> Activate/Deactivate light on the torch </summary>
    public override void ToggleInHand()
    {
        _light.enabled = false;
        if (!_isInHand) return;

		if (_islit) 
		{
            _islit = false;
            DOTween.Kill(gameObject.GetInstanceID() + "light");
            DOTween.To(() => _lightPercent, x => _lightPercent = x, 0f, _torchConfig.lightOffDuration).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "light")
                .OnUpdate(() =>
                {
                    _propertyBlock.SetFloat("_lightPercent", _lightPercent);
                    _meshRenderer.SetPropertyBlock(_propertyBlock);
                });
            _light.DOIntensity(0f, _torchConfig.lightOffDuration).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "light");
            _torchTop.DOLocalMoveY(0f, _torchConfig.lightOffDuration).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "light").OnComplete(() => { _light.enabled = false; });
        }
		else
		{
            _islit = true;
            _light.enabled = true;
            DOTween.Kill(gameObject.GetInstanceID() + "light");
            DOTween.To(() => _lightPercent, x => _lightPercent = x, 1f, _torchConfig.lightOffDuration).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "light")
               .OnUpdate(() =>
               {
                   _propertyBlock.SetFloat("_lightPercent", _lightPercent);
                   _meshRenderer.SetPropertyBlock(_propertyBlock);
               });
            _light.DOIntensity(_torchConfig.lightIntensity, _torchConfig.lightOnDuration).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "light");
            _torchTop.DOLocalMoveY(_torchConfig.topTorchOffsetDistance, _torchConfig.lightOnDuration).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "light");
        }
    }

    private bool HasMoved()
    {
        if (_lastPosition != transform.position)
        {
            _lastPosition = transform.position;
            return true;
        }
        else
        {
            return false;
        }
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
        if (!_isInHand || !_torchConfig.canThrow) 
		{
			return false;
		}

        _aimPreview.enabled = false;

        _lastPosition = transform.position;

        gameObject.transform.parent = null;
		_rigidbody.constraints = RigidbodyConstraints.None;
		_rigidbody.velocity = Quaternion.AngleAxis(-CalculateThrowAngleOffset(_cameraTransform), _cameraTransform.right) * _cameraTransform.forward * CalculateLaunchForce(_cameraTransform);
		_isInHand = false;

		StartCoroutine(WaitAndDeactivateTorch(_torchConfig.groundedLightDuration));

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

    private IEnumerator WaitAndDeactivateTorch(float duration)
    {
        yield return new WaitForSeconds(duration);
        _rsoTorchManager.value.RemoveTorchFromList(this);
        DeactivateTorch();
    }

    public override bool StateInHand()
    {
        return _isInHand;
    }

    #endregion

    #region fall feedback

    /// <summary>
    /// Calculate the difference in height between player and torch and update torch state based on that.
    /// </summary>
    private void UpdateTorchFeedback()
    {
        if (!_isInHand)
        {
            if (_rsoCharacterPosition.value.y - transform.position.y > _characterConfig.lethalHeight)
            {
                if (!_hasChangedColor)
                {
                    DOTween.Kill(gameObject.GetInstanceID() + "feedback");
                    _light.DOColor(_torchConfig.deathColor, 0.5f).SetId(gameObject.GetInstanceID() + "feedback");
                    DOTween.To(() => _emitColor, x => _emitColor = x, _torchConfig.deathColor, 0.5f).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "feedback")
                       .OnUpdate(() => {
                           _propertyBlock.SetColor("_lightColor", _emitColor);
                           _meshRenderer.SetPropertyBlock(_propertyBlock);
                       });
                    _hasChangedColor = true;
                }
            }
            else
            {
                if (_hasChangedColor)
                {
                    DOTween.Kill(gameObject.GetInstanceID() + "feedback");
                    _light.DOColor(_torchConfig.baseColor, 0.5f).SetId(gameObject.GetInstanceID() + "feedback");
                    DOTween.To(() => _emitColor, x => _emitColor = x, _torchConfig.baseColor, 0.5f).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() + "feedback")
                       .OnUpdate(() => {
                           _propertyBlock.SetColor("_lightColor", _emitColor);
                           _meshRenderer.SetPropertyBlock(_propertyBlock);
                       });
                    _hasChangedColor = false;
                }
            }
            if (_rsoCharacterPosition.value.y - transform.position.y > _characterConfig.lethalHeight + _ropeConfig.maxLength)
            {
                _rsoTorchManager.value.RemoveTorchFromList(this);
                DeactivateTorch();
            }
        }
    }

    /// <summary>
    /// Make torch flicker and 
    /// </summary>
    public void DeactivateTorch()
    {
        if (_torchConfig.activateBreakAnim)
        {
            _isDeactivate = true;
            _rsoCharacterPosition.OnChanged -= UpdateTorchFeedback;
            Sequence _deactivatingSequence = DOTween.Sequence().Pause();
            _deactivatingSequence.AppendInterval(0.03f);
            _deactivatingSequence.AppendCallback(() => { _light.enabled = false; });
            _deactivatingSequence.AppendInterval(0.08f);
            _deactivatingSequence.AppendCallback(() => { _light.enabled = true; });
            _deactivatingSequence.AppendInterval(0.03f);
            _deactivatingSequence.AppendCallback(() => { _light.enabled = false; });
            _deactivatingSequence.AppendInterval(0.03f);
            _deactivatingSequence.AppendCallback(() => { _light.enabled = true; });
            _deactivatingSequence.AppendInterval(0.03f);
            _deactivatingSequence.AppendCallback(() => { _light.enabled = false; });
            _deactivatingSequence.AppendInterval(0.03f);
            _deactivatingSequence.AppendCallback(() => { _light.enabled = true; });
            _deactivatingSequence.Insert(0f, _light.DOIntensity(0f, _torchConfig.deactivatingTime).SetEase(Ease.Linear));
            _deactivatingSequence.Insert(0f, DOTween.To(() => _light.range, x => _light.range = x, 0f, _torchConfig.deactivatingTime).SetEase(Ease.Linear));
            _deactivatingSequence.Insert(0f, DOTween.To(() => _lightPercent, x => _lightPercent = x, 0f, _torchConfig.deactivatingTime).SetEase(Ease.Linear)
                                                            .OnUpdate(() => {
                                                                _propertyBlock.SetFloat("_lightPercent", _lightPercent);
                                                                _meshRenderer.SetPropertyBlock(_propertyBlock);
                                                            }));
            _deactivatingSequence.SetId(gameObject.GetInstanceID());
            _deactivatingSequence.Play().OnComplete(() => { DestroyTorch(); });
        }
        else
        {
            DestroyTorch();
        }
    }

    private void DestroyTorch()
    {
        DOTween.Kill(gameObject.GetInstanceID() + "light");
        DOTween.Kill(gameObject.GetInstanceID() + "feedback");
        DOTween.Kill(gameObject.GetInstanceID());
        Destroy(gameObject);
    }

    #endregion

}