using System.Collections.Generic;
using Obi;
using UnityEngine;

public class Rope : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private ObiParticleAttachment _obiParticleCharacterAttachment;
	[SerializeField] private ObiRope _obiRope;

	[Header("Scriptable references")]
	[SerializeField] private RopeConfig _ropeConfig;

	[Header("debug: length")]
	public List<Vector3> folds;

	// ----- PRIVATE VARIABLES -----
	private CharacterMotor attachedCharacter;


	public void Initialize()
	{
		// initialize folds list
		folds = new List<Vector3>() { transform.position };

		// update the last particle group position
		_obiRope.blueprint.positions[^1] = new Vector3(_ropeConfig.maxLength, 0, 0);

		// Debug.Log($"ROPE: attachement = {_obiParticleCharacterAttachment.particleGroup}");
	}

	public void Update()
	{
		// HandleMoveAlong();
	}

	/// <summary>
	/// 	update particle group attached to the character's obi collider 
	/// </summary>
	public void HandleMoveAlong()
	{
		// exit, if there is no attached character
		if (attachedCharacter == null) return;

		float characterBaseLength = (transform.position - attachedCharacter.transform.position).magnitude;
		if (characterBaseLength <= _ropeConfig.maxLength)
		{
			// update particle group position the character is attached to
			// simply update on x-axis because _obiRope.blueprint.positions[0] = [0, 0, 0]
			// and _obiRope.blueprint.positions[^1] = [maxRopeLength, 0, 0]
			_obiRope.blueprint.positions[1] = new Vector3(characterBaseLength, 0, 0);
		}
	}

	/// <summary>
	/// 	attach the character obi collider to the obi particle attachement
	/// </summary>
	/// <param name="newCharacterMotor">source of the interaction</param>
	public void Interact(CharacterMotor source)
	{
		attachedCharacter = source;

		// attach the character obi collider to the obi particle attachement
		_obiParticleCharacterAttachment.target = attachedCharacter.obiCollider.transform;
	}

	public void Cancel()
	{
		attachedCharacter = null;

		// remove the obi particle attachement target
		_obiParticleCharacterAttachment.target = null;
	}

	public float GetLength()
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
}