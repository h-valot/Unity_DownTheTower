using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ToxicGas: MonoBehaviour
{
	[Title("Internal references")]
    [SerializeField] private GameObject m_gaz;
    [SerializeField] private List<ExplosiveMushroom> m_mushrooms;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Toxic m_ssoToxic;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_KillCharacter m_rseKillCharacter;

    public void Enter(Torch torch)
    {
        if (torch.IsInHand)
        {
            m_rseKillCharacter.Call();
        }
        else
        {
            Destroy(torch.gameObject);
            m_gaz.SetActive(false);
            foreach (var mushroom in m_mushrooms)
            {
                mushroom.Explode();
            }
            StartCoroutine(Refill());
        }
    }

    public void Enter(CharacterMotor character)
    {
        m_rseKillCharacter.Call();
    }

    private IEnumerator Refill()
    {
        yield return new WaitForSeconds(m_ssoToxic.Cooldown);
        m_gaz.SetActive(true);

        foreach (var mushroom in m_mushrooms)
        {
            mushroom.Refilled();
        }
    }
}
