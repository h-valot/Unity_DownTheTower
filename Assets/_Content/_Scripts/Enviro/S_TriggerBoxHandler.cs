using UnityEngine;

public class TriggerBoxHandler : MonoBehaviour
{
    [SerializeField] private ToxicGas _toxicGas;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<OldCharacterMotor>(out var _character))
        {
            _toxicGas.CharacterHasEnter(_character);
        }

        if (other.TryGetComponent<Torch>(out var _torch))
        {
            _toxicGas.TorchHasEnter(_torch);        
        }
    }
}