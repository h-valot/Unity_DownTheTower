using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	[FoldoutGroup("Tweakable values")][SerializeField] private SSO_Sound m_ambianceStart;
	[FoldoutGroup("Tweakable values")][SerializeField] private SSO_Sound m_ambianceLoop;
	[FoldoutGroup("Tweakable values")][SerializeField] private SSO_Sound m_ambianceTail;

	[FoldoutGroup("Internal references")][SerializeField] private AudioSource m_musicSourceA;
    [FoldoutGroup("Internal references")][SerializeField] private AudioSource m_musicSourceB;
    [FoldoutGroup("Internal references")][SerializeField] private AudioSource m_sfxSourceGlobal;
    [FoldoutGroup("Internal references")][SerializeField] private AudioSource m_sfxSourceRope;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_PlaySound m_rsePlaySound;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_PlaySoundAt m_rsePlaySoundAt;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_StopSound m_rseStopSound;

    private void Start()
    {
        //OnPlaySound(m_ambianceStart);
    }

    private void OnEnable()
    {
        m_rsePlaySound.action += OnPlaySound;
        m_rsePlaySoundAt.action += OnPlaySoundAt;
		m_rseStopSound.action += OnStopSound;
    }

    private void OnDisable()
	{
		m_rsePlaySound.action -= OnPlaySound;
		m_rsePlaySoundAt.action -= OnPlaySoundAt;
		m_rseStopSound.action -= OnStopSound;
	}

	private void OnPlaySound(SSO_Sound sound)
	{
		switch (sound.Type)
		{
			case SoundType.MUSIC:
				if (m_musicSourceA.clip != sound.Clip)
				{
					SyncSource(m_musicSourceA, sound);
					PlayAudioSource(m_musicSourceA);


                    SyncSource(m_musicSourceB, m_ambianceLoop);
					m_musicSourceB.PlayDelayed(sound.Clip.length);
				}
				break;

			case SoundType.SFX_GLOBAL:
				SyncSource(m_sfxSourceGlobal, sound);
				m_sfxSourceGlobal.Play();
				break;

			case SoundType.SFX_ROPE:
				if (!m_sfxSourceRope.isPlaying)
				{
					SyncSource(m_sfxSourceRope, sound);
					m_sfxSourceRope.Play();
				}
				break;
		}
	}

	private void OnPlaySoundAt(SSO_Sound sound, Vector3 position)
	{
		//SyncSource(m_sfxSourceGlobal, sound);
		AudioSource.PlayClipAtPoint(sound.Clip, position, sound.Volume);
	}

	private void OnStopSound(SSO_Sound sound)
	{
		switch (sound.Type)
		{
			case SoundType.MUSIC:
				if (m_musicSourceA.isPlaying
				&& m_musicSourceA.clip == sound.Clip)
				{
					m_musicSourceA.Stop();
				}

				if (m_musicSourceB.isPlaying
				&& m_musicSourceB.clip == sound.Clip)
				{
					m_musicSourceB.Stop();
				}
				break;

			case SoundType.SFX_ROPE:
				if (m_sfxSourceRope.isPlaying)
				{
					m_sfxSourceRope.Stop();
				}
				break;
		}
	}

	private void SyncSource(AudioSource audioSource, SSO_Sound sound)
	{
		audioSource.clip = sound.Clip;
		audioSource.volume = sound.Volume;
		audioSource.pitch = sound.Pitch;
		audioSource.loop = sound.Loop;
	}

	private IEnumerator FadeIn(AudioSource audioSource, float volume)
    {
        float timer = 0;
        float duration = 4;
        float originalVolume = audioSource.volume;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(originalVolume, volume, 5f);
        }
       yield return null;
    }

	private void PlayAudioSource(AudioSource audioSource)
	{
		audioSource.Play();
	}
}