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
	[SerializeField] private RSO_GamePaused m_rsoGamePaused;
	[SerializeField] private RSO_CharacterState m_rsoCharacterState;
	[SerializeField] private RSO_InteractableValid m_rsoInteractableValid;
	[SerializeField] private RSO_InteractableRecyclable m_rsoInteractableRecyclable;

	private bool m_isActive = true;

    private void OnEnable()
    {
        m_rseHideUI.action += ToggleUI;
		m_rsoInteractableValid.OnChanged += ToggleInteract;
		m_rsoInteractableRecyclable.OnChanged += ToggleRecycle;
        m_rsoCharacterState.OnChanged += SwitchAdvisorInputs;
		m_rsoGamePaused.OnChanged += HideAdvisor;
	}

    private void OnDisable()
    {
        m_rseHideUI.action -= ToggleUI;
		m_rsoInteractableValid.OnChanged -= ToggleInteract;
		m_rsoInteractableRecyclable.OnChanged -= ToggleRecycle;
        m_rsoCharacterState.OnChanged -= SwitchAdvisorInputs;
		m_rsoGamePaused.OnChanged -= HideAdvisor;
	}

    private void ToggleInteract()
    {
        m_graphicInteract.SetActive(m_rsoInteractableValid.value);
    }

    private void ToggleRecycle()
    {
        m_graphicRecycle.SetActive(m_rsoInteractableRecyclable.value);
    }

    private void ToggleUI()
	{
		m_isActive = !m_isActive;
		m_inputPanel.SetActive(m_isActive);
	}

    private void HideAdvisor()
    {
		m_isActive = !m_rsoGamePaused.value;
		m_inputPanel.SetActive(m_isActive);
	}

    private void SwitchAdvisorInputs()
    {
        m_ropeInputs.SetActive(m_rsoCharacterState.value == BehaviorState.ROPE);
        m_locomotionInputs.SetActive(m_rsoCharacterState.value != BehaviorState.ROPE);
    }
}