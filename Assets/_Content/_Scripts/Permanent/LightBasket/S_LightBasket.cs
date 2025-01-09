using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class LightBasket : MonoBehaviour
{
    [SerializeField] private LightDoor _parentDoor;
    [SerializeField] private GameObject _permanentLight;
    [SerializeField] private float _maxVelocitySnap;

    private bool _isFilled = false;
    private List<Rigidbody> torches = new List<Rigidbody>();

    private void Update()
    {
        if (_isFilled) return;

        foreach (Rigidbody torch in torches)
        {
            if (torch.velocity.magnitude <= _maxVelocitySnap) ActivateBasket(torch);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isFilled) return;

        if (other.gameObject.TryGetComponent<Torch>(out Torch torch))
        {
            torches.Add(other.gameObject.GetComponent<Rigidbody>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<Torch>(out Torch torch))
        {
            torches.Remove(other.gameObject.GetComponent<Rigidbody>());
        }
    }

    private void ActivateBasket(Rigidbody torch)
    {
        _isFilled = true;
        torches.Remove(torch);
        Destroy(torch.gameObject);
        Instantiate(_permanentLight, transform.position, Quaternion.identity, transform); 
    }
}
