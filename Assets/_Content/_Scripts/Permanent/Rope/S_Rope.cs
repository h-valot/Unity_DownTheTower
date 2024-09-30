using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private Transform _ropeAttach;
	[SerializeField] private RopeSegment _firstSegment;

	[Header("Scriptable references")]
	[SerializeField] private RopeConfig _ropeConfig;
	[SerializeField] private RSO_CharacterPosition _rsoCharacterPosition;

	[Header("debug: length")]
	public List<Vector3> _folds = new List<Vector3>();
	public List<RopeSegment> _segments = new List<RopeSegment>();

	// ----- PRIVATE VARIABLES -----
	private ConfigurableJoint _characterJoint;
	private bool _isInitialized;

	#region default functions

	public void Update()
	{
		if (!_isInitialized) return;
		if (_characterJoint == null) return;

		ExtendRope();
	}

	#endregion

	#region rope managment

	public void Initialize()
	{
		_folds = new List<Vector3>() { _ropeAttach.position.CutDigits(2) };
		_segments = new List<RopeSegment>() { _firstSegment };
		ExtendRope();

		_isInitialized = true;
	}

	/// <summary>
	/// 	instantiate new rope segments if the character is to far away
	/// </summary>
	public void ExtendRope()
	{
		if (GetBaseCharaDistance() >= _ropeConfig.maxLength) 
		{
			Detach();
			return;
		}

		float delta = GetBaseCharaDistance() - GetRopeLength();
		float instantiableSegment = delta / GetSegmentLength();
		int segmentToInstantiate = Mathf.Clamp(Mathf.FloorToInt(instantiableSegment), 0, _ropeConfig.maxSegmentInstantiatedPerFrame);
		for (int i = 0; i < segmentToInstantiate; i++)
		{
			Quaternion facingCharacter = Quaternion.LookRotation((_segments[^1].next.position - _rsoCharacterPosition.value).normalized);

			// instantiate a new segment
			RopeSegment newSegment = Instantiate(
				_ropeConfig.pfSegment,
				_segments[^1].next.position,
				facingCharacter,
				_segments[^1].transform
			);

			// connect joints together
			newSegment.Connect(_segments[^1].rb);
			_characterJoint.connectedBody = newSegment.rb;

			newSegment.name = $"PF_RopeSegment_{_segments.Count}";
			newSegment.Hide();
			_segments[^1].Show();

			// store the newly created segment
			_segments.Add(newSegment);
		}
	}

	public void Attach(ConfigurableJoint joint)
	{
		_characterJoint = joint;
	}

	public void Detach()
	{
		_segments[^1].Disconnect();
		_characterJoint.connectedBody = null;
		_characterJoint = null;
	}

	/// <summary>
	/// 	current distance between the character's position and the base of the rope.
	/// </summary>
	public float GetBaseCharaDistance()
	{
		float output = 0;

		// assert: called before folds is initialized
		if (!_isInitialized) return output;

		// assert: character ref null
		if (_characterJoint == null) return output;

		for (int i = 0; i < _folds.Count; i++)
		{
			Vector3 nextPosition = i + 1 >= _folds.Count
				? _rsoCharacterPosition.value
				: _folds[i + 1];

			output += (_folds[i] - nextPosition).magnitude;
		}

		return output;
	}

	/// <summary>
	/// 	current length of the rope.
	/// </summary>
	public float GetRopeLength()
	{
		return GetSegmentLength() * _segments.Count;
	}

	private float GetSegmentLength()
	{
		return _ropeConfig.pfSegment.capsuleCollider.height             // height of the segment collider
			- (2 * _ropeConfig.pfSegment.capsuleCollider.radius);       // top and bot offset that overlap with other segments
	}

	#endregion
}