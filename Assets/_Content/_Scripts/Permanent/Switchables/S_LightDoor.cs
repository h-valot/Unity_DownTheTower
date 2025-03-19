using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDoor : LightReciever
{
    [Title("References")]
    [SerializeField] private RSE_PlaySoundAt m_rsePlayAt;
    [SerializeField] private SSO_Sound m_ssoDoorOpen;
    [SerializeField] private MeshRenderer m_meshRendererDoorBorder;
    [SerializeField] private MeshRenderer m_meshRendererDoorDoor;
    [SerializeField] private Transform m_alphaDoor;

    [Title("Tweakable values")]
    [SerializeField] private float m_openHeight = 4.8f;
    [SerializeField] private float m_openDuration = 1f;
    [SerializeField] private float m_openEmitStrength = 1f;
    [SerializeField] private float m_closeEmitStrength = 1f;

    private MaterialPropertyBlock m_propertyBlockDoorDoor;
    private MaterialPropertyBlock m_propertyBlockDoorBorder;
    private float m_currentEmitStrength;
    private void Awake()
    {
        m_propertyBlockDoorDoor = new MaterialPropertyBlock();
        m_propertyBlockDoorDoor.SetFloat("_WorldZAlpha", m_alphaDoor.position.y);
        m_meshRendererDoorDoor.SetPropertyBlock(m_propertyBlockDoorDoor);

        m_propertyBlockDoorBorder = new MaterialPropertyBlock();
        m_currentEmitStrength = m_closeEmitStrength;
        m_propertyBlockDoorBorder.SetFloat("_EmitStrength", m_currentEmitStrength);
        m_meshRendererDoorBorder.SetPropertyBlock(m_propertyBlockDoorBorder);
    }

    private void OnDisable()
    {
        DOTween.Kill(this);
    }

    protected override void ActivateMechanism()
    {
        transform.DOLocalMoveY(m_openHeight, m_openDuration).SetTarget(this);
        DOTween.To(() => m_currentEmitStrength, x => m_currentEmitStrength = x, m_openEmitStrength, 0.5f).SetTarget(this).SetEase(Ease.Linear)
                            .OnUpdate(() => {
                                m_propertyBlockDoorBorder.SetFloat("_EmitStrength", m_currentEmitStrength);
                                m_meshRendererDoorBorder.SetPropertyBlock(m_propertyBlockDoorBorder);
                            });
        m_rsePlayAt.Call(m_ssoDoorOpen, this.transform.position);
    }
}
