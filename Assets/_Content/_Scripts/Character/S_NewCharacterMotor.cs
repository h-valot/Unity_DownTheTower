using NaughtyAttributes;
using UnityEngine;

public class NewCharacterMotor : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private Transform _orientation;
	[SerializeField] private Rigidbody _rigidbody;

	[Header("Scriptable references")]
	[SerializeField] private NewCharacterConfig _characterConfig;
	[SerializeField] private RSO_PlayerDeath _rsoPlayerDeath;
	[SerializeField] private RSE_Sprint _rseSprint;
	[SerializeField] private RSE_Look _rseLook;
	[SerializeField] private RSE_Move _rseMove;
	[SerializeField] private RSE_Jump _rseJump;

	[Header("debug: move")]
	[ReadOnly] public float _moveSpeed;
	[ReadOnly] public Vector3 _moveInput;

	[Header("debug: status")]
	[ReadOnly] public bool _isJumping;
	[ReadOnly] public bool _isGrounded;
	[ReadOnly] public bool _isSprinting;

	// ----- PRIVATE VARIABLES -----
	// - input -
	private KeyCode _jumpKey = KeyCode.Space;

	// - move -
	private Vector3 _moveDirection;

	private void Start()
	{
		Initialize();
	}

	private void Update()
	{
		CheckGround();
		SpeedControl();
	}

	private void FixedUpdate()
	{
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

	private void Initialize()
	{
		_rigidbody.freezeRotation = true;
	}

	private void CheckGround()
	{
		_isGrounded = Physics.Raycast(transform.position, Vector3.down, _characterConfig.groundedRaycastLength);
	}

	private void SpeedControl()
	{
		_moveSpeed = _isSprinting ? _characterConfig.sprintSpeed : _characterConfig.walkSpeed;

		Vector3 flatVelocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);

		// limit velocity if needed
		if (flatVelocity.magnitude > _moveSpeed)
		{
			Vector3 limitedVelocity = flatVelocity.normalized * _moveSpeed;
			_rigidbody.velocity = new Vector3(limitedVelocity.x, _rigidbody.velocity.y, limitedVelocity.z);
		}
	}

	private void HandleDrag()
	{
		if (_isGrounded)
		{
			_rigidbody.drag = _characterConfig.groundDrag;
		}
		else
		{
			_rigidbody.drag = 0;
		}
	}

	private void HandleMovement()
	{
		// calculate movement direction
		_moveDirection = _orientation.forward * _moveInput.y + _orientation.right * _moveInput.x;

		// on ground
		if (_isGrounded)
		{
			_rigidbody.AddForce(_moveDirection.normalized * _moveSpeed * 10f, ForceMode.Force);
		}

		// in air
		else if (!_isGrounded)
		{
			_rigidbody.AddForce(_moveDirection.normalized * _moveSpeed * 10f * _characterConfig.airControlModifier, ForceMode.Force);
		}
	}

	private void Move(Vector2 input)
	{
		_moveInput = input;
	}

	private void Jump()
	{
		if (!_isGrounded
			|| _isJumping)
		{
			return;
		}

		_isJumping = true;

		// reset y velocity
		_rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);

		_rigidbody.AddForce(transform.up * _characterConfig.jumpForce, ForceMode.Impulse);

		Invoke(nameof(ResetJump), _characterConfig.jumpCooldown);

		// the square root of H * -2 * G = how much velocity needed to reach desired height
		// _gravityModifier.y = Mathf.Sqrt(_characterConfig.jumpHeight * -2f * _characterConfig.gravity);
	}

	private void ResetJump()
	{
		_isJumping = false;
	}

	private void Sprint(bool isSprinting)
	{
		_isSprinting = isSprinting;
	}

}