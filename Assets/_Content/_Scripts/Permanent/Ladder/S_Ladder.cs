using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
	[Header("Internal references")]
    [SerializeField] private GameObject _ladderMesh;
    [SerializeField] private GameObject _TPTriggerTop;
    [SerializeField] private GameObject _TPTriggerBottom;
	[SerializeField] private GameObject _tippyTop;

	[Header("Scriptable references")]
	[SerializeField] private LadderConfig _ladderConfig;

    private Vector3 _bottomPos;
    private Vector3 _topHitPos;

    public void Initialize()
	{
		_bottomPos = transform.position;
        _tippyTop.transform.position = new Vector3(_bottomPos.x, _bottomPos.y + _ladderConfig.maxHeight, _bottomPos.z);
        _ladderMesh.transform.DOScaleY(_ladderConfig.maxHeight / 2, 1);
        _ladderMesh.transform.DOMoveY(transform.position.y + (_ladderConfig.maxHeight / 2), 1).OnComplete(Falling);
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
        _TPTriggerTop.GetComponent<LadderTP>().SetVariables(_topHitPos, _bottomPos - transform.forward, false);
        _TPTriggerBottom.GetComponent<LadderTP>().SetVariables(_bottomPos, PickBestTeleportPoint(), true);
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