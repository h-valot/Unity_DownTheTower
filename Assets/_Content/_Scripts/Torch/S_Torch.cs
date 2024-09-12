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
    private bool canThrow = true;
    [SerializeField] private int litDuration;
        
    // Start is called before the first frame update
    void Start()
    {
        playerCam = Camera.main;
        torchRigidbody = gameObject.GetComponent<Rigidbody>();
    }

    public void ChangeLightState() // Function to call to lit or unlit the torch
    {
        Debug.Log("je tente d'éteindre la lumière");
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

    public void ThrowTorch(Vector3 throwForward)
    {
        if (canThrow == true)
        {
            gameObject.transform.parent = null;
            torchRigidbody.constraints = RigidbodyConstraints.None;
            ThrowTorchPrefab.GetComponent<Rigidbody>().velocity = throwForward * 10;
            canThrow = false;

            WaitAndDestroyTorch(5);
        }
        
    }

    IEnumerator WaitAndDestroyTorch(int value)
    {
        Debug.Log("Je commence la coroutine");
        yield return new WaitForSeconds(value);
        Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
