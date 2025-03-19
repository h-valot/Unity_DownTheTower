using Sirenix.OdinInspector;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [FoldoutGroup("Internal references")][SerializeField] private AudioSource m_musicSourceA;
    [FoldoutGroup("Internal references")][SerializeField] private AudioSource m_musicSourceB;
    [FoldoutGroup("Internal references")][SerializeField] private AudioSource m_sfxSourceGlobal;
    [FoldoutGroup("Internal references")][SerializeField] private AudioSource m_sfxSourceRope;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Sound m_ambianceStart;
    [FoldoutGroup("Scriptable")][SerializeField] private SSO_Sound m_ambianceLoop;
    [FoldoutGroup("Scriptable")][SerializeField] private SSO_Sound m_ambianceTail;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_PlaySound m_rsePlaySound;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_PlayAt m_rsePlayAt;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_PlayRope m_rsePlayRope;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_PlayRopeStop m_rsePlayRopeStop;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_PlayMusic m_rsePlayMusic;

    private void OnEnable()
    {
        m_rsePlaySound.action += PlaySound;
        m_rsePlayAt.action += PlaySoundAt;
        m_rsePlayMusic.action += PlayMusic;

        m_rsePlayRope.action += PlayRope;
        m_rsePlayRopeStop.action += StopRope;
    }

    private void OnDisable()
    {
        m_rsePlaySound.action -= PlaySound;
        m_rsePlayAt.action -= PlaySoundAt;
        m_rsePlayMusic.action -= PlayMusic;

        m_rsePlayRope.action -= PlayRope;
        m_rsePlayRopeStop.action -= StopRope;
    }

	private void SyncSource(AudioSource audioSource, SSO_Sound sound)
	{
		audioSource.clip = sound.Clip;
		audioSource.volume = sound.Volume;
		audioSource.pitch = sound.Pitch;
		audioSource.loop = sound.Loop;
	}

	private void Play(AudioSource audioSource, SSO_Sound sound)
	{
		SyncSource(audioSource, sound);
		audioSource.Play();
	}

	private void PlayDelay(AudioSource audioSource, SSO_Sound sound, float delay)
	{
		SyncSource(audioSource, sound);
		audioSource.PlayDelayed(delay);
	}

	private void PlayAt(AudioSource audioSource, SSO_Sound sound, Vector3 position)
	{
		SyncSource(audioSource, sound);
		AudioSource.PlayClipAtPoint(m_sfxSourceGlobal.clip, position);
	}

	private void PlaySound(SSO_Sound sound)
	{
		Play(m_sfxSourceGlobal, sound);
	}

	private void PlaySoundAt(SSO_Sound sound, Vector3 position)
	{
		PlayAt(m_sfxSourceGlobal, sound, position);
	}

	private void PlayMusic(SSO_Sound sound)
    {
		// Assertion
        if (m_musicSourceA.clip == sound.Clip) return;

		Play(m_musicSourceA, sound);
		PlayDelay(m_musicSourceB, m_ambianceLoop, sound.Clip.length);
    }

    private void PlayRope(SSO_Sound sound)
    {
		// Assertions
		if (m_sfxSourceRope.isPlaying) return;
        if (m_sfxSourceRope.clip == sound.Clip) return;

		Play(m_sfxSourceRope, sound);
    }

    private void StopRope(SSO_Sound sound)
    {
		// Assertion
        if (!m_sfxSourceRope.isPlaying) return;

		m_sfxSourceRope.Stop();
    }
}