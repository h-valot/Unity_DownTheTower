using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PreLadder : MonoBehaviour
{
    [SerializeField] private RSO_PlayerTransform _rsoPlayerTransform;
    [SerializeField] private float minDistFromPlayer;
    [SerializeField] private float maxDistFromPlayer;
    [SerializeField] private float minCameraAngle;
    [SerializeField] private float maxCameraAngle;

    // Start is called before the first frame update
    void Start()
    {
        UpdatePosition();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        transform.position = _rsoPlayerTransform.value.position + _rsoPlayerTransform.value.forward * GetDistanceWithCamera();
    }

    private float GetDistanceWithCamera()
    {
        float value = math.remap(minCameraAngle, minCameraAngle, minDistFromPlayer, maxDistFromPlayer, Mathf.Clamp(Camera.main.transform.rotation.eulerAngles.x, minCameraAngle, minCameraAngle));
        Debug.Log(value.ToString());
        return 1;
    }
}
