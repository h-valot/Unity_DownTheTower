using Sirenix.OdinInspector;
using UnityEngine;

public class SoundTriggerBoxManager : MonoBehaviour
{
    [SerializeField] private SSO_Sound m_sound;

    [FoldoutGroup("Scriptable")][SerializeField] private RSE_PlaySound RSE_PlaySound;

    public void OnTriggerEnter(Collider collider)
    {
		if (collider.TryGetComponent<CharacterMotor>(out var character))
		{
			RSE_PlaySound.Call(m_sound);
		}
    }
}
