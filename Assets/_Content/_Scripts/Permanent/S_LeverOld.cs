using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class LeverOld : Interactible
{

    [SerializeField] private GameObject _target;
    [SerializeField] private GameObject _leverPivot;
    [SerializeField] private Vector3 _rotationLeverActivate;
    [SerializeField] private Vector3 _rotationLeverDeactivate;
    [SerializeField] private Vector3 _rotationTargetActivate;
    [SerializeField] private Vector3 _rotationTargetDeactivate;

    // animation curve
    [SerializeField] private AnimationCurve _rotationCurve;

    private bool _objectActivate;


    private void Start()
    {
      
    }

    IEnumerator WaitXSeconds(float _time, Action callback)
    {
        yield return new WaitForSeconds(_time);
        callback();
    }

    public override void InteractionTrigger()
    {
        if (_objectActivate == false)
        {
            _objectActivate = true;
            Debug.Log("dans la boucle");
            _leverPivot.transform.DOLocalRotate(_rotationLeverActivate, 1f).SetEase(Ease.InQuint).SetId("Lever").OnComplete(() => { StartCoroutine( WaitXSeconds(0.5f, RotateTargetActivate)); });
            StartCoroutine(WaitXSeconds(8f, RotateLeverDeactivate));
        }
        _objectActivate = false;
    }

    private void RotateTargetActivate()
    {
        _target.transform.DOLocalRotate(_rotationTargetActivate, 1f).SetEase(Ease.OutBounce).SetId("Platform");
    }

    private void RotateTargetDeactivate()
    {
        _target.transform.DOLocalRotate(_rotationTargetDeactivate, 3f).SetEase(_rotationCurve).SetId("Platform");
    }

    private void RotateLeverDeactivate()
    {
        _leverPivot.transform.DOLocalRotate(_rotationLeverDeactivate, 2f).SetEase(Ease.InBack).SetId("Lever").OnComplete(() => { StartCoroutine(WaitXSeconds(0.5f, RotateTargetDeactivate)); });
    }
}
