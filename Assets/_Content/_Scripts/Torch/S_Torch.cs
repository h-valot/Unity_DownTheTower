using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Torch : MonoBehaviour
{
    [SerializeField] private Light torchLight;
    [SerializeField] private int torchIntensity;
        
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void ChangeLightState()
    {
        if (torchLight.GetComponent<Light>().enabled == true)
        {
            torchLight.GetComponent<Light>().enabled = false;
        }  
        else
        {
            torchLight.GetComponent<Light>().enabled = true;
        }
        return;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("e"))
        {
            ChangeLightState();
        }
    }
}
