using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PreLadder : MonoBehaviour
{
    [SerializeField] private RSO_PlayerTransform _rsoPlayerTransform;
    [SerializeField] private Ladder _pfLadder;
    [SerializeField] private float _minDistFromPlayer;
    [SerializeField] private float _maxDistFromPlayer;
    [SerializeField] private float _minCameraAngle;
    [SerializeField] private float _maxCameraAngle;
    [SerializeField] private float _ladderHeight;

    private bool _isPlaceable = false;

    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
        CheckIfPlaceable();
    }

    private void UpdatePosition()
    {
        transform.position = _rsoPlayerTransform.value.position + _rsoPlayerTransform.value.forward * GetDistanceWithCamera();
        _isPlaceable = !Physics.Raycast(transform.position + new Vector3(0, 0.25f, 0), new Vector3(0, -1, 0), 0.5f);
    }

    private float GetDistanceWithCamera()
    {
        // get camera angle & clamp
        float angle = Camera.main.transform.rotation.eulerAngles.x;
        if (angle > 80 || angle < _minCameraAngle) angle = _minCameraAngle;
        else if (angle > _maxCameraAngle) angle = _maxCameraAngle;

        // convert camera angle value to distance from player value
        return _maxDistFromPlayer - ((angle - _minCameraAngle) * (_maxDistFromPlayer - _minDistFromPlayer) / (_maxCameraAngle - _minCameraAngle));
    }

    public void InstanciateLadder()
    {
        if (_isPlaceable)
        {
            Ladder newLadder = Instantiate(_pfLadder, transform.position, _rsoPlayerTransform.value.rotation);
            newLadder.SetHeight(_ladderHeight);
        }
    }

    private void CheckIfPlaceable()
    {
        // Cast 1 = Check if there is ground under the ladder ; Cast 2 = Check that there is enough room above
        _isPlaceable = Physics.Raycast(transform.position + new Vector3(0, 0.25f, 0), new Vector3(0, -1, 0), 0.5f) &&
            !Physics.Raycast(transform.position + new Vector3(0, 0.25f, 0), new Vector3(0, 1, 0), _ladderHeight - 0.25f);

    }
}
