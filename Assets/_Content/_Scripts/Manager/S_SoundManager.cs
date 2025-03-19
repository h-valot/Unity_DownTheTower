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
        OnPlaySound(m_ambianceStart);
    }

    private void OnEnable()
    {
        m_rsePlaySound.action += OnPlaySound;
        m_rsePlaySoundAt.action += PlaySfxAt;
		m_rseStopSound.action += OnStopSound;
    }

    private void OnDisable()
	{
		m_rsePlaySound.action -= OnPlaySound;
		m_rsePlaySoundAt.action -= PlaySfxAt;
		m_rseStopSound.action -= OnStopSound;
	}

	private void SyncSource(AudioSource audioSource, SSO_Sound sound)
	{
		audioSource.clip = sound.Clip;
		audioSource.volume = sound.Volume;
		audioSource.pitch = sound.Pitch;
		audioSource.loop = sound.Loop;
	}

	private void PlayDelay(AudioSource audioSource, SSO_Sound sound, float delay)
	{
		SyncSource(audioSource, sound);
		audioSource.PlayDelayed(delay);
	}

	private void Play(AudioSource audioSource, SSO_Sound sound, Vector3 position = new Vector3())
	{
		SyncSource(audioSource, sound);
		if (position == Vector3.zero)
		{
			audioSource.Play();
		}
		else
		{
			AudioSource.PlayClipAtPoint(m_sfxSourceGlobal.clip, position);
		}
	}

	private void OnPlaySound(SSO_Sound sound)
	{
		switch (sound.Type)
		{
			case SoundType.MUSIC:
				if (m_musicSourceA.clip != sound.Clip)
				{
					Play(m_musicSourceA, sound);
					PlayDelay(m_musicSourceB, m_ambianceLoop, sound.Clip.length);
				}
				break;

			case SoundType.SFX_GLOBAL:
				Play(m_sfxSourceGlobal, sound);
				break;

			case SoundType.SFX_ROPE:
				// Assertions
				if (!m_sfxSourceRope.isPlaying
				|| m_sfxSourceRope.clip != sound.Clip)
				{
					Play(m_sfxSourceRope, sound);
				}
				break;
		}
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

	private void PlaySfxAt(SSO_Sound sound, Vector3 position)
	{
		Play(m_sfxSourceGlobal, sound, position);
	}

    private IEnumerator FadeIn(AudioSource audiosource, float volume)
    {
        float timer = 0;
        float duration = 4;
        float originalVolume = audiosource.volume;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            audiosource.volume = Mathf.Lerp(originalVolume, volume, 5f);
        }
       yield return null;
    }
}