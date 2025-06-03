using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;

public class Elevator : MonoBehaviour
{
    [Title("Tweakable values")]
    [SerializeField] private float m_rideTime;
	
	[Title("Internal references")]
	[SerializeField] private GameObject m_interactable;
    [SerializeField] private GameObject m_bottomAnchor;
    [SerializeField] private GameObject m_topAnchor;

    [Title("External references")]
    [SerializeField] private ElevatorButton[] m_buttons;
    [SerializeField] private GameObject m_pillarPrefab;
    [SerializeField] private RSO_CharacterElevator m_rsoCharacterElevator;
    [SerializeField] private RSE_PlaySound m_rsePlaySound;
    [SerializeField] private RSE_StopSound m_rseStopSound;
    [SerializeField] private SSO_Sound m_ssoElevatorStart;
    [SerializeField] private SSO_Sound m_ssoElevatorEnd;

    public bool IsUp = false;

    public CharacterMotor m_characterMotor;
    private float m_characterOffset;
    private List<GameObject> m_pillars;

    private void Awake()
    {
        m_pillars = new List<GameObject>();

        m_bottomAnchor.transform.parent = null;
        m_topAnchor.transform.parent = null;

        int numberOfPillar = (int)Mathf.Floor((transform.position.y - m_bottomAnchor.transform.position.y) * 0.25f);
        if (numberOfPillar != m_pillars.Count)
        {
            if (numberOfPillar > m_pillars.Count)
            {
                int tmpCount = numberOfPillar - m_pillars.Count;
                for (int i = 0; i < tmpCount; i++)
                {
                    m_pillars.Add(Instantiate(m_pillarPrefab, transform.position, transform.rotation, transform));
                    m_pillars[^1].transform.localPosition = new Vector3(0f, -m_pillars.Count * 4f, 0f);
                }
            }
            else
            {
                int tmpCount = m_pillars.Count - numberOfPillar;
                for (int i = 0; i < tmpCount; i++)
                {
                    Destroy(m_pillars[^1]);
                }
            }
        }
    }

    public void StartElevator()
    {
        m_rsoCharacterElevator.value = true;
        m_characterOffset = 0.2368546f;

        m_rsePlaySound.Call(m_ssoElevatorStart);

        if (IsUp)
        {
            Descend();
        }
        else
        {
            Ascend();
        }
    }

    public void Ascend()
    {
        m_interactable.SetActive(false);
        transform.DOMoveY(m_topAnchor.transform.position.y, m_rideTime).SetEase(Ease.InOutCubic)
            .OnUpdate(()=> 
            {
                m_characterMotor.Rigidbody.position = transform.position + new Vector3(0f, m_characterOffset, 0f);
                int numberOfPillar =(int) Mathf.Floor((transform.position.y - m_bottomAnchor.transform.position.y) * 0.25f);
                if(numberOfPillar != m_pillars.Count)
                {
                    int tmpCount = numberOfPillar - m_pillars.Count;
                    for (int i = 0; i < tmpCount; i++)
                    {
                        m_pillars.Add(Instantiate(m_pillarPrefab, transform.position, transform.rotation, transform));
                        m_pillars[^1].transform.localPosition = new Vector3(0f, -m_pillars.Count * 4f, 0f);
                    }
                }
            })
            .OnComplete(() =>
            {
                m_rseStopSound.Call(m_ssoElevatorStart);
                m_rsePlaySound.Call(m_ssoElevatorEnd);
                m_interactable.SetActive(true);
                m_rsoCharacterElevator.value = false;
            });

        IsUp = true;
        foreach(ElevatorButton button in m_buttons)
        {
            button.UpdateButtonState();
        }
    }

    public void Descend()
    {
        m_interactable.SetActive(false);
        transform.DOMoveY(m_bottomAnchor.transform.position.y, m_rideTime).SetEase(Ease.InOutCubic)
            .OnUpdate(() => 
            {
                m_characterMotor.Rigidbody.position = transform.position + new Vector3(0f, m_characterOffset, 0f);
                
                int numberOfPillar = (int)Mathf.Floor((transform.position.y - m_bottomAnchor.transform.position.y) * 0.25f);
                
                if (numberOfPillar != m_pillars.Count)
                {
                    int tmpCount = m_pillars.Count - numberOfPillar;
                    for (int i = 0; i < tmpCount; i++)
                    {
                        GameObject go = m_pillars[^1];
                        m_pillars.RemoveAt(m_pillars.Count - 1);
                        Destroy(go);

                    }
                }
            })
            .OnComplete(() => 
            {
                m_rseStopSound.Call(m_ssoElevatorStart);
                m_rsePlaySound.Call(m_ssoElevatorEnd);
                m_interactable.SetActive(true);
                m_rsoCharacterElevator.value = false;
            });

        IsUp = false;
        foreach (ElevatorButton button in m_buttons)
        {
            button.UpdateButtonState();
        }
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
        Gizmos.DrawWireSphere(m_bottomAnchor.transform.position, 0.5f);
        Gizmos.DrawWireSphere(m_topAnchor.transform.position, 0.5f);
    }
}