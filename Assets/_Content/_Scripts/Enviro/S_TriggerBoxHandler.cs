using UnityEngine;

public class TriggerBoxHandler : MonoBehaviour
{
    [SerializeField] private ToxicGas m_toxicGas;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NewCharacterMotor>(out var character))
        {
            m_toxicGas.CharacterHasEnter(character);
        }

        if (other.TryGetComponent<Torch>(out var torch))
        {
            m_toxicGas.TorchHasEnter(torch);        
        }
    }
}