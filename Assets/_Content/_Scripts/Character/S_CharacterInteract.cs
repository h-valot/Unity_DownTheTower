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
	[SerializeField] private RSO_CraftInputLocked m_rsoCraftInputLocked;
	[SerializeField] private RSO_RecycleInputLocked m_rsoRecycleInputLocked;
	[SerializeField] private RSO_CharacterState m_rsoCharacterState;
	[SerializeField] private RSO_InteractableValid m_rsoInteractableValid;
	[SerializeField] private RSO_InteractableRecyclable m_rsoInteractableRecyclable;

	[SerializeField] private List<Interactable> m_interactables = new List<Interactable>();
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

	private void Update()
	{
		UpdateValidity();
		CheckInteractableValidity();
		CheckInteractibleRecyclability();
	}

	private void Interact()
	{
		// Assertion
		if (m_interactables.Count <= 0 || m_rsoCharacterState.value != BehaviorState.LOCOMOTION) return;

		GetNearestInteractable()?.InteractionTrigger();
	}

	private void Recycle()
	{
		// Assertion
		if (m_interactables.Count <= 0 || m_rsoCharacterState.value != BehaviorState.LOCOMOTION) return;

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

		if (doRecycle) interactable.Recycle();
	}

	/// <summary>
	/// 	Return the nearest interactable in front of the character.
	/// </summary>
	private Interactable GetNearestInteractable()
	{
		// Assertion
		if (m_interactables.Count(i => i.IsValid) <= 0) return null;

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

	private void UpdateValidity()
	{
		// Assertion
		if (m_interactables.Count <= 0) return;

		// - Get interactable in front of the character -
		foreach (var interactable in m_interactables)
		{
			// Assertion
			if (!interactable) continue;

			Vector3 towardsInteract = interactable.transform.position - transform.position;

			interactable.IsValid = Vector3.Dot(
				new Vector3(m_characterGraphics.transform.forward.x, 0, m_characterGraphics.transform.forward.z).normalized,
				new Vector3(towardsInteract.x, 0, towardsInteract.z).normalized
			) > 0.5f;
		}
	}

	private void CheckInteractableValidity()
	{
		m_rsoInteractableValid.value =
			m_interactables.Count(i => i.IsValid) > 0
			&& m_rsoCharacterState.value == BehaviorState.LOCOMOTION;
	}

	private void CheckInteractibleRecyclability()
	{
		m_rsoInteractableRecyclable.value =
			m_interactables.Where(i => i.IsRecyclable).Count(i => i.IsValid) > 0
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
		m_rsoCraftInputLocked.value = true;
		m_rsoRecycleInputLocked.value = true;

		m_backpack.transform.SetParent(m_backpackAnchor.transform, false);
		m_backpack.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		m_backpack.transform.localScale = Vector3.one;
		m_backpack.ToggleCollider(false);
	}
}