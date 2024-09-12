using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_SoundManager : MonoBehaviour
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

    private void PlaySound(Sound sound)
    {

    }
}
