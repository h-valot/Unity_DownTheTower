using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Rope : Permanent
{
	[Header("Internal references")]
	[SerializeField] private Transform _ropeAttach;
	[SerializeField] private MeshRenderer _previewMeshRendered;
	[SerializeField] private GameObject _previewGameObject;

	[Header("Scriptable references")]
	[SerializeField] private RopeConfig _ropeConfig;
	[SerializeField] private RSO_CharacterPosition _rsoCharacterPosition;

	[Header("Debug")]
	public bool isConnected;
	public bool isPlaced;
	[HideInInspector] public float holdLength;
	[HideInInspector] public List<Vector3> folds = new List<Vector3>();

	private List<RopeLine> _ropeLines = new List<RopeLine>();
	private Transform _characterHarness;

	#region default functions

	public void Update()
	{
		// asserts
		if (!isConnected) return;
		if (!isPlaced) return;

		HandleFolds();
		HandleEnd();
		DrawLines();
	}

	#endregion

	#region permanent & placement functions

	public override void InitializePreview()
	{
		_previewGameObject.SetActive(true);
		_previewGameObject.transform.rotation = Quaternion.identity;
	}

	public override void PreviewThrow(Transform cameraTransform)
	{
		if (Physics.Raycast(
			cameraTransform.position, 
			GetPositionRayDirection(cameraTransform, _ropeConfig.cameraOffsetAngle, _ropeConfig.maxCameraDownwardClamp), 
			out var hitInfo, 
			_ropeConfig.maxDistFromCamera, 
			~_ropeConfig.layersToIgnore))
		{
			if (!_previewGameObject.activeInHierarchy)
			{
				_previewGameObject.SetActive(true);
			}

			// update preview position
			_previewGameObject.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y + _ropeConfig.heightLimit / 2, hitInfo.point.z);

			UpdateColor(isDeployable: 
				IsGroundFlat(hitInfo, _ropeConfig.maxGroundAngle) 
				&& !IsCeiling(hitInfo, _ropeConfig.heightLimit) 
				&& !IsSpaceInFront(hitInfo, cameraTransform, _ropeConfig.minDistanceFromWall)
			);
		}
		else
		{
			UpdateColor(isDeployable: false);

			if (_previewGameObject.activeInHierarchy) 
			{
				_previewGameObject.SetActive(false);
			}
		}
	}

	private void UpdateColor(bool isDeployable)
	{
		_previewMeshRendered.material.SetFloat("_colorSwitch", isDeployable ? 1f : 0f);
	}

	public override bool Throw(Transform cameraTransform)
	{
		_previewGameObject.SetActive(false);

		if (Physics.Raycast(
			cameraTransform.position,
			GetPositionRayDirection(cameraTransform, _ropeConfig.cameraOffsetAngle, _ropeConfig.maxCameraDownwardClamp),
			out var hitInfo,
			_ropeConfig.maxDistFromCamera,
			~_ropeConfig.layersToIgnore))
		{
			if (IsGroundFlat(hitInfo, _ropeConfig.cameraOffsetAngle) 
				&& !IsCeiling(hitInfo, _ropeConfig.heightLimit) 
				&& !IsSpaceInFront(hitInfo, cameraTransform, _ropeConfig.minDistanceFromWall))
			{
				transform.SetParent(null, true);
				Deploy(cameraTransform, hitInfo.point);
				return true;
			}
		}
		
		return false;
	}

	private void Deploy(Transform cameraTransform, Vector3 deployPoint)
	{
		transform.eulerAngles = new Vector3(0, cameraTransform.rotation.eulerAngles.y, 0);
		transform.DOJump(deployPoint, 1f, 0, 0.3f).OnComplete(() =>
		{
			// rope custom initialization commands 
			folds = new List<Vector3>() { _ropeAttach.position.CutDigits(2) };
			isPlaced = true;
		});
	}

	#endregion

	#region rope managment

	public void Attach(Transform harness)
	{
		_characterHarness = harness;
		isConnected = true;
	}

	public void Detach()
	{
		_characterHarness = null;
		isConnected = false;
	}

	/// <summary>
	/// 	check rope folding using raycasts
	/// </summary>
	public void HandleFolds()
	{
		// assert: character ref null
		if (_characterHarness == null) return;

		// add fold if a collider stands between the character and the last fold
		if (Physics.Linecast(_characterHarness.position, folds[^1], out var addHit, ~_ropeConfig.foldLayerToIgnore))
		{
			Vector3 approximatePoint = addHit.point.CutDigits(2);

			if (folds.Count >= 2)
			{
				// minimal distance between two fold point to be register
				if ((folds[^1] - folds[^2]).magnitude >= _ropeConfig.minFoldDistance)
				{
					folds.AddUnique(approximatePoint, UpdateHoldLength);
				}
			}
			else
			{
				folds.AddUnique(approximatePoint, UpdateHoldLength);
			}
		}

		// Remove the last fold from the list if there is no collider 
		// that stands between the character and the previous last fold.
		if (folds.Count >= 2
		&& !Physics.Linecast(_characterHarness.position, folds[^2], out var removeHit, ~_ropeConfig.foldLayerToIgnore))
		{
			holdLength = GetLastFoldHarnessDistance() + (folds[^2] - folds[^1]).magnitude;
			folds.Remove(folds[^1]);
		}
	}

	/// <summary>
	/// 	detach the rope from the player if its total length is greater than the limit.
	/// </summary>
	private void HandleEnd()
	{
		// assert: total rope length is smaller than the max length
		if (GetTotalLength() <= _ropeConfig.maxLength) return;

		Detach();
	}

	/// <summary>
	/// 	update hold rope radius to be the distance between the character rope attach position and the last fold of the rope.
	/// 	only if allowed.
	/// </summary>
	/// <param name="isAllowed">is it allowed to update hold rope radius</param>
	public void UpdateHoldLength(bool isAllowed = true)
	{
		// Assert: is it not allowed
		if (!isAllowed) return;

		holdLength = GetLastFoldHarnessDistance();
	}

	/// <summary>
	/// 	current distance between the character's position and the base of the rope.
	/// </summary>
	public float GetTotalLength()
	{
		// Assertions
		if (!isPlaced) return 0;
		if (_characterHarness == null) return 0;

		float output = 0;
		for (int i = 0; i < folds.Count; i++)
		{
			Vector3 nextPosition = i + 1 >= folds.Count
				? _characterHarness.position
				: folds[i + 1];

			output += (folds[i] - nextPosition).magnitude;
		}
		return output;
	}

	public float GetLastFoldHarnessDistance()
	{
		// Assertions
		if (!isPlaced) return -1;
		if (_characterHarness == null) return -1;

		// Note that we do not connect the last fold to the harness
		// but the character's current position. This avoids re-centering
		// issue if spamming holding rope key
		return (folds[^1] - _rsoCharacterPosition.value).magnitude;
	}

	private void DrawLines()
	{
		// Assertions
		if (!isPlaced) return;
		if (_characterHarness == null) return;

		// Clear lists
		if (_ropeLines.Count >= 1)
		{
			for (int i = _ropeLines.Count - 1; i >= 0; i--)
			{
				Destroy(_ropeLines[i].gameObject);
			}
			_ropeLines = new List<RopeLine>();
		}

		// Get material based in the total distance
		Material material = _ropeConfig.dangerMaterial;
		if (GetTotalLength() <= _ropeConfig.maxLength / 2f)
		{
			material = _ropeConfig.safeMaterial;
		}
		else if (GetTotalLength() <= 3 * (_ropeConfig.maxLength / 4f))
		{
			material = _ropeConfig.midMaterial;
		}

		// Draw lines 
		for (int i = 0; i < folds.Count; i++)
		{
			RopeLine newRopeLine = Instantiate(_ropeConfig.pfRopeLine);
			newRopeLine.SetPositions(folds[i], i + 1 >= folds.Count ? _characterHarness.position : folds[i + 1]);
			newRopeLine.SetColor(material);
			_ropeLines.Add(newRopeLine);
		}
	}

	#endregion
}