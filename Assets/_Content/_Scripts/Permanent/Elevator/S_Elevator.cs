using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    [Title("Tweakable values")]
    [SerializeField] private float m_maxHeight;
    [SerializeField] private float m_minHeight;
    [SerializeField] private float m_rideTime;
	
	[Title("Internal references")]
	[SerializeField] private GameObject m_interactable;

	[HideInInspector] public bool IsUp = false;

    public void StartElevator()
    {
        if (IsUp) Descend();
        else Ascend();
    }

    public void Ascend()
    {
        m_interactable.SetActive(false);
        transform.DOMoveY(m_maxHeight, m_rideTime).SetEase(Ease.InOutCubic).OnComplete(() =>
            m_interactable.SetActive(true));

        IsUp = true;
    }

    public void Descend()
    {
        m_interactable.SetActive(false);
        transform.DOMoveY(m_minHeight, m_rideTime).SetEase(Ease.InOutCubic).OnComplete(() =>
            m_interactable.SetActive(true));

        IsUp = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var character))
        {
            character.transform.parent = transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var character))
        {
            character.transform.parent = null;
        }
    }
}