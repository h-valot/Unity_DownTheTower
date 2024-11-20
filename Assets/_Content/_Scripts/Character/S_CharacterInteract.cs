using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterInteract : MonoBehaviour
{
	[Header("Internal references")]
	[SerializeField] private Transform m_backpackAnchor;
	[SerializeField] private Transform m_characterGraphics;

	[Header("Scriptable references")]
	[SerializeField] private NewCharacterConfig m_characterConfig;
	[Space(5)]
	[SerializeField] private RSE_Recycle m_rseRecycle;
	[SerializeField] private RSE_Interact m_rseInteract;
	[Space(5)]
	[SerializeField] private RSO_CanCraft m_rsoCanCraft;
	[SerializeField] private RSO_CanRecycle m_rsoCanRecycle;
	[SerializeField] private RSO_CanInteract m_rsoCanInteract;
	[SerializeField] private RSO_CharacterState m_rsoCharacterState;

	private List<Interactable> m_interactables = new List<Interactable>();
	private Backpack m_backpack;

	private void Start()
	{
		GetBackpackDebug();
	}

	private void OnEnable()
	{
		m_rseRecycle.action += Recycle;
		m_rseInteract.action += Interact;
	}

	private void OnDisable()
	{
		m_rseRecycle.action -= Recycle;
		m_rseInteract.action -= Interact;
	}

	private void Interact()
	{
		// Assertion
		if (m_interactables.Count == 0 || m_rsoCharacterState.value != BehaviorState.LOCOMOTION) return;

		GetNearestInteractable()?.InteractionTrigger();
	}

	private void Recycle()
	{
		// Assertion
		if (m_interactables.Count == 0 || m_rsoCharacterState.value != BehaviorState.LOCOMOTION) return;

		Interactable interactable = GetNearestInteractable();

		// Assert: interactable isn't valid
		if (!interactable || !interactable.IsRecyclable) return;

		Remove(interactable, doRecycle: true);
	}

	/// <summary>
	/// 	Add the given interactable into the interactable list.
	/// </summary>
	public void Add(Interactable interactable)
	{
		m_interactables.Add(interactable);
	}

	/// <summary>
	/// 	Remove the given interactable from the interactable list.
	/// 	The character will no longer be able to interact with it.
	/// </summary>
	public void Remove(Interactable interactable, bool doRecycle = false)
	{
		interactable.IsValid = false;
		m_interactables.Remove(interactable);

		CheckShowInteract();
		CheckShowRecycle(false);

		if (doRecycle) interactable.Recycle();
	}

	/// <summary>
	/// 	Return the nearest interactable in front of the character.
	/// </summary>
	private Interactable GetNearestInteractable()
	{
		// - Get interactable in front of the character -
		var counter = 0;
		foreach (var interactable in m_interactables)
		{
			// Assertion
			if (!interactable) continue;

			Vector3 towardsInteract = interactable.transform.position - transform.position;

			interactable.IsValid = Vector3.Dot(
				new Vector3(m_characterGraphics.transform.forward.x, 0, m_characterGraphics.transform.forward.z).normalized,
				new Vector3(towardsInteract.x, 0, towardsInteract.z).normalized
			) > 0.5f;

			if (interactable.IsValid) counter++;
		}

		// Assert: there is no interactable in front of the character.
		if (counter == 0) return null;

		// - Get the nearest interactable object from the character -
		var nearest = m_interactables.FirstOrDefault(i => i.IsValid);
		foreach (var valid in m_interactables.Where(i => i.IsValid))
		{
			if ((valid.transform.position - transform.position).sqrMagnitude <
				(nearest.transform.position - transform.position).sqrMagnitude)
			{
				nearest = valid;
			}
		}
		return nearest;
	}

	private void CheckShowInteract()
	{
		m_rsoCanInteract.value =
			m_interactables.Count(i => i.IsValid) > 0
			&& m_rsoCharacterState.value == BehaviorState.LOCOMOTION;
	}

	private void CheckShowRecycle(bool isRecyclable)
	{
		m_rsoCanRecycle.value = 
			isRecyclable
			&& m_rsoCharacterState.value == BehaviorState.LOCOMOTION;
	}

	private void GetBackpackDebug()
	{
		// Assertion
		if (!m_characterConfig.startWithBag) return;

		m_backpack = FindAnyObjectByType<Backpack>();
		if (m_backpack == null) m_backpack = Instantiate(m_characterConfig.pfBackpack);
		m_backpack.ForceSetupBackpack(this);
	}

	public void Pickup(Backpack backpack)
	{
		m_backpack = backpack;

		Remove(m_backpack);
		m_rsoCanCraft.value = true;
		m_rsoCanRecycle.value = true;

		m_backpack.transform.SetParent(m_backpackAnchor.transform, false);
		m_backpack.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		m_backpack.transform.localScale = Vector3.one;
	}
}