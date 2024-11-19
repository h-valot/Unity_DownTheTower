using System;
using NaughtyAttributes;
using UnityEngine;

public class Interactible : MonoBehaviour
{
	[Header("Interactible")]
	[Tooltip("If set as true destroys the `Object To Recycle` on interaction triggered. Otherwise, don't.")]
    public bool isRecyclable;

	[EnableIf("isRecyclable")]
	[Tooltip("The object that will be destroy when interacted if `Is Recyclable` is true.")]
	public GameObject objectToRecycle;

	[SerializeField] private bool displayGizmos;

	public Action OnInteracted;

    public virtual void InteractionTrigger() 
	{
		OnInteracted?.Invoke();
	}

    public virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<OldCharacterMotor>(out var _character))
        {
            _character.AddToInteractList(this);
        }
    }

    public virtual void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<OldCharacterMotor>(out var _character))
        {
            _character.RemoveFromInteractList(this);
        }
    }

#if UNITY_EDITOR

	private void OnDrawGizmos()
	{
		if (!displayGizmos) return;

		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(transform.position, 0.25f);
	}

#endif
}