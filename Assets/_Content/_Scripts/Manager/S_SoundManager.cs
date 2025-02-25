using Sirenix.OdinInspector;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_PlaySound m_rsePlaySound;
    [SerializeField] private RSE_PlayAt m_rsePlayAt;

    [FoldoutGroup("References")]
    [SerializeField] private AudioSource m_musicSource_1;
    [SerializeField] private AudioSource m_musicSource_2;
    [SerializeField] private AudioSource m_audioSource_Once;
    [SerializeField] private AudioSource m_audioSource_Endless;

    private void OnEnable()
    {
        m_rsePlaySound.action += PlaySound;
        m_rsePlayAt.action += PlayAt;
    }

    private void OnDisable()
    {
        m_rsePlaySound.action -= PlaySound;
        m_rsePlayAt.action -= PlayAt;
    }

    private void PlayMusic(SSO_Sound sound)
    {

    }
    private void PlaySound(SSO_Sound sound)
    {
        m_audioSource_Once.clip = sound.Clip;
        m_audioSource_Once.Play();
    }

    private void PlayAt(SSO_Sound sound, Vector3 place)
    {

    }
}