using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Rope : MonoBehaviour, IInteractable
{
	[Header("Internal references")]
	[SerializeField] private Transform _ropeAttach;
	[SerializeField] private ConfigurableJoint _configurableJoint;

	[Header("Scriptable references")]
	[SerializeField] private RopeConfig _ropeConfig;

	[Header("debug: length")]
	public List<Vector3> folds = new List<Vector3>();
	public List<RopeSegment> segments = new List<RopeSegment>();

	// ----- PRIVATE VARIABLES -----
	private CharacterMotor attachedCharacter;
	private bool _isInitialized;

	// ----- PROPRIETIES -----
	/// <summary>
	/// 	current distance between the character's position and the base of the rope.
	/// </summary>
	public float baseCharaDistance => GetBaseCharaDistance();

	/// <summary>
	/// 	current length of the rope.
	/// </summary>
	public float ropeLength => GetRopeLength();

	// ----- DEFAULT FUNCTIONS -----
	public void Update()
	{
		if (!_isInitialized) return;

		ExtendRope();
	}

	public void Initialize()
	{
		// initialize folds list
		folds = new List<Vector3>() { _ropeAttach.position.CutDigits(2) };
		ExtendRope();

		_isInitialized = true;
	}

	public void ExtendRope()
	{
		if (attachedCharacter == null) return;

		if (baseCharaDistance >= _ropeConfig.maxLength) return;

		float delta = baseCharaDistance - ropeLength;
		float instantiableSegment = delta / GetSegmentLength();
		int segmentToInstantiate = Mathf.FloorToInt(instantiableSegment);

		if (segmentToInstantiate <= 0) return;

		for (int i = 0; i < segmentToInstantiate; i++)
		{
			// instantiate a new segment
			RopeSegment newSegment = Instantiate(
				_ropeConfig.pfSegment,
				segments[^1].top.position,
				segments[^1].transform.rotation,
				segments[^1].transform
			);
			newSegment.Connect(segments[^1].rb);
			attachedCharacter.configurableJoint.connectedBody = newSegment.rb;
			segments.Add(newSegment);

		}
	}

	/// <summary>
	/// 	attach the character obi collider to the obi particle attachement
	/// </summary>
	/// <param name="newCharacterMotor">source of the interaction</param>
	public void Interact(CharacterMotor source)
	{
		attachedCharacter = source;
	}

	public void Cancel()
	{
		attachedCharacter = null;
	}

	private float GetBaseCharaDistance()
	{
		float output = 0;

		for (int i = 0; i < folds.Count; i++)
		{
			Vector3 nextPosition = i + 1 >= folds.Count
				? attachedCharacter.transform.position
				: folds[i + 1];

			output += (folds[i] - nextPosition).magnitude;
		}

		return output;
	}

	private float GetRopeLength()
	{
		return GetSegmentLength() * segments.Count;
	}

	private float GetSegmentLength()
	{
		return _ropeConfig.pfSegment.capsuleCollider.height             // height of the segment collider
			- (2 * _ropeConfig.pfSegment.capsuleCollider.radius);       // top and bot offset that overlap with other segments
	}
}