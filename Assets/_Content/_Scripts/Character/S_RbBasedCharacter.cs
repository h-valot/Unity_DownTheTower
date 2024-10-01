using UnityEngine;
using NaughtyAttributes;
using UnityEditor.Rendering;

public class RbBasedCharacter : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private Transform _cameraDirection;
	[SerializeField] private Transform _characterDirection;
	[SerializeField] private Rigidbody _rb;
	public ConfigurableJoint configurableJoint;

	[Header("Scriptable references")]
	[SerializeField] private CharacterConfig _characterConfig;
	[SerializeField] private RopeConfig _ropeConfig;
	[SerializeField] private RSO_CharacterForward _rsoCharacterForward;
	[SerializeField] private RSO_CharacterPosition _rsoCharacterPosition;
	[SerializeField] private RSO_PlayerDeath _rsoPlayerDeath;
	[SerializeField] private RSE_Sprint _rseSprint;
	[SerializeField] private RSE_Look _rseLook;
	[SerializeField] private RSE_Move _rseMove;
	[SerializeField] private RSE_Jump _rseJump;

	[Header("debug: move")]
	[ReadOnly] public Vector2 _moveInput;
	[ReadOnly] public Vector3 _velocity;
	[ReadOnly] public bool _isSprinting;
	[ReadOnly] public bool _isJumping;

	[Header("debug: permanent")]
	[ReadOnly] public float _ropeLength;

	private PreRope _currentPreRope;
	private Rope _lastInstantiatedRope;
	private Rope _equippedRope;

	private const float RIGIDBODY_FORCE_SCALAR = 10f;

	private void Update()
	{
		// temp
		HandleInputs();

		if (_equippedRope)
		{
			ForceAnchorPosition();
			HandleRopeLength();
		}
	}

	private void FixedUpdate()
	{
		// physics-based calculations
		HandleMovement();
	}

	private void OnEnable()
	{
		_rseMove.action += Move;
		_rseJump.action += Jump;
		_rseSprint.action += Sprint;
	}

	private void OnDisable()
	{
		_rseMove.action -= Move;
		_rseJump.action -= Jump;
		_rseSprint.action -= Sprint;
	}

	private void Move(Vector2 input)
	{
		_moveInput = input;
	}

	private void Jump()
	{
		// the square root of H * -2 * G = how much velocity needed to reach desired height
		Vector3 verticalForce = new Vector3(0, Mathf.Sqrt(_characterConfig.jumpHeight * -2f * _characterConfig.gravity), 0);

		_rb.AddForce(verticalForce * RIGIDBODY_FORCE_SCALAR, ForceMode.Impulse);
	}

	private void Sprint(bool isSprinting)
	{
		_isSprinting = isSprinting;
	}

	private void HandleInputs()
	{
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			_currentPreRope = Instantiate(_ropeConfig.pfPreRope);
		}

		if (Input.GetKeyUp(KeyCode.Mouse0))
		{
			_lastInstantiatedRope = _currentPreRope.InstantiateRope();
			_lastInstantiatedRope.Attach(configurableJoint);
			_equippedRope = _lastInstantiatedRope;

			// destroy rope previsualization
			if (_currentPreRope != null)
			{
				Destroy(_currentPreRope.gameObject);
				_currentPreRope = null;
			}
		}

		if (Input.GetKeyDown(KeyCode.L)
			&& _equippedRope != null)
		{
			_equippedRope.Detach();
			_equippedRope = null;
		}
	}

	private void HandleMovement()
	{
		Vector3 direction = (
			new Vector3(_cameraDirection.forward.x, 0, _cameraDirection.forward.z) * _moveInput.y 
			+ new Vector3(_cameraDirection.right.x, 0, _cameraDirection.right.z) * _moveInput.x).normalized;

		// _rb.AddForce(direction.normalized * _characterConfig.walkSpeed * RIGIDBODY_FORCE_SCALAR, ForceMode.Force);

		Vector3 desiredVelocity = direction * _characterConfig.walkSpeed;
		_rb.AddForce(desiredVelocity - _rb.velocity, ForceMode.Acceleration);
		_velocity = _rb.velocity;


		// update rso
		if (_rsoCharacterPosition.value != _characterDirection.position) { _rsoCharacterPosition.value = _characterDirection.position; }
		if (_rsoCharacterForward.value != _characterDirection.forward) { _rsoCharacterForward.value = _characterDirection.forward; }
	}

	private void HandleRopeLength()
	{
		if (Physics.Linecast(configurableJoint.transform.position, _equippedRope.folds[^1], out var addHit, ~_ropeConfig.foldLayerToIgnore))
		{
			Vector3 approximatePoint = addHit.point.CutDigits(2);

			if (_equippedRope.folds.Count >= 2)
			{
				// minimal distance between two fold point to be register
				if ((_equippedRope.folds[^1] - _equippedRope.folds[^2]).magnitude >= _ropeConfig.foldMinimalDistance)
				{
					_equippedRope.folds.AddUnique(approximatePoint);
				}
			}
			else
			{
				_equippedRope.folds.AddUnique(approximatePoint);
			}
		}

		if (_equippedRope.folds.Count >= 2
			&& !Physics.Linecast(configurableJoint.transform.position, _equippedRope.folds[^2], out var removeHit, ~_ropeConfig.foldLayerToIgnore))
		{
			_equippedRope.folds.Remove(_equippedRope.folds[^1]);
		}
	}

	private void ForceAnchorPosition()
	{
		Vector3 desiredAnchorPosition =
			_rsoCharacterPosition.value
			+ (_rsoCharacterForward.value.normalized * 0.5f)
			+ (Vector3.up * 0.5f);

		configurableJoint.transform.position = desiredAnchorPosition;
		configurableJoint.transform.rotation = Quaternion.LookRotation((desiredAnchorPosition - _equippedRope.folds[^1]).normalized);
	}
}