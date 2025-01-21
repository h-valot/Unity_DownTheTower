using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_TorchSpawner : Switchable
{
    [Header("External References")]
    [SerializeField] private GameObject _spawnedTorchPF;

    private Torch _activeTorch;

    protected override void ActivateMechanism()
    {
        if (_activeTorch != null) return;
        _activeTorch = Instantiate(_spawnedTorchPF, transform.position, Quaternion.identity, transform).GetComponent<Torch>();
    }

    protected override void DeactivateMechanism()
    {
        StartCoroutine(_activeTorch.WaitAndDeactivateTorch(0));
        _activeTorch = null;
    }
}
