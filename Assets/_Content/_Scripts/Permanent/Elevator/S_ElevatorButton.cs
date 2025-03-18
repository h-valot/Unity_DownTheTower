using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class ElevatorButton : Interactable
{
    [Title("Tweakable values")]
    [SerializeField] private bool m_isUpButton;
    [SerializeField] private float EmitStrengthActive;
    [SerializeField] private float EmitStrengthInactive;

    [Title("External references")]
    [SerializeField] private Elevator m_elevator;
    [SerializeField] private RSE_PlayAt m_rse_playAt;
    [SerializeField] private SSO_Sound m_ButtonSound;

    [Title("Internal references")]
    [SerializeField] private MeshRenderer m_rendererBase;
    [SerializeField] private MeshRenderer m_rendererButton;

    private MaterialPropertyBlock m_propertyBlock;
    private float m_currentEmit;

    private void Awake()
    {
        m_propertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        UpdateButtonState();
    }

    private void OnDisable()
    {
        DOTween.Kill(this);
    }

    public override void InteractionTrigger()
    {
        if(m_elevator.IsUp && !m_isUpButton)
        {
            m_elevator.Descend();
            m_rse_playAt.Call(m_ButtonSound, this.transform.position);
            UpdateGraphics();
        }
        else if(!m_elevator.IsUp && m_isUpButton)
        {
            m_elevator.Ascend();
            m_rse_playAt.Call(m_ButtonSound, this.transform.position);
            UpdateGraphics();
        }
    }

    private void UpdateGraphics()
    {
        DOTween.Kill(this);
        DOTween.To(() => m_currentEmit, x => m_currentEmit = x, EmitStrengthInactive, 0.2f).SetTarget(this).SetEase(Ease.Linear)
                            .OnUpdate(() => {
                                m_propertyBlock.SetFloat("_EmitStrength", m_currentEmit);
                                m_rendererBase.SetPropertyBlock(m_propertyBlock);
                                m_rendererButton.SetPropertyBlock(m_propertyBlock);
                            });
        m_rendererButton.transform.DOLocalMoveZ(-0.08f, 0.2f).SetTarget(this).OnComplete(() => 
        {
            m_rendererButton.transform.DOLocalMoveZ(0, 0.2f).SetTarget(this); 
        });
    }

    public void UpdateButtonState()
    {
        if ((m_elevator.IsUp && m_isUpButton) || (!m_elevator.IsUp && !m_isUpButton))
        {
            m_propertyBlock.SetFloat("_EmitStrength", EmitStrengthInactive);
            m_rendererBase.SetPropertyBlock(m_propertyBlock);
            m_rendererButton.SetPropertyBlock(m_propertyBlock);
        }
        else
        {
            DOTween.To(() => m_currentEmit, x => m_currentEmit = x, EmitStrengthActive, 0.2f).SetTarget(this).SetEase(Ease.Linear)
                            .OnUpdate(() => {
                                m_propertyBlock.SetFloat("_EmitStrength", m_currentEmit);
                                m_rendererBase.SetPropertyBlock(m_propertyBlock);
                                m_rendererButton.SetPropertyBlock(m_propertyBlock);
                            });
        }
    }
}
