using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : Switchable
{
    [Title("References")]
    [SerializeField] private RSE_PlaySoundAt m_rsePlayAt;
    [SerializeField] private RSO_CharacterPosition m_rsoCharacterPosition;
    [SerializeField] private SSO_Sound m_ssoBridgeActivate;
    [SerializeField] private SSO_Sound m_ssoGateOpening;
    [SerializeField] private SSO_Sound m_ssoGateClosing;

    [SerializeField] private Transform m_bridge0;
    [SerializeField] private Transform m_bridge1;
    [SerializeField] private MeshRenderer m_bridgeGateRenderer;


    [Title("Tweakable values")]
    [SerializeField] private float m_bridge0OpenRotation = 0f;
    [SerializeField] private float m_bridge0CloseRotation = -85f;
    [SerializeField] private float m_bridge1OpenRotation = 0f;
    [SerializeField] private float m_bridge1CloseRotation = 172f;
    [SerializeField] private float m_bridgeOpenningDuration = 1f;
    [SerializeField] private float m_gateOpenEmitStrentgh = 7f;
    [SerializeField] private float m_gateCloseEmitStrentgh = 0.5f;
    [SerializeField] private float m_gateHeightOpen = -0.963f;
    [SerializeField] private float m_gateOpenningDuration = 1.25f;
    [SerializeField] private float m_gateClosingDuration = 0.5f;

    private MaterialPropertyBlock m_propertyBlock;
    private float m_currentEmitStrength;

    private void Awake()
    {
        m_propertyBlock = new MaterialPropertyBlock();
        m_currentEmitStrength = m_gateOpenEmitStrentgh;
        m_propertyBlock.SetFloat("_EmitStrength", m_currentEmitStrength);
        m_bridgeGateRenderer.SetPropertyBlock(m_propertyBlock);
        m_bridge0.localRotation = Quaternion.Euler(m_bridge0CloseRotation, 0f, 0f);
        m_bridge1.localRotation = Quaternion.Euler(m_bridge1CloseRotation, 0f, 0f);
    }

    private void OnDisable()
    {
        DOTween.Kill(this);
    }

    protected override void ActivateMechanism()
    {
        DOTween.Kill(this);
        m_bridge0.DOLocalRotate(new Vector3(m_bridge0OpenRotation, 0f, 0f), m_bridgeOpenningDuration).SetTarget(this);
        m_bridge1.DOLocalRotate(new Vector3(m_bridge1OpenRotation, 0f, 0f), m_bridgeOpenningDuration).SetTarget(this).OnComplete(() =>
        {
            DOTween.To(() => m_currentEmitStrength, x => m_currentEmitStrength = x, m_gateCloseEmitStrentgh, m_gateOpenningDuration).SetTarget(this).SetEase(Ease.Linear)
                            .OnUpdate(() => {
                                m_propertyBlock.SetFloat("_EmitStrength", m_currentEmitStrength);
                                m_bridgeGateRenderer.SetPropertyBlock(m_propertyBlock);
                            });
            m_bridgeGateRenderer.transform.DOLocalMoveY(m_gateHeightOpen, m_gateOpenningDuration).SetTarget(this);
            m_rsePlayAt.Call(m_ssoGateOpening, m_bridgeGateRenderer.transform.position);
        });

        m_rsePlayAt.Call(m_ssoBridgeActivate, m_rsoCharacterPosition.value);
    }

    protected override void DeactivateMechanism()
    {
        DOTween.Kill(this);
        DOTween.To(() => m_currentEmitStrength, x => m_currentEmitStrength = x, m_gateOpenEmitStrentgh, m_gateClosingDuration).SetTarget(this).SetEase(Ease.Linear)
                            .OnUpdate(() => {
                                m_propertyBlock.SetFloat("_EmitStrength", m_currentEmitStrength);
                                m_bridgeGateRenderer.SetPropertyBlock(m_propertyBlock);
                            });
        m_bridgeGateRenderer.transform.DOLocalMoveY(0f, m_gateClosingDuration).SetTarget(this).OnComplete(() =>
        {

            m_bridge0.DOLocalRotate(new Vector3(m_bridge0CloseRotation, 0f, 0f), m_bridgeOpenningDuration).SetTarget(this);
            m_bridge1.DOLocalRotate(new Vector3(m_bridge1CloseRotation, 0f, 0f), m_bridgeOpenningDuration).SetTarget(this);
            m_rsePlayAt.Call(m_ssoBridgeActivate, this.transform.position);
        });
        m_rsePlayAt.Call(m_ssoGateClosing, m_bridgeGateRenderer.transform.position);
    }
}
