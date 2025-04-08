using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
	[FoldoutGroup("Tweakable values")][SerializeField] private SSO_Sound m_ambianceStart;
	[FoldoutGroup("Tweakable values")][SerializeField] private SSO_Sound m_ambianceLoop;
	[FoldoutGroup("Tweakable values")][SerializeField] private SSO_Sound m_ambianceTail;

	[FoldoutGroup("Internal references")][SerializeField] private AudioSource m_musicSourceA;
    [FoldoutGroup("Internal references")][SerializeField] private AudioSource m_musicSourceB;
    [FoldoutGroup("Internal references")][SerializeField] private AudioSource m_sfxSourceGlobal;
    [FoldoutGroup("Internal references")][SerializeField] private AudioSource m_sfxSourceRope;

    [FoldoutGroup("External references")][SerializeField] private AudioMixer m_audioMixer;

    [FoldoutGroup("Scriptable")][SerializeField] private RSE_PlaySound m_rsePlaySound;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_PlaySoundAt m_rsePlaySoundAt;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_StopSound m_rseStopSound;

    [FoldoutGroup("Scriptable")][SerializeField] private SSO_Settings m_ssoSettings;

    private void Start()
    {
		InitializeVolume();
        OnPlaySound(m_ambianceStart);
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

	private void InitializeVolume()
	{
		m_audioMixer.SetFloat("Master", m_ssoSettings.MasterVolumeDB);
        m_audioMixer.SetFloat("Music", m_ssoSettings.MusicVolumeDB);
        m_audioMixer.SetFloat("SFX", m_ssoSettings.SfxVolumeDB);
    }
	private void OnPlaySound(SSO_Sound sound)
	{
		switch (sound.Type)
		{
			case SoundType.MUSIC:
				if (m_musicSourceA.clip != sound.Clip)
				{
					SyncSource(m_musicSourceA, sound);
					m_musicSourceA.Play();

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
		PlayClipAtPoint(sound.Clip, position, sound.Volume, sound.Output);
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

    public static void PlayClipAtPoint(AudioClip clip, Vector3 position, [UnityEngine.Internal.DefaultValue("1.0F")] float volume, AudioMixerGroup group)
    {
        GameObject gameObject = new GameObject("One shot audio");
        gameObject.transform.position = position;
        AudioSource audioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
        audioSource.clip = clip;
        audioSource.spatialBlend = 1f;
        audioSource.volume = volume;
		audioSource.outputAudioMixerGroup = group;
        audioSource.Play();
        Object.Destroy(gameObject, clip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
    }

}