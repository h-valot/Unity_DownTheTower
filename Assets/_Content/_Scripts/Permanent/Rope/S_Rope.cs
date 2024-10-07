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

	[Header("debug")]
	public bool isConnected;
	public float currentLength;
	public float holdLength;
	public List<Vector3> folds = new List<Vector3>();

	private bool _isPlaced;
	private ConfigurableJoint _characterJoint;

	#region default functions

	public void Update()
	{
		currentLength = GetTotalLength();
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
		transform.DOJump(deployPoint, 1f, 0, 0.3f);

		// rope custom initialization commands 
		_isPlaced = true;
	}

	#endregion

	#region rope managment

	public void Attach(ConfigurableJoint joint)
	{
		folds = new List<Vector3>() { _ropeAttach.position.CutDigits(2) };

		_characterJoint = joint;
		isConnected = true;
	}

	public void Detach()
	{
		_characterJoint.connectedBody = null;
		_characterJoint = null;
		isConnected = false;
	}

	/// <summary>
	/// 	check rope folding using raycasts
	/// </summary>
	public void HandleFolds()
	{
		// assert: character ref null
		if (_characterJoint == null) return;

		// add fold if a collider stands between the character and the last fold
		if (Physics.Linecast(_characterJoint.transform.position, folds[^1], out var addHit, ~_ropeConfig.foldLayerToIgnore))
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

		// remove the last fold from the list if there is no collider that stands between the character and the previous last fold
		if (folds.Count >= 2
			&& !Physics.Linecast(_characterJoint.transform.position, folds[^2], out var removeHit, ~_ropeConfig.foldLayerToIgnore))
		{
			holdLength = GetLastFoldCharaDistance() + (folds[^2] - folds[^1]).magnitude;
			folds.Remove(folds[^1]);
		}
	}

	/// <summary>
	/// 	update hold rope radius to be the distance between the character rope attach position and the last fold of the rope.
	/// 	only if allowed.
	/// </summary>
	/// <param name="isAllowed">is it allowed to update hold rope radius</param>
	public void UpdateHoldLength(bool isAllowed = true)
	{
		// assert: is it not allowed
		if (!isAllowed) return;

		holdLength = GetLastFoldCharaDistance();
	}

	/// <summary>
	/// 	current distance between the character's position and the base of the rope.
	/// </summary>
	public float GetTotalLength()
	{
		// assert: called before folds is initialized
		if (!_isPlaced) return 0;

		// assert: character ref null
		if (_characterJoint == null) return 0;

		float output = 0;
		for (int i = 0; i < folds.Count; i++)
		{
			Vector3 nextPosition = i + 1 >= folds.Count
				? _characterJoint.transform.position
				: folds[i + 1];

			output += (folds[i] - nextPosition).magnitude;
		}
		return output;
	}

	public float GetLastFoldCharaDistance()
	{
		// assert: called before folds is initialized
		if (!_isPlaced) return 0;

		// assert: character ref null
		if (_characterJoint == null) return 0;

		Debug.Log($"ROPE: folds[^1] = {folds[^1]} - _characterJoint.transform.position = {_characterJoint.transform.position}"
				+ $"\nrope length = {(folds[^1] - _characterJoint.transform.position).magnitude}");
		return (folds[^1] - _characterJoint.transform.position).magnitude;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		// assert: called before folds is initialized
		if (!_isPlaced) return;

		// assert: character ref null
		if (_characterJoint == null) return;

		Gizmos.color = Color.red;
		for (int i = 0; i < folds.Count; i++)
		{
			Gizmos.DrawLine(folds[i], i + 1 >= folds.Count ? _characterJoint.transform.position : folds[i + 1]);
		}
	}
#endif

	#endregion
}