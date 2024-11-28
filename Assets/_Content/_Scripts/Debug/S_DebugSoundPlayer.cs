using UnityEngine;

public class DebugSoundPlayer : MonoBehaviour
{
    private AudioSource m_audioData;

    private void Start()
    {
        m_audioData = GetComponent<AudioSource>();
        m_audioData.Play(0);
        Destroy(gameObject, 5f);
    }
}