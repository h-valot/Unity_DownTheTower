using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class LightBasket : MonoBehaviour
{
    [SerializeField] private LightReciever _parentDoor;
    [SerializeField] private float _maxVelocitySnap = 1;
    [SerializeField] private MeshRenderer m_meshRendererBasket;
    [SerializeField] private float m_activationDuration = 1f;
    [SerializeField] private float m_activeEmitStrength = 1f;
    [SerializeField] private float m_deactiveEmitStrength = 1f;

    private MaterialPropertyBlock m_propertyBlockBasket;
    private float m_currentEmitStrength;

    private bool _isFilled = false;
    private List<Rigidbody> torches = new List<Rigidbody>();

    private void Awake()
    {
        m_currentEmitStrength = m_deactiveEmitStrength;
        m_propertyBlockBasket = new MaterialPropertyBlock();
        m_propertyBlockBasket.SetFloat("_EmitStrength", m_currentEmitStrength);
        m_meshRendererBasket.SetPropertyBlock(m_propertyBlockBasket);
    }

    private void OnDisable()
    {
        DOTween.Kill(this);
    }

    private void Update()
    {
        if (_isFilled) return;

        foreach (Rigidbody torch in torches)
        {
            if (torch.linearVelocity.magnitude <= _maxVelocitySnap) ActivateBasket(torch);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isFilled) return;

        if (other.gameObject.TryGetComponent<Torch>(out Torch torch))
        {
            if (torch.IsInHand) return;
            torches.Add(other.gameObject.GetComponent<Rigidbody>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<Torch>(out Torch torch))
        {
            torches.Remove(other.gameObject.GetComponent<Rigidbody>());
        }
    }

    private void ActivateBasket(Rigidbody torch)
    {
        _isFilled = true;
        torch.GetComponent<Torch>().DetachAndCancelDeactivation();
        _parentDoor.AddToLightCounter();
        DOTween.To(() => m_currentEmitStrength, x => m_currentEmitStrength = x, m_activeEmitStrength, 0.5f).SetTarget(this).SetEase(Ease.Linear)
                            .OnUpdate(() => {
                                m_propertyBlockBasket.SetFloat("_EmitStrength", m_currentEmitStrength);
                                m_meshRendererBasket.SetPropertyBlock(m_propertyBlockBasket);
                            });
    }
}
