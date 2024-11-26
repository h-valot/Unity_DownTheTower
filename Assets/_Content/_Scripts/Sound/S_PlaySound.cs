using UnityEngine;

public class PlaySound : MonoBehaviour
{
    private AudioSource _audioData;

    private void Start()
    {
        _audioData = GetComponent<AudioSource>();
        _audioData.Play(0);
        Destroy(gameObject, 5f);
    }
}