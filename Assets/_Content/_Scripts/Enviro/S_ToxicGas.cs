using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ToxicGas: MonoBehaviour
{
    [SerializeField] private List<GameObject> _mushroomsList;
    [SerializeField] private GameObject _gaz;
    [SerializeField] private ToxicConfig _toxicConfig;

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.TryGetComponent<CharacterMotor>(out var _character))
        {
            _character.HandleDeath();
        }

        if (other.TryGetComponent<Torch>(out var _torch))
        {

        }
    }

    public void TorchHasEnter(Torch _torch)
    {
        Destroy(_torch.gameObject);
        _gaz.SetActive(false);
        StartCoroutine(TimetoRefill(_toxicConfig.cooldownToRefill));
    }

    public void CharacterHasEnter(CharacterMotor _character)
    {
        _character.HandleDeath();
    }

    private IEnumerator TimetoRefill(float _cooldown)
    {
        yield return new WaitForSeconds(_cooldown);
        _gaz.SetActive(true);
    }
}
