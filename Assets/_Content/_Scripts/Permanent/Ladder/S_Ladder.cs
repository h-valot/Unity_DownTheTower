using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

public class Ladder : MonoBehaviour
{
    [SerializeField] private GameObject _ladderMesh;
    [SerializeField] private GameObject _TPTriggerTop;
    [SerializeField] private GameObject _TPTriggerBottom;
    [SerializeField] private GameObject _tippyTop;
    [SerializeField] private float _maxHeight;
    [SerializeField] private float _additionalRaycastHeight;
    [SerializeField] private LayerMask _layersToIgnore;

    private Vector3 _bottomPos;
    private Vector3 _topHitPos;

    // Start is called before the first frame update
    void Start()
    {
        SpawnLadder();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SpawnLadder()
    {
        _bottomPos = transform.position;
        _tippyTop.transform.position = new Vector3(_bottomPos.x, _bottomPos.y + _maxHeight, _bottomPos.z);
        _ladderMesh.transform.DOScaleY(_maxHeight / 2, 1);
        _ladderMesh.transform.DOMoveY(transform.position.y + (_maxHeight / 2), 1).OnComplete(() => Falling());
    }

    private void Falling()
    {
        transform.DOLocalRotate(new Vector3(180,transform.rotation.eulerAngles.y,0), 2).OnUpdate(() => PlacementRay());
    }

    [Tooltip("Raycast aligned with the ladder, meant to stop the ladder from falling further once it collides with something.")]
    private void PlacementRay()
    {
        RaycastHit hit;
        Vector3 originPos = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z);
        if (Physics.Raycast(originPos, transform.up, out hit, _maxHeight, ~(_layersToIgnore)))
        {
            transform.DOKill();
            _topHitPos = hit.point;
            _tippyTop.transform.Rotate(- transform.rotation.eulerAngles.x, 0, 0);
            if (transform.rotation.eulerAngles.x <= 45 && !ForwardRay())
            {
                SetUpTP();
            }
        }

    }

    [Tooltip("Raycast that checks that there are no walls at the top of the ladder.")]
    private bool ForwardRay()
    {
        Vector3 originPos = _tippyTop.transform.position + (-_tippyTop.transform.forward * 0.5f) + new Vector3(0, _additionalRaycastHeight, 0);
        return Physics.Raycast(originPos, _tippyTop.transform.forward, 1, ~(_layersToIgnore));
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
        CastRayAndAddToList(hitlist, _tippyTop.transform.position + new Vector3(0, _additionalRaycastHeight, 0) + _tippyTop.transform.forward);

        // left raycast
        CastRayAndAddToList(hitlist, _tippyTop.transform.position + (-_tippyTop.transform.right * 0.5f) + new Vector3(0, _additionalRaycastHeight, 0) + (_tippyTop.transform.forward * 0.5f));

        // right raycast
        CastRayAndAddToList(hitlist, _tippyTop.transform.position + (_tippyTop.transform.right * 0.5f) + new Vector3(0, _additionalRaycastHeight, 0) + (_tippyTop.transform.forward * 0.5f));

        // returns shortest vector
        return GetShortestVectorToTop(hitlist);
    }

    private void CastRayAndAddToList(List<Vector3> hitList, Vector3 origin)
    {
        RaycastHit hit;
        if (Physics.Raycast(origin, -_tippyTop.transform.up, out hit, _maxHeight, ~(_layersToIgnore)))
        {
            hitList.Add(hit.point);
        }
    }

    private Vector3 GetShortestVectorToTop(List<Vector3> hitList)
    {
        
        Vector3 shortest = _tippyTop.transform.position + new Vector3(0,0.5f,0);
        if(hitList.Count > 0 )
        {
            shortest = hitList.ElementAt(0);
            for (int i = 1; i < hitList.Count; i++)
            {
                if ((_tippyTop.transform.position - shortest).sqrMagnitude > (_tippyTop.transform.position - hitList.ElementAt(i)).sqrMagnitude)
                {
                    shortest = hitList.ElementAt(i);
                }
            }
        }
        return shortest;
    }
}
