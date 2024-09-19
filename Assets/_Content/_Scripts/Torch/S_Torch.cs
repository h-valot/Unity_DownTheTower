using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Torch : MonoBehaviour
{
    [Header("Internal References")]
    [SerializeField] private Light _torchLight;
    [SerializeField] private Rigidbody _torchRigidbody;
    [SerializeField] private MeshRenderer _litBody;
    [SerializeField] private Material _litMaterial;
    [SerializeField] private Material _unlitMaterial;
    [SerializeField] private TorchConfig _torchConfig;

    // ----- PREVISUALIZER -----


    // ----- PRIVATE VARIABLES -----
    private bool _canThrow = true;
    private bool _isActive = false;

    private void Start()
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
                StartCoroutine(UnlitTorch(_torchConfig.timeToUnlit));
            }
            else
            {
                StartCoroutine(LitTorch(_torchConfig.timeToLit));
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

    IEnumerator LitTorch(int _time)
    {
        yield return new WaitForSeconds(_time);
        _torchLight.enabled = true; // Si Unlit, lit la torche
        _litBody.material = _litMaterial;
    }

    IEnumerator UnlitTorch(int _time)
    {
        yield return new WaitForSeconds(_time);
        _torchLight.enabled = false;
        _litBody.material = _unlitMaterial;
    }

    private IEnumerator WaitAndDestroyTorch(int _time)
    {
        yield return new WaitForSeconds(_time);
        Destroy(gameObject);
    }
}
