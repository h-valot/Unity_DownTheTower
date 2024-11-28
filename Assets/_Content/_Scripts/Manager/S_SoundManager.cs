using Sirenix.OdinInspector;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_PlaySound m_rsePlaySound;

    private void OnEnable()
    {
        m_rsePlaySound.action += PlaySound;
    }

    private void OnDisable()
    {
        m_rsePlaySound.action -= PlaySound;
    }

    private void PlaySound(SSO_Sound sound)
    {
		// TODO
    }
}