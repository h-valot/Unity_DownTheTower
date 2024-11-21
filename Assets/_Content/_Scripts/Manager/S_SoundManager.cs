using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private RSE_PlaySound _rsePlaySound;

    private void OnEnable()
    {
        _rsePlaySound.action += PlaySound;
    }

    private void OnDisable()
    {
        _rsePlaySound.action -= PlaySound;
    }

    private void PlaySound(SoundConfig sound)
    {

    }
}
