using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : Permanent
{
	[Header("Internal references")]
    [SerializeField] private GameObject _ladderMesh;
    [SerializeField] private GameObject _previewLadder;
    [SerializeField] private MeshRenderer _previewLadderRenderer;
    [SerializeField] private GameObject _TPTriggerTop;
    [SerializeField] private GameObject _TPTriggerBottom;
	[SerializeField] private GameObject _tippyTop;

    [Header("Scriptable references")]
	[SerializeField] private LadderConfig _ladderConfig;

    private Vector3 _bottomPos;
    private Vector3 _topHitPos;

    public override void InitializePreview()
    {
        _previewLadder.SetActive(true);
        _previewLadder.transform.localScale = new Vector3(
            _previewLadder.transform.localScale.x,
            _ladderConfig.maxHeight / 2,
            _previewLadder.transform.localScale.z
        );
        _previewLadder.transform.position = new Vector3(
            _previewLadder.transform.position.x,
            transform.position.y + (_ladderConfig.maxHeight / 2),
            _previewLadder.transform.position.z
        );
        _previewLadder.transform.rotation = Quaternion.identity;
    }

    //Call in Late Update
    public override void PreviewThrow(Transform _cameraTransform)
    {
        if (Physics.Raycast(_cameraTransform.position, GetPositionRayDirection(_cameraTransform), out var hit, _ladderConfig.maxDistFromCamera, ~(_ladderConfig.layersToIgnore)))
        {
            if (!_previewLadder.activeSelf) _previewLadder.SetActive(true);
            _previewLadder.transform.position = new Vector3(hit.point.x, hit.point.y + _ladderConfig.maxHeight / 2, hit.point.z);
            _previewLadder.transform.rotation = Quaternion.identity;
            UpdateColor(IsGroundFlat(hit) && !IsCeiling(hit) && !IsSpaceInFront(hit, _cameraTransform));
        }
        else
        {
            UpdateColor(false);
            if (_previewLadder.activeSelf) _previewLadder.SetActive(false);
        }
    }

    private Vector3 GetPositionRayDirection(Transform _cameraTransform)
    {
        Vector3 offsetRay = Quaternion.AngleAxis(_ladderConfig.cameraOffset, _cameraTransform.right) * _cameraTransform.forward;
        float angleDifference = Vector3.SignedAngle(new Vector3(_cameraTransform.forward.x, 0, _cameraTransform.forward.z).normalized, offsetRay.normalized, _cameraTransform.right);
        if (_ladderConfig.maxCameraDownwardClamp < angleDifference)
        {
            offsetRay = Quaternion.AngleAxis(_ladderConfig.cameraOffset - (angleDifference - _ladderConfig.maxCameraDownwardClamp) * 0.5f, _cameraTransform.right).normalized * _cameraTransform.forward;
        }

        return offsetRay;
    }

    private bool IsGroundFlat(RaycastHit hit)
    {
        float product = Vector3.Dot(hit.normal, new Vector3(0, 1, 0));
        return (product >= _ladderConfig.maxGroundAngle);
    }


    private bool IsCeiling(RaycastHit hit)
    {
        //Check that there is enough room above current position
        return Physics.Raycast(hit.point + new Vector3(0, 0.25f, 0), Vector3.up, _ladderConfig.maxHeight - 0.25f);
    }

    private bool IsSpaceInFront(RaycastHit hit, Transform _cameraTransform)
    {
        //Check that there is a little room in front of ladder
        return Physics.Raycast(hit.point + new Vector3(0, 0.25f, 0),
            Vector3.Normalize(new Vector3(_cameraTransform.forward.x, 0, _cameraTransform.forward.z)), _ladderConfig.minDistanceFromWall);
    }

    private void UpdateColor(bool _isDeployable)
    {
        if (!_isDeployable)
        {
            if (_previewLadderRenderer.material.GetFloat("_colorSwitch") != 1f) _previewLadderRenderer.material.SetFloat("_colorSwitch", 1f);
        }
        else
        {
            if (_previewLadderRenderer.material.GetFloat("_colorSwitch") != 0f) _previewLadderRenderer.material.SetFloat("_colorSwitch", 0f);
        }
    }

    public override bool Throw(Transform _cameraTransform)
    {
        _previewLadder.SetActive(false);

        if (Physics.Raycast(_cameraTransform.position, GetPositionRayDirection(_cameraTransform), out var hit, _ladderConfig.maxDistFromCamera, ~(_ladderConfig.layersToIgnore)))
        {
            if (IsGroundFlat(hit) && !IsCeiling(hit) && !IsSpaceInFront(hit, _cameraTransform))
            {
                transform.SetParent(null, true);
                Deploy(_cameraTransform, hit.point);
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public void Deploy(Transform _cameraTransform, Vector3 _deployPosition)
	{
        transform.eulerAngles = new Vector3(0, _cameraTransform.rotation.eulerAngles.y, 0);
        transform.DOJump(_deployPosition, 1f, 0, 0.3f).OnComplete(() =>
        {
            _bottomPos = transform.position;
            _tippyTop.transform.position = new Vector3(_bottomPos.x, _bottomPos.y + _ladderConfig.maxHeight, _bottomPos.z);
            _ladderMesh.transform.DOScaleY(_ladderConfig.maxHeight / 2, 1);
            _ladderMesh.transform.DOMoveY(transform.position.y + (_ladderConfig.maxHeight / 2), 1).OnComplete(Falling);
        });
	}

    private void Falling()
    {
        transform.DOLocalRotate(new Vector3(180,transform.rotation.eulerAngles.y,0), 2).OnUpdate(PlacementRay);
    }

	// Raycast aligned with the ladder, meant to stop the ladder from falling further once it collides with something.
	private void PlacementRay()
    {
        Vector3 originPos = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z);
        if (Physics.Raycast(originPos, transform.up, out var hit, _ladderConfig.maxHeight, ~(_ladderConfig.layersToIgnore)))
        {
            transform.DOKill();
            _topHitPos = hit.point;
            _tippyTop.transform.Rotate(- transform.rotation.eulerAngles.x, 0, 0);
            if (transform.rotation.eulerAngles.x <= _ladderConfig.minWalkableAngle && !ForwardRay())
            {
                SetUpTP();
            }
        }
    }

    // raycast that checks that there are no walls at the top of the ladder.
    private bool ForwardRay()
    {
        Vector3 originPos = _tippyTop.transform.position + (-_tippyTop.transform.forward * 0.5f) + new Vector3(0, _ladderConfig.additionalRaycastHeight, 0);
        return Physics.Raycast(originPos, _tippyTop.transform.forward, 1, ~(_ladderConfig.layersToIgnore));
    }

    private void SetUpTP()
    {
        // activate trigger zones for top and bottom of ladder
        _TPTriggerTop.SetActive(true);
        _TPTriggerBottom.SetActive(true);

        // pass on values to trigger zones (origin, destination, isBottom)
        _TPTriggerTop.GetComponent<LadderTP>().SetVariables(_topHitPos, _bottomPos - transform.forward);
        _TPTriggerBottom.GetComponent<LadderTP>().SetVariables(_bottomPos, PickBestTeleportPoint());
    }
    
    private Vector3 PickBestTeleportPoint()
    {
        List<Vector3> hitlist = new List<Vector3>();

        // center raycast
        CastRayAndAddToList(
			hitlist,
			_tippyTop.transform.position 
			+ new Vector3(0, _ladderConfig.additionalRaycastHeight, 0) 
			+ _tippyTop.transform.forward
		);

        // left raycast
        CastRayAndAddToList(hitlist, _tippyTop.transform.position + (-_tippyTop.transform.right * 0.5f) + new Vector3(0, _ladderConfig.additionalRaycastHeight, 0) + (_tippyTop.transform.forward * 0.5f));

        // right raycast
        CastRayAndAddToList(hitlist, _tippyTop.transform.position + (_tippyTop.transform.right * 0.5f) + new Vector3(0, _ladderConfig.additionalRaycastHeight, 0) + (_tippyTop.transform.forward * 0.5f));

        // returns shortest vector
        return GetShortestVectorToTop(hitlist);
    }

    private void CastRayAndAddToList(List<Vector3> hitList, Vector3 origin)
    {
        RaycastHit hit;
        if (Physics.Raycast(origin, -_tippyTop.transform.up, out hit, _ladderConfig.maxHeight, ~(_ladderConfig.layersToIgnore)))
        {
            hitList.Add(hit.point);
        }
    }

    private Vector3 GetShortestVectorToTop(List<Vector3> hitList)
    {
        
        Vector3 shortest = _tippyTop.transform.position + new Vector3(0,0.5f,0);
        if(hitList.Count > 0 )
        {
            shortest = hitList[0];
            for (int i = 1; i < hitList.Count; i++)
            {
                if ((_tippyTop.transform.position - shortest).sqrMagnitude > (_tippyTop.transform.position - hitList[i]).sqrMagnitude)
                {
                    shortest = hitList[i];
                }
            }
        }
        return shortest;
    }
}