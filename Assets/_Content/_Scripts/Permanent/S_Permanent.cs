using UnityEngine;

public class Permanent : MonoBehaviour
{
	public CharacterMotor.CraftType _craftType;

	public virtual void InitializePreview() { }

	public virtual void PreviewThrow(Transform _cameraTransform) { }

	public virtual bool Throw(Transform _cameraTransform) 
	{
		return false;
	}

	public virtual void ToggleInHand() { }
	
	public virtual bool StateInHand() 
	{ 
		return false; 
	}
}