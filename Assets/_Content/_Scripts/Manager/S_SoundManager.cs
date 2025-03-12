using Sirenix.OdinInspector;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_PlaySound m_rsePlaySound;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_PlayAt m_rsePlayAt;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_PlayRope m_rsePlayRope;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_PlayRopeStop m_rsePlayRopeStop;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_PlayMusic m_rsePlayMusic;

    [FoldoutGroup("References")][SerializeField] private AudioSource m_musicSource_1;
    [FoldoutGroup("References")][SerializeField] private AudioSource m_musicSource_2;
    [FoldoutGroup("References")][SerializeField] private AudioSource m_audioSource_Once;
    [FoldoutGroup("References")][SerializeField] private AudioSource m_audioSource_Rope;

    [FoldoutGroup("Sounds")][SerializeField] private SSO_Sound m_ambianceLoop;
    [FoldoutGroup("Sounds")][SerializeField] private SSO_Sound m_ambianceTail;

    private void OnEnable()
    {
        m_rsePlayMusic.action += PlayMusic;
        m_rsePlaySound.action += PlaySound;
        m_rsePlayAt.action += PlayAt;
        m_rsePlayRope.action += PlayRope;
        m_rsePlayRopeStop.action += StopPlayRope;
    }

    private void OnDisable()
    {
        m_rsePlayMusic.action -= PlayMusic;
        m_rsePlaySound.action -= PlaySound;
        m_rsePlayAt.action -= PlayAt;
        m_rsePlayRope.action -= PlayRope;
        m_rsePlayRopeStop.action -= StopPlayRope;
    }

    private void PlayMusic(SSO_Sound sound)
    {
        if (m_musicSource_1.clip != sound.Clip)
        {
            m_musicSource_1.clip = sound.Clip;
            m_musicSource_1.Play();
            m_musicSource_2.clip = m_ambianceLoop.Clip;
            m_musicSource_2.PlayDelayed(sound.Clip.length);
        }

    }
    private void PlaySound(SSO_Sound sound)
    {
        m_audioSource_Once.clip = sound.Clip;
        m_audioSource_Once.Play();
    }

    private void PlayAt(SSO_Sound sound, Vector3 position)
    {
        m_audioSource_Once.clip = sound.Clip;
        AudioSource.PlayClipAtPoint(m_audioSource_Once.clip, position);
    }

    private void PlayRope(SSO_Sound sound)
    {
        if (m_audioSource_Rope.clip != sound.Clip)
        {
            m_audioSource_Rope.clip = sound.Clip;
            m_audioSource_Rope.Play();
        }

        else if (m_audioSource_Rope.isPlaying == false)
        {
            m_audioSource_Rope.clip = sound.Clip;
            m_audioSource_Rope.Play();
        }
    }

    private void StopPlayRope(SSO_Sound sound)
    {
        if (m_audioSource_Rope.isPlaying == true)
        {
            m_audioSource_Rope.Stop();
        }
    }
}