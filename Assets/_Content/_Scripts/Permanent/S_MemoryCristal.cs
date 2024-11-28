using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class MemoryCristal : Interactable
{
	[Title("Tweakable values")]
    [SerializeField] private Vector3 m_direction;
	[SerializeField] private float m_duration;

	[Title("External references")]
    [SerializeField] private Transform m_firstDoor;
    [SerializeField] private Transform m_secondDoor;

    private bool m_doorOpen;

    private void Start()
	{
		// Assertion
		if (!m_firstDoor || !m_secondDoor) return;

		Animation(m_firstDoor, m_direction, 0f);
		Animation(m_secondDoor, -m_direction, 0f);
	}

    public override void InteractionTrigger()
    {
		base.InteractionTrigger();

		// Assertions
		if (!m_firstDoor || !m_secondDoor) return;
        if (m_doorOpen) return;

		Animation(m_firstDoor, -m_direction, m_duration);
		Animation(m_secondDoor, m_direction, m_duration);
		m_doorOpen = true;
    }

    private void Animation(Transform transform, Vector3 direction, float duration)
    {
		m_firstDoor.DOMove(transform.position + direction, duration);
	}
}