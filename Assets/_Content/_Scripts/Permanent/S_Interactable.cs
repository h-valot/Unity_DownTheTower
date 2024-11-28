using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class Interactable : MonoBehaviour
{
	[Title("Interactible")]
	[Tooltip("If set as true destroys the `Object To Recycle` on interaction triggered. Otherwise, don't.")]
	[SerializeField] private bool m_isRecyclable;

	[EnableIf("m_isRecyclable")]
	[Tooltip("The object that will be destroy when interacted if `Is Recyclable` is true.")]
	[SerializeField] private GameObject m_objectToRecycle;

	[SerializeField] private bool m_displayGizmos;

	public Action OnInteracted;
	public bool IsValid { get; set; }
	public bool IsRecyclable => m_isRecyclable && m_objectToRecycle != null;

	/// <summary>
	/// 	Called when the interactable is getting interacted.
	/// </summary>
	public virtual void InteractionTrigger() 
	{
		OnInteracted?.Invoke();
	}

	/// <summary>
	/// 	Called when a collider enters the interactable's collider. 
	/// 	If it's the character, add itseft to the character's interactable list.
	/// </summary>
    public virtual void OnTriggerEnter(Collider collider)
    {
		if (collider.TryGetComponent<CharacterInteract>(out var character))
		{
			character.Add(this);
		}
	}

	/// <summary>
	/// 	Called when a collider enters the interactable's collider. 
	/// 	If it's the character, remove itseft from the character's interactable list.
	/// </summary>
	public virtual void OnTriggerExit(Collider collider)
	{
		if (collider.TryGetComponent<CharacterInteract>(out var character))
		{
			character.Remove(this);
		}
	}

	/// <summary>
	/// 	Destroy the object to recycle.
	/// </summary>
	public void Recycle()
	{
		Destroy(m_objectToRecycle.gameObject);
	}

#if UNITY_EDITOR

	private void OnDrawGizmos()
	{
		// Assertion
		if (!m_displayGizmos) return;

		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(transform.position, 0.25f);
	}

#endif
}