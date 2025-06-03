using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
	[FoldoutGroup("Tweakable values")][SerializeField] private SSO_Sound m_mainTitleMusic;
    [FoldoutGroup("Tweakable values")][SerializeField] private float m_fadeDuration = 1f;

    [FoldoutGroup("Internal references")][SerializeField] private AudioSource m_musicSourceA;
    [FoldoutGroup("Internal references")][SerializeField] private AudioSource m_musicSourceB;
    [FoldoutGroup("Internal references")][SerializeField] private AudioSource m_sfxSourceGlobal;
    [FoldoutGroup("Internal references")][SerializeField] private AudioSource m_sfxSourceRope;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_PlaySound m_rsePlaySound;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_PlaySoundAt m_rsePlaySoundAt;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_StopSound m_rseStopSound;

	private bool isMusicSourceAMain = false;

    private void Start()
    {
		m_musicSourceA.volume = 0f;
		m_musicSourceB.volume = 0f;
        isMusicSourceAMain = true;
        m_musicSourceA.clip = m_mainTitleMusic.Clip;
        m_musicSourceA.pitch = m_mainTitleMusic.Pitch;
        m_musicSourceA.loop = m_mainTitleMusic.Loop;
        PlayAudioSourceFadeIn(m_musicSourceA, 3f, m_mainTitleMusic);
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
        DOTween.Kill(this);
    }

	private void OnPlaySound(SSO_Sound sound)
	{
		switch (sound.Type)
		{
			case SoundType.MUSIC:
				DOTween.Kill(this);
				if (!isMusicSourceAMain)
				{
					isMusicSourceAMain=true;
                    m_musicSourceA.clip = sound.Clip;
                    m_musicSourceA.pitch = sound.Pitch;
                    m_musicSourceA.loop = sound.Loop;
                    PlayAudioSourceFadeIn(m_musicSourceA, m_fadeDuration, sound);
					StopAudioSourceFadeOut(m_musicSourceB, m_fadeDuration);
				}
				else
				{
                    isMusicSourceAMain = false;
                    m_musicSourceB.clip = sound.Clip;
                    m_musicSourceB.pitch = sound.Pitch;
                    m_musicSourceB.loop = sound.Loop;
                    PlayAudioSourceFadeIn(m_musicSourceB, m_fadeDuration, sound);
					StopAudioSourceFadeOut(m_musicSourceA, m_fadeDuration);
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

	private void PlayAudioSourceFadeIn(AudioSource audioSource, float duration, SSO_Sound sound)
    {
		audioSource.Play();
        audioSource.DOFade(sound.Volume, duration).SetEase(Ease.Linear).SetId(this);
    }

	private void StopAudioSourceFadeOut(AudioSource audioSource, float duration)
	{
        audioSource.DOFade(0f, duration).SetEase(Ease.Linear).SetId(this).OnComplete(() => { audioSource.Stop(); });
    }
}