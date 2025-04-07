using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DecalFixer : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(NextFrameDeactivate());
    }

    IEnumerator NextFrameDeactivate()
    {
        yield return new WaitForSeconds(0);
        GetComponent<DecalProjector>().enabled = false;
        StartCoroutine(NextFrameActivate());
    }

    IEnumerator NextFrameActivate()
    {
        yield return new WaitForSeconds(0);
        GetComponent<DecalProjector>().enabled = true;
    }
}
