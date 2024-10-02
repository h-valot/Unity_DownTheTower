using UnityEngine;

public class InputAdvisor : MonoBehaviour
{
    [SerializeField] private RSE_CanInteract _rseCanInteract;
    [SerializeField] private GameObject graphicInteract;

    private void OnEnable()
    {
        _rseCanInteract.action += ToggleInteract;
    }

    private void OnDisable()
    {
        _rseCanInteract.action -= ToggleInteract;
    }

    private void ToggleInteract(bool isActive)
    {
        graphicInteract.SetActive(isActive);
    }
}
