using UnityEngine;

public class InputAdvisor : MonoBehaviour
{
    [Header("Internal References")]
    [SerializeField] private GameObject m_graphicInteract;
    [SerializeField] private GameObject m_graphicRecycle;
    [SerializeField] private GameObject m_inputPanel;
    [SerializeField] private GameObject m_ropeInputs;
    [SerializeField] private GameObject m_locomotionInputs;

    [Header("External References")]
    [SerializeField] private RSE_HideUI m_rseHideUI;
	[Space(5)]
    [SerializeField] private RSO_CanInteract m_rsoCanInteract;
    [SerializeField] private RSO_CanRecycle m_rsoCanRecycle;
    [SerializeField] private RSO_CharacterState m_rsoCharacterState;

    private bool m_isActive = true;

    private void OnEnable()
    {
        m_rsoCanInteract.OnChanged += ToggleInteract;
        m_rsoCanRecycle.OnChanged += ToggleRecycle;
        m_rseHideUI.action += ToggleUI;
        m_rsoCharacterState.OnChanged += SwitchAdvisorInputs;
    }

    private void OnDisable()
    {
        m_rsoCanInteract.OnChanged -= ToggleInteract;
        m_rsoCanRecycle.OnChanged -= ToggleRecycle;
        m_rseHideUI.action -= ToggleUI;
        m_rsoCharacterState.OnChanged -= SwitchAdvisorInputs;
    }

    private void ToggleInteract()
    {
        m_graphicInteract.SetActive(m_rsoCanInteract.value);
    }

    private void ToggleRecycle()
    {
        m_graphicRecycle.SetActive(m_rsoCanRecycle.value);
    }

    private void ToggleUI()
    {
        m_isActive = !m_isActive;
        m_inputPanel.SetActive(m_isActive);
    }

    private void SwitchAdvisorInputs()
    {
        m_ropeInputs.SetActive(m_rsoCharacterState.value == BehaviorState.ROPE);
        m_locomotionInputs.SetActive(m_rsoCharacterState.value != BehaviorState.ROPE);
    }
}