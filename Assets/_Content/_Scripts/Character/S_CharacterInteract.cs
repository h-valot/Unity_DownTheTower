using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterInteract : MonoBehaviour
{
	[FoldoutGroup("Internal references")][SerializeField] private Transform m_backpackAnchor;
	[FoldoutGroup("Internal references")][SerializeField] private Transform m_characterGraphics;
	[FoldoutGroup("Internal references")][SerializeField] private CharacterMotor m_characterMotor;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Character m_ssoCharacter;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Recycle m_rseRecycle;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_Interact m_rseInteract;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_InputsLocked m_rsoInputsLocked;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CraftInputLocked m_rsoCraftInputLocked;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_RecycleInputLocked m_rsoRecycleInputLocked;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterState m_rsoCharacterState;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_InteractableValid m_rsoInteractableValid;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_InteractableRecyclable m_rsoInteractableRecyclable;

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

	private void Update()
	{
		HandleInvalidInteractables();
		CheckValidity();
		CheckRecyclability();
	}

	private void Interact(bool isPressed)
	{
		// Assertions
		if (m_rsoInputsLocked.value
		|| !isPressed
		|| m_interactables.Count <= 0 
		|| (m_rsoCharacterState.value != BehaviorState.LOCOMOTION 
		&& m_rsoCharacterState.value != BehaviorState.FALL))
		{
			return;
		}

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
		// Assert: Can't add a rope to equip if a rope is already equipped.
		if (interactable as RopeInteractable && m_characterMotor.IsRopeValid) return;

		m_interactables.Add(interactable);
	}

	/// <summary>
	/// 	Remove the given interactable from the interactable list.
	/// 	The character will no longer be able to interact with it.
	/// </summary>
	public void Remove(Interactable interactable, bool doRecycle = false)
	{
		m_interactables.Remove(interactable);

		if (doRecycle) interactable.Recycle();
	}

	/// <summary>
	/// 	Return the nearest interactable in front of the character.
	/// </summary>
	private Interactable GetNearestInteractable()
	{
		// Assertion
		if (m_interactables.Count <= 0) return null;

		// - Get the nearest interactable object from the character -
		var nearest = m_interactables.FirstOrDefault();

		if (!nearest)
		{
			Remove(nearest);
			return null;
		}

		foreach (var interactable in m_interactables)
		{
			if (!interactable) 
			{
				Remove(interactable);
				continue;
			}

			if ((interactable.transform.position - transform.position).sqrMagnitude < (nearest.transform.position - transform.position).sqrMagnitude)
			{
				nearest = interactable;
			}
		}
		return nearest;
	}

	/// <summary>
	/// Remove invalid interactables from the interactables list.
	/// </summary>
	private void HandleInvalidInteractables()
	{
		// Assertion
		if (m_interactables.Count <= 0) return;

		for (int i = m_interactables.Count - 1; i >= 0; i--)
		{
			// Assert: Can't add a rope to equip if a rope is already equipped.
			if (m_interactables[i] as RopeInteractable && m_characterMotor.IsRopeValid) Remove(m_interactables[i]);
		}
	}

	private void CheckValidity()
	{
		// Assertion
		if (m_interactables == null) return;

		m_rsoInteractableValid.value =
			m_interactables.Count > 0 && (m_rsoCharacterState.value == BehaviorState.LOCOMOTION || m_rsoCharacterState.value == BehaviorState.FALL)
			? GetNearestInteractable().Type
			: InteractableType.NONE;
	}

	private void CheckRecyclability()
	{
		m_rsoInteractableRecyclable.value =
			m_interactables.Where(i => i.IsRecyclable).ToList().Count > 0
			&& m_rsoCharacterState.value == BehaviorState.LOCOMOTION;
	}

	private void GetBackpackDebug()
	{
		// Assertion
		if (!m_ssoCharacter.StartWithBag) return;

		m_backpack = FindAnyObjectByType<Backpack>();
		if (m_backpack == null) m_backpack = Instantiate(m_ssoCharacter.PfBackpack);
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

		m_characterMotor.BagRobotSocket = m_backpack.RobotSocket;
		m_characterMotor.BagCraftSocket = m_backpack.CraftSocket;
    }
}