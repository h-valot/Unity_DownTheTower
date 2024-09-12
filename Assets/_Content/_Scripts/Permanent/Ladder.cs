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
        Debug.Log(transform.rotation.ToString());
        Debug.Log(transform.forward.ToString());
    }

    private void Falling()
    {
        transform.DORotate(transform.forward * 90, 1).OnUpdate(() => CastRayOnUpdate());
    }

    private void CastRayOnUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.up, out hit, maxHeight, ~(layersToIgnore)))
        {
            Debug.Log(hit.collider.gameObject.name);
            
            transform.DOKill();
        }
        
    }
}
