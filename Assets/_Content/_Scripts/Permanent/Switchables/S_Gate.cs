using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Gate : Switchable
{
    [Title("References")]
    [SerializeField] private RSE_PlayAt m_rsePlayAt;
    [SerializeField] private SSO_Sound m_ssoDoorOpen;
    [SerializeField] private SSO_Sound m_ssoDoorClose;

    [SerializeField] private MeshRenderer m_meshRendererDoorBorder;

    [Title("Tweakable values")]
    [SerializeField] private float m_openHeight = 4.8f;
    [SerializeField] private float m_openDuration = 1f;
    [SerializeField] private float m_openEmitStrength = 1f;
    [SerializeField] private float m_closeHeight = 0.24f;
    [SerializeField] private float m_closeDuration = 0.5f;
    [SerializeField] private float m_closeEmitStrength = 1f;

    private MaterialPropertyBlock m_propertyBlock;
    private float m_currentEmitStrength;

    private void Awake()
    {
        m_propertyBlock = new MaterialPropertyBlock();
        m_currentEmitStrength = m_closeEmitStrength;
        m_propertyBlock.SetFloat("_EmitStrength", m_currentEmitStrength);
        m_meshRendererDoorBorder.SetPropertyBlock(m_propertyBlock);
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
                                m_propertyBlock.SetFloat("_EmitStrength", m_currentEmitStrength);
                                m_meshRendererDoorBorder.SetPropertyBlock(m_propertyBlock);
                            });
        m_rsePlayAt.Call(m_ssoDoorOpen, this.transform.position);
    }

    protected override void DeactivateMechanism()
    {
        transform.DOLocalMoveY(m_closeHeight, m_closeDuration).SetTarget(this);
        DOTween.To(() => m_currentEmitStrength, x => m_currentEmitStrength = x, m_closeEmitStrength, 0.5f).SetTarget(this).SetEase(Ease.Linear)
                            .OnUpdate(() => {
                                m_propertyBlock.SetFloat("_EmitStrength", m_currentEmitStrength);
                                m_meshRendererDoorBorder.SetPropertyBlock(m_propertyBlock);
                            });
        m_rsePlayAt.Call(m_ssoDoorClose, this.transform.position);
    }
}
