using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlaySound : MonoBehaviour
{
    private AudioSource audioData;

    void Start()
    {
        audioData = GetComponent<AudioSource>();
        audioData.Play(0);
        Destroy(gameObject, 5f);
    }
}
