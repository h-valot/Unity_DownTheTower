using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class S_Torch : MonoBehaviour
{
    [Header("Internal References")]
    [SerializeField] private Light _torchLight;
    [SerializeField] private TorchConfig _torchConfig; 

    // ----- PRIVATE VARIABLES -----
    private Rigidbody _torchRigidbody;
    private bool _canThrow = true;

    // Start is called before the first frame update²
    void Start()
    {
        _torchLight.intensity = _torchConfig.torchIntensity;
        _torchRigidbody = gameObject.GetComponent<Rigidbody>();
        _torchRigidbody.constraints = RigidbodyConstraints.FreezeAll;

    }

    public void ChangeLightState() // Function to call to lit or unlit the torch
    {
        if (_canThrow == true)
        {
            if (_torchLight.enabled == true) // Si lit, unlit la torche
            {
                _torchLight.enabled = false;
            }
            else
            {
                _torchLight.enabled = true; // Si Unlit, lit la torche
            }
            return;
        }
    }

    public void ThrowTorch(Vector3 _throwForward)
    {
        if (_canThrow == true)
        {
            gameObject.transform.parent = null;
            _torchRigidbody.constraints = RigidbodyConstraints.None;
            gameObject.GetComponent<Rigidbody>().velocity = _throwForward * 10;
            _canThrow = false;

            StartCoroutine(WaitAndDestroyTorch(_torchConfig.litDuration));
        }

    }

    IEnumerator WaitAndDestroyTorch(int _time)
    {
        yield return new WaitForSeconds(_time);
        Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
