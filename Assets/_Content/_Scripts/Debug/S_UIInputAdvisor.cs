using Sirenix.OdinInspector;
using UnityEngine;

public class UIInputAdvisor : MonoBehaviour
{
    [Title("Internal References")]
	[SerializeField] private GameObject m_graphicInteract;
    [SerializeField] private GameObject m_graphicRecycle;
    [SerializeField] private GameObject m_inputPanel;
    [SerializeField] private GameObject m_ropeInputs;
    [SerializeField] private GameObject m_locomotionInputs;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_InputAdviceDisplayed m_rsoInputAdviceDisplayed;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_GamePaused m_rsoGamePaused;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterState m_rsoCharacterState;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_InteractableValid m_rsoInteractableValid;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_InteractableRecyclable m_rsoInteractableRecyclable;

	private bool m_isActive = true;

    private void OnEnable()
    {
        m_rsoInputAdviceDisplayed.OnChanged += OnInputAdviceDisplayedChanged;
		m_rsoInteractableValid.OnChanged += ToggleInteract;
		m_rsoInteractableRecyclable.OnChanged += ToggleRecycle;
        m_rsoCharacterState.OnChanged += SwitchAdvisorInputs;
		m_rsoGamePaused.OnChanged += OnGamePaused;
	}

    private void OnDisable()
    {
        m_rsoInputAdviceDisplayed.OnChanged -= OnInputAdviceDisplayedChanged;
		m_rsoInteractableValid.OnChanged -= ToggleInteract;
		m_rsoInteractableRecyclable.OnChanged -= ToggleRecycle;
        m_rsoCharacterState.OnChanged -= SwitchAdvisorInputs;
		m_rsoGamePaused.OnChanged -= OnGamePaused;
	}

    private void ToggleInteract()
    {
        m_graphicInteract.SetActive(m_rsoInteractableValid.value);
    }

    private void ToggleRecycle()
    {
        m_graphicRecycle.SetActive(m_rsoInteractableRecyclable.value);
    }

    private void OnInputAdviceDisplayedChanged()
	{
		Toggle(m_rsoInputAdviceDisplayed.value);
	}

    private void OnGamePaused()
    {
		Toggle(!m_rsoGamePaused.value);
	}

	private void Toggle(bool isEnabled)
	{
		m_isActive = isEnabled;
		m_inputPanel.SetActive(m_isActive);
	}

    private void SwitchAdvisorInputs()
    {
        m_ropeInputs.SetActive(m_rsoCharacterState.value == BehaviorState.ROPE);
        m_locomotionInputs.SetActive(m_rsoCharacterState.value != BehaviorState.ROPE);
    }
}