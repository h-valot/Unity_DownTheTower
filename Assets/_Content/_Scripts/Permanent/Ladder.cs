using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    [SerializeField] private GameObject ladderMesh;
    [SerializeField] private float maxHeight = 4;
    [SerializeField] private LayerMask layersToIgnore;

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
        ladderMesh.transform.DOScaleY(maxHeight / 2, 1);
        ladderMesh.transform.DOMoveY(transform.position.y + (maxHeight / 2), 1).OnComplete(() => Falling());
    }

    private void Falling()
    {
        transform.DOLocalRotate(new Vector3(180,transform.rotation.eulerAngles.y,0), 2).OnUpdate(() => CastRayOnUpdate());
    }

    private void CastRayOnUpdate()
    {
        RaycastHit hit;
        Vector3 originPos = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z);
        if (Physics.Raycast(originPos, transform.up, out hit, maxHeight, ~(layersToIgnore)))
        {   
            transform.DOKill();
        }
        
    }
}
