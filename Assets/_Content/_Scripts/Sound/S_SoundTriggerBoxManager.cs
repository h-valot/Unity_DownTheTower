using Sirenix.OdinInspector;
using UnityEngine;

public class SoundTriggerBoxManager : MonoBehaviour
{
    [SerializeField] private SSO_Sound m_sound;
    [SerializeField] private SSO_Sound m_ambiance;

    [FoldoutGroup("Scriptable")][SerializeField] private RSE_PlaySound RSE_PlaySound;

    public void OnTriggerEnter(Collider collider)
    {
		if (collider.TryGetComponent<CharacterMotor>(out var character))
		{
			RSE_PlaySound.Call(m_sound);
		}
    }

    public void OnTriggerExit(Collider collider)
    {
        if (collider.TryGetComponent<CharacterMotor>(out var character))
        {
            RSE_PlaySound.Call(m_ambiance);
        }
    }
}
