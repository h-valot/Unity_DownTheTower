using EnhancedHierarchy.Icons;
using UnityEngine;

public class InputAdvisor : MonoBehaviour
{

    [Header("Internal References")]
    [SerializeField] private GameObject parentPanel;
    [SerializeField] private GameObject graphicInteract;
    [SerializeField] private GameObject graphicRecycle;
    [SerializeField] private GameObject inputPanel;
    [SerializeField] private GameObject ropeInputs;
    [SerializeField] private GameObject locomotionInputs;

    [Header("External References")]
    [SerializeField] private RSE_CanInteract _rseCanInteract;
    [SerializeField] private RSE_CanRecycle _rseCanRecycle;
    [SerializeField] private RSE_HideUI _rseHideUI;
    [SerializeField] private RSO_CharacterState _rsoCharacterState;
    [SerializeField] private RSO_GamePaused _rsoGamePaused;

    private void OnEnable()
    {
        _rseCanInteract.action += ToggleInteract;
        _rseCanRecycle.action += ToggleRecycle;
        _rseHideUI.action += ToggleUI;
        _rsoCharacterState.OnChanged += SwitchAdvisorInputs;
        _rsoGamePaused.OnChanged += HideAdvisor;
    }

    private void OnDisable()
    {
        _rseCanInteract.action -= ToggleInteract;
        _rseCanRecycle.action -= ToggleRecycle;
        _rseHideUI.action -= ToggleUI;
        _rsoCharacterState.OnChanged -= SwitchAdvisorInputs;
        _rsoGamePaused.OnChanged -= HideAdvisor;
    }

    private void ToggleInteract(bool isActive)
    {
        graphicInteract.SetActive(isActive);
    }

    private void ToggleRecycle(bool isActive)
    {
        graphicRecycle.SetActive(isActive);
    }

    private void ToggleUI()
    {
        inputPanel.SetActive(!inputPanel.activeInHierarchy);
    }

    private void HideAdvisor()
    {
        if (_rsoGamePaused.value) parentPanel.SetActive(false);
        else parentPanel.SetActive(true);
    }

    private void SwitchAdvisorInputs()
    {
        ropeInputs.SetActive(_rsoCharacterState.value == AnimationState.ROPE);
        locomotionInputs.SetActive(!(_rsoCharacterState.value == AnimationState.ROPE));
    }
}
