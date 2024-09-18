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
	[ReadOnly] public Vector3 _gravity;

	[Header("debug: status")]
	[ReadOnly] public bool _isJumping;
	[ReadOnly] public bool _isGrounded;
	[ReadOnly] public bool _isSprinting;

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

	private void CheckGround()
	{
		_isGrounded = Physics.Raycast(transform.position, Vector3.down, _characterConfig.groundedRaycastLength);
	}

	private void SpeedControl()
	{
		// - variables -
		Vector3 flatVelocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
		_moveSpeed = _isSprinting ? _characterConfig.sprintSpeed : _characterConfig.walkSpeed;

		// limit velocity if needed
		if (flatVelocity.magnitude > _moveSpeed)
		{
			Vector3 limitedVelocity = flatVelocity.normalized * _moveSpeed;
			_rigidbody.velocity = new Vector3(limitedVelocity.x, _rigidbody.velocity.y, limitedVelocity.z);
		}
	}

	private void HandleMovement()
	{
		// - variables -
		Vector3 moveDirection = _orientation.forward * _moveInput.y + _orientation.right * _moveInput.x;
		float speed;

		// - grounded -
		if (_isGrounded)
		{
			speed = _moveSpeed * 10f;
		}

		// - in air -
		else
		{
			speed = _moveSpeed * 10f * _characterConfig.airControlModifier;
		}

		// - move -
		_rigidbody.AddForce(moveDirection.normalized * speed, ForceMode.Force);
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

		// the square root of H * -2 * G = how much velocity needed to reach desired height
		_rigidbody.velocity = new Vector3(
			_rigidbody.velocity.x,
			Mathf.Sqrt(_characterConfig.jumpHeight * -2f * _characterConfig.gravity),   
			_rigidbody.velocity.z
		);

		_isJumping = false;
	}

	private void Sprint(bool isSprinting)
	{
		_isSprinting = isSprinting;
	}

}