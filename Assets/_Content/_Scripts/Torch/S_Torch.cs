using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class S_Torch : MonoBehaviour
{
    [Header("Internal References")]
    [SerializeField] private Light _torchLight;
    [SerializeField] private Rigidbody _torchRigidbody;
    [SerializeField] private MeshRenderer _litBody;
    [SerializeField] private Material _litMaterial;
    [SerializeField] private Material _unlitMaterial;
    [SerializeField] private TorchConfig _torchConfig;

    // ----- PRIVATE VARIABLES -----
    private bool _isActive;

  

    // Start is called before the first frame update²
    void Start()
    {
        _torchLight.intensity = _torchConfig.torchIntensity;
        _torchRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        _isActive = true;
    }

    public void ChangeLightState() // Function to call to lit or unlit the torch
    {
        if (_isActive == true)
        {
            if (_torchLight.enabled == true) // Si lit, unlit la torche
            {
                _torchLight.enabled = false;
                _litBody.material = _unlitMaterial;
                
            }
            else
            {
                _torchLight.enabled = true; // Si Unlit, lit la torche
                _litBody.material = _litMaterial;
            }
            return;
        }
    }

    public void ThrowTorch(Vector3 _throwForward)
    {
        if (_isActive == true)
        {
            if (_torchConfig.canThrow == true)
            {
                gameObject.transform.parent = null;
                _torchRigidbody.constraints = RigidbodyConstraints.None;
                gameObject.GetComponent<Rigidbody>().velocity = _throwForward * 10;
                _isActive = false;

                StartCoroutine(WaitAndDestroyTorch(_torchConfig.litDuration));
            }
        }
    }

    IEnumerator WaitAndDestroyTorch(int _time)
    {
        yield return new WaitForSeconds(_time);
        Destroy(gameObject);
    }
}
