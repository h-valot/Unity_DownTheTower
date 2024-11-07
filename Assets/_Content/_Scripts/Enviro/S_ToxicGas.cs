using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ToxicGas: MonoBehaviour
{
    [SerializeField] private List<ExplosiveMushroom> _mushroomsList;
    [SerializeField] private GameObject _gaz;
    [SerializeField] private ToxicConfig _toxicConfig;
    [SerializeField] private CharacterMotor _characterMotor;
    [SerializeField] private RSE_KillCharacter _rseKillCharacter;


    public void TorchHasEnter(Torch _torch)
    {
        if (_torch._isInHand)
        {
            _rseKillCharacter.Call();
        }

        else
        {
            Destroy(_torch.gameObject);
            _gaz.SetActive(false);
            foreach (var mushroom in _mushroomsList)
            {
                mushroom.Explode();
            }
            StartCoroutine(TimetoRefill(_toxicConfig.cooldownToRefill));

        }
    }

    public void CharacterHasEnter(CharacterMotor _character)
    {
        _rseKillCharacter.Call();
    }

    private IEnumerator TimetoRefill(float _cooldown)
    {
        yield return new WaitForSeconds(_cooldown);
        _gaz.SetActive(true);

        foreach (var mushroom in _mushroomsList)
        {
            mushroom.Refilled();
        }
    }
}
