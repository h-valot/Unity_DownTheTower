using UnityEngine;

public class InputAdvisor : MonoBehaviour
{

    [Header("Internal References")]
    [SerializeField] private GameObject graphicInteract;
    [SerializeField] private GameObject graphicRecycle;
    [SerializeField] private GameObject inputPanel;
    [SerializeField] private GameObject ropeInputs;
    [SerializeField] private GameObject locomotionInputs;

    [Header("External References")]
    [SerializeField] private RSE_CanInteract _rseCanInteract;
    [SerializeField] private RSE_CanRecycle _rseCanRecycle;
    [SerializeField] private RSO_CharacterState _rsoCharacterState;
    [SerializeField] private RSE_HideUI _rseHideUI;

    private bool isUIactive = true;

    private void OnEnable()
    {
        _rseCanInteract.action += ToggleInteract;
        _rseCanRecycle.action += ToggleRecycle;
        _rseHideUI.action += ToggleUI;
        _rsoCharacterState.OnChanged += SwitchAdvisorInputs;
    }

    private void OnDisable()
    {
        _rseCanInteract.action -= ToggleInteract;
        _rseCanRecycle.action -= ToggleRecycle;
        _rseHideUI.action -= ToggleUI;
        _rsoCharacterState.OnChanged -= SwitchAdvisorInputs;
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
        isUIactive = !isUIactive;
        inputPanel.SetActive(isUIactive);
    }

    private void SwitchAdvisorInputs()
    {
        ropeInputs.SetActive(_rsoCharacterState.value == AnimationState.ROPE);
        locomotionInputs.SetActive(!(_rsoCharacterState.value == AnimationState.ROPE));
    }
}
