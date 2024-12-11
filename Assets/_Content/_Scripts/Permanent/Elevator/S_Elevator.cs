using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    [Title("Tweakable values")]
    [SerializeField] private float m_maxHeight;
    [SerializeField] private float m_rideTime;
	
	[Title("Internal references")]
	[SerializeField] private GameObject m_interactable;

	public bool IsUp = false;

    public CharacterMotor m_characterMotor;
    private float m_characterOffset;

    public void StartElevator()
    {
        if (IsUp) Descend();
        else Ascend();

        m_characterOffset = m_characterMotor.transform.position.y - transform.position.y; 
    }

    public void Ascend()
    {
        m_interactable.SetActive(false);
        transform.DOMoveY(transform.position.y + m_maxHeight, m_rideTime).SetEase(Ease.InOutCubic)
            .OnUpdate(()=> { m_characterMotor.Rigidbody.position = transform.position + new Vector3(0f, m_characterOffset, 0f); })
            .OnComplete(() => m_interactable.SetActive(true));

        IsUp = true;
    }

    public void Descend()
    {
        m_interactable.SetActive(false);
        transform.DOMoveY(transform.position.y - m_maxHeight, m_rideTime).SetEase(Ease.InOutCubic)
            .OnUpdate(() => { m_characterMotor.Rigidbody.position = transform.position + new Vector3(0f, m_characterOffset, 0f); })
            .OnComplete(() => m_interactable.SetActive(true));

        IsUp = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterMotor>())
        {
            m_characterMotor = other.GetComponent<CharacterMotor>();
            m_characterMotor.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CharacterMotor>())
        {
            m_characterMotor = other.GetComponent<CharacterMotor>();
            m_characterMotor.transform.SetParent(null);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, m_maxHeight, 0) * (IsUp ? -1 : 1), 0.5f);
    }
}