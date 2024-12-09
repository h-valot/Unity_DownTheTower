using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    [SerializeField] private GuardianMotor _GuardianRef;

    public void OnTriggerEnter(Collider other)
    {
        return;

        if (other.TryGetComponent<CharacterMotor>(out var character))
        {
            _GuardianRef.RemovePotentialTargets(character.gameObject);
			//StartCoroutine(_GuardianRef.KillPlayer(character));
		}

		if (other.TryGetComponent<Torch>(out var torch))
        {
            if (torch != null)
            {
                if (!torch.StateInHand())
                {
                    {
                        _GuardianRef.RemovePotentialTargets(torch.gameObject);
                        _GuardianRef.DestroyTorch(torch);
                    }
                }
            }


        } 
    }
}
