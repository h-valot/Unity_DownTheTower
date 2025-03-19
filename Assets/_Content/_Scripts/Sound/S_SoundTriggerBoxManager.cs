using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_SoundTriggerBoxManager : MonoBehaviour
{
    [SerializeField] private RSE_PlayMusic m_rsePlayMusic;
    [SerializeField] private SSO_Sound m_soundToPlay;
    public void TriggerEnter()
    {
        m_rsePlayMusic.Call(m_soundToPlay);
    }

    public void TriggerExit()
    {
        print("bye");
    }
}
