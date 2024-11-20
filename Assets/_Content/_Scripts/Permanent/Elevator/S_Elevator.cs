using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    [Header("Internal Variables")]
    [SerializeField] private GameObject _interactible;

    [Header("Tweakable Variables")]
    [SerializeField] private float _maxHeight;
    [SerializeField] private float _minHeight;
    [SerializeField] private float _rideTime;

    [Header("Public Variables")]
    public bool isUp = false;

    public void StartElevator()
    {
        if (isUp) Descend();
        else Ascend();
    }

    public void Ascend()
    {
        _interactible.SetActive(false);
        transform.DOMoveY(_maxHeight, _rideTime).SetEase(Ease.InOutCubic).OnComplete(() =>
            _interactible.SetActive(true));
        isUp = true;
    }

    public void Descend()
    {
        _interactible.SetActive(false);
        transform.DOMoveY(_minHeight, _rideTime).SetEase(Ease.InOutCubic).OnComplete(() =>
            _interactible.SetActive(true));
        isUp = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var _character))
        {
            _character.transform.parent = transform;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var _character))
        {
            _character.transform.parent = null;
        }
    }
}
