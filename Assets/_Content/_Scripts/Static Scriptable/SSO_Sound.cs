using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Sound", menuName = "Static Scriptable/Sound")]
public class SSO_Sound : ScriptableObject
{
    public SoundType Type;

	public bool Loop;

	[Title("Clip")]
	[InfoBox("If true, the returned audio clip will be a random one in the `Clips` list. Otherwise, the provided one.", InfoMessageType.None)]
	[SerializeField] private bool m_randomizeClip;

	[HideIf("m_randomizeClip")]
	[SerializeField] private AudioClip m_clip;

	[ShowIf("m_randomizeClip")]
	[SerializeField] private List<AudioClip> m_clips = new List<AudioClip>();

	public AudioClip Clip
	{
		get
		{
			if (m_randomizeClip) return m_clips[UnityEngine.Random.Range(0, m_clips.Count - 1)];
			else return m_clip;
		}

		private set
		{
			Clip = value;
		}
	}


	[Title("Volume")]
	[InfoBox("If true, the return volume value will be a random value in the provided range. Otherwise, the provided one.", InfoMessageType.None)]
	[SerializeField] private bool m_randomizeVolume;

	[HideIf("m_randomizeVolume")]
	[Range(0f, 1f)]
	[SerializeField] private float m_volumeValue = 1f;

	[ShowIf("m_randomizeVolume")]
	[MinMaxSlider(0f, 1f, true)]
	[SerializeField] private Vector2 m_volumeRange = new Vector2();

	public float Volume
	{
		get
		{
			if (m_randomizeVolume) return UnityEngine.Random.Range(m_volumeRange.x, m_volumeRange.y);
			else return m_volumeValue;
		}

		private set
		{
			Volume = value;
		}
	}


	[Title("Pitch")]
	[InfoBox("If true, the return pitch value will be a random value in the provided range. Otherwise, the provided one.", InfoMessageType.None)]
	[SerializeField] private bool m_randomizePitch;

	[HideIf("m_randomizePitch")]
	[Range(-3f, 3f)]
	[SerializeField] private float m_pitchValue = 1f;

	[ShowIf("m_randomizePitch")]
	[MinMaxSlider(-3f, 3f, true)]
	[SerializeField] private Vector2 m_pitchRange = new Vector2();

	public float Pitch 
	{ 
		get 
		{
			if (m_randomizePitch) return UnityEngine.Random.Range(m_pitchRange.x, m_pitchRange.y);
			else return m_pitchValue;
		} 

		private set
		{
			Pitch = value;
		}
	}
}