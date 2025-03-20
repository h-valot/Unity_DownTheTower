using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class UIInputAdvisor : MonoBehaviour
{
	[FoldoutGroup("Internal References")][SerializeField] private GameObject m_graphicInteract;
	[FoldoutGroup("Internal References")][SerializeField] private TextMeshProUGUI m_tmpInteract;
	[FoldoutGroup("Internal References")][SerializeField] private GameObject m_graphicRecycle;
	[FoldoutGroup("Internal References")][SerializeField] private GameObject m_inputPanel;
	[FoldoutGroup("Internal References")][SerializeField] private GameObject m_ropeInputs;
	[FoldoutGroup("Internal References")][SerializeField] private GameObject m_locomotionInputs;
    [FoldoutGroup("Internal References")][SerializeField] private GameObject m_onScreenHints;

    [FoldoutGroup("Scriptable")][SerializeField] private SSO_Character m_ssoCharacter;

    [FoldoutGroup("Scriptable")][SerializeField] private RSE_GameEnd m_rseGameEnd;
    [FoldoutGroup("Scriptable")][SerializeField] private RSO_GameStarted m_rsoGameStarted;
    [FoldoutGroup("Scriptable")][SerializeField] private RSO_InteractableRecyclable m_rsoInteractableRecyclable;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_OnScreenHintsDisplayed m_rsoOnScreenHintsDisplayed;
    [FoldoutGroup("Scriptable")][SerializeField] private RSO_InputAdvisorDisplayed m_rsoInputAdviceDisplayed;
    [FoldoutGroup("Scriptable")][SerializeField] private RSO_InteractableValid m_rsoInteractableValid;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterState m_rsoCharacterState;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_Pause m_rsoPause;

	private bool m_isActive = true;

    private void Start()
    {
		Toggle(false);
    }

    private void OnEnable()
    {
		m_rsoInteractableRecyclable.OnChanged += ToggleRecycle;
        m_rsoInputAdviceDisplayed.OnChanged += OnInputAdvisorDisplayedChanged;
        m_rsoOnScreenHintsDisplayed.OnChanged += OnOnScreenHintsDisplayedChanged;
        m_rsoInteractableValid.OnChanged += ToggleInteract;
        m_rsoCharacterState.OnChanged += SwitchAdvisorInputs;
		m_rsoPause.OnChanged += OnPaused;
		m_rsoGameStarted.OnChanged += GameStart;
        m_rseGameEnd.action += GameEnd;
    }

    private void OnDisable()
    {
		m_rsoInteractableRecyclable.OnChanged -= ToggleRecycle;
        m_rsoInputAdviceDisplayed.OnChanged -= OnInputAdvisorDisplayedChanged;
        m_rsoOnScreenHintsDisplayed.OnChanged -= OnOnScreenHintsDisplayedChanged;
        m_rsoInteractableValid.OnChanged -= ToggleInteract;
        m_rsoCharacterState.OnChanged -= SwitchAdvisorInputs;
		m_rsoPause.OnChanged -= OnPaused;
        m_rsoGameStarted.OnChanged -= GameStart;
        m_rseGameEnd.action -= GameEnd;
    }

    private void ToggleInteract()
	{
		if (m_rsoPause.value) return;

		m_graphicInteract.SetActive(m_rsoInteractableValid.value != InteractableType.NONE);
		m_tmpInteract.text = m_ssoCharacter.InteractableFlavors.FirstOrDefault(i => i.Type == m_rsoInteractableValid.value).Flavor;
	}

	private void ToggleRecycle()
	{
		if (m_rsoPause.value) return;

		m_graphicRecycle.SetActive(m_rsoInteractableRecyclable.value);
    }

    private void OnOnScreenHintsDisplayedChanged()
	{
		if (m_rsoPause.value) return;

        ToggleOnScreenHints(m_rsoOnScreenHintsDisplayed.value);
	}

    private void OnInputAdvisorDisplayedChanged()
    {
        if (m_rsoPause.value) return;

        Toggle(m_rsoInputAdviceDisplayed.value);
    }

    private void OnPaused()
    {
		if (m_rsoPause.value)
		{
			Toggle(false);
		}
		else
		{
			Toggle(true);
		}
	}

	private void GameEnd()
	{
		Toggle(false);
	}

	private void GameStart()
    {
        if(m_rsoGameStarted.value) Toggle(true);
    }

	private void Toggle(bool isEnabled)
	{
		m_isActive = isEnabled;
		m_inputPanel.SetActive(m_isActive);
	}

    private void ToggleOnScreenHints(bool isEnabled)
    {
        m_isActive = isEnabled;
        m_onScreenHints.SetActive(m_isActive);
    }

    private void SwitchAdvisorInputs()
    {
        m_ropeInputs.SetActive(m_rsoCharacterState.value == BehaviorState.ROPE);
        m_locomotionInputs.SetActive(m_rsoCharacterState.value != BehaviorState.ROPE);
    }
}