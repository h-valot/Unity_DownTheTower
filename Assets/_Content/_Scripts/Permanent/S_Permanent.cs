using UnityEngine;

public class Permanent : MonoBehaviour
{
	public CharacterMotor.CraftType _craftType;

	public virtual void PreviewThrow(Vector3 _cameraForward) { }

	public virtual void Throw(Vector3 _cameraForward) { }

	public virtual void ToggleInHand() { }
}