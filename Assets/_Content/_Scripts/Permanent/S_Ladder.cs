using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    [SerializeField] private GameObject _ladderMesh;
    [SerializeField] private GameObject _TPTriggerTop;
    [SerializeField] private GameObject _TPTriggerBottom;
    [SerializeField] private float _maxHeight = 4;
    [SerializeField] private LayerMask _layersToIgnore;

    private Vector3 _bottomPos;
    private Vector3 _topPos;
    private bool _isPlaced = false;
    private int _cringeIndex = 0;

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
        _ladderMesh.transform.DOScaleY(_maxHeight / 2, 1);
        _ladderMesh.transform.DOMoveY(transform.position.y + (_maxHeight / 2), 1).OnComplete(() => Falling());
    }

    private void Falling()
    {
        transform.DOLocalRotate(new Vector3(180,transform.rotation.eulerAngles.y,0), 2).OnUpdate(() => CastRayOnUpdate());
    }

    private void CastRayOnUpdate()
    {
        RaycastHit hit;
        Vector3 originPos = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z);
        if (Physics.Raycast(originPos, transform.up, out hit, _maxHeight, ~(_layersToIgnore)))
        {   
            transform.DOKill();
            _isPlaced = true;
            _topPos = hit.point;
            if(transform.rotation.eulerAngles.x <= 45)
            {
                SetUpTP();
            }
        }
        
    }

    private void SetUpTP()
    {
        _TPTriggerTop.SetActive(true);
        _TPTriggerBottom.SetActive(true);
        _TPTriggerTop.GetComponent<LadderTP>().SetVariables(_topPos, _bottomPos - transform.forward, false);
        _TPTriggerBottom.GetComponent<LadderTP>().SetVariables(_bottomPos, _topPos + transform.forward, true);
    }

}
