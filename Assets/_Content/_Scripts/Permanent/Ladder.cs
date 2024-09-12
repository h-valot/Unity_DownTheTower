using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    [SerializeField] private GameObject ladderMesh;
    [SerializeField] private float maxHeight = 2;

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
        ladderMesh.transform.DOScaleY(maxHeight, 1);
        ladderMesh.transform.DOMoveY(transform.position.y + (1 + maxHeight / 2), 1);
    }
}
