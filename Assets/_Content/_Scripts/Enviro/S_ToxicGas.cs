using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToxicGas: MonoBehaviour
{
    [SerializeField] private List<ExplosiveMushroom> m_mushrooms;
    [SerializeField] private GameObject m_gaz;

    [SerializeField] private SSO_Toxic m_ssoToxic;
    [SerializeField] private SSO_Character m_ssoCharacter;

    [SerializeField] private RSE_KillCharacter m_rseKillCharacter;

    public void TorchHasEnter(Torch _torch)
    {
        if (_torch._isInHand)
        {
            m_rseKillCharacter.Call();
        }
        else
        {
            Destroy(_torch.gameObject);
            m_gaz.SetActive(false);
            foreach (var mushroom in m_mushrooms)
            {
                mushroom.Explode();
            }
            StartCoroutine(TimetoRefill(m_ssoToxic.Cooldown));
        }
    }

    public void CharacterHasEnter(CharacterMotor character)
    {
        m_rseKillCharacter.Call();
    }

    private IEnumerator TimetoRefill(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        m_gaz.SetActive(true);

        foreach (var mushroom in m_mushrooms)
        {
            mushroom.Refilled();
        }
    }
}
