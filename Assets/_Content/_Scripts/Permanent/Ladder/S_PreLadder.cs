using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PreLadder : MonoBehaviour
{
    [SerializeField] private RSO_PlayerTransform _rsoPlayerTransform;
    [SerializeField] private Ladder _pfLadder;
    [SerializeField] private float minDistFromPlayer;
    [SerializeField] private float maxDistFromPlayer;
    [SerializeField] private float minCameraAngle;
    [SerializeField] private float maxCameraAngle;

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
        // get camera angle & clamp
        float angle = Camera.main.transform.rotation.eulerAngles.x;
        if (angle > 80 || angle < minCameraAngle) angle = minCameraAngle;
        else if (angle > maxCameraAngle) angle = maxCameraAngle;

        // convert camera angle value to distance from player value
        return maxDistFromPlayer - ((angle - minCameraAngle) * (maxDistFromPlayer - minDistFromPlayer) / (maxCameraAngle - minCameraAngle));
    }

    public void InstanciateLadder()
    {
        if (Physics.Raycast(transform.position + new Vector3(0, 0.25f, 0), new Vector3(0, -1, 0), 0.5f))
        {
            Instantiate(_pfLadder, transform.position, _rsoPlayerTransform.value.rotation);
        }
    }
}
