using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class S_Torch : MonoBehaviour
{
    [SerializeField] private Light torchLight;
    [SerializeField] private int torchIntensity;
    private Camera playerCam;
    [SerializeField] private GameObject ThrowTorchPrefab;
    private Rigidbody torchRigidbody;
        
    // Start is called before the first frame update
    void Start()
    {
        playerCam = Camera.main;
        torchRigidbody = gameObject.GetComponent<Rigidbody>();
    }

    void ChangeLightState() // Function to call to lit or unlit the torch
    {
        if (torchLight.GetComponent<Light>().enabled == true) // Si lit, unlit la torche
        {
            torchLight.GetComponent<Light>().enabled = false;
        }  
        else
        {
            torchLight.GetComponent<Light>().enabled = true; // Si Unlit, lit la torche
        }
        return;
    }

    void ThrowTorch(Vector3 throwForward)
    {
        Ray r = playerCam.ScreenPointToRay(Input.mousePosition);

        Vector3 dir = r.GetPoint(1) - r.GetPoint(0);
        torchRigidbody.constraints = RigidbodyConstraints.None;
        ThrowTorchPrefab.GetComponent<Rigidbody>().velocity = throwForward * 5;
        
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("e"))
        {
            ChangeLightState();
        }

        if (Input.GetKeyDown("g"))
        {
            
        }
    }
}
