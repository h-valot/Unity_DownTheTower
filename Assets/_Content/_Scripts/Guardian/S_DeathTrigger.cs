using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    [SerializeField] private Guardian _GuardianRef;

    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out var character))
        {
            _GuardianRef.RemovePotentialTargets(character.gameObject);
			//StartCoroutine(_GuardianRef.KillPlayer());
			//character.HandleDeath();
		}

		if (other.TryGetComponent<Torch>(out var torch))
        {
            if (torch!=null)

                if (!torch.StateInHand())
                {
                    {
                        _GuardianRef.RemovePotentialTargets(torch.gameObject);
                        Debug.Log("Je vire la ref");
                    }
                    _GuardianRef.destroyTorchCoroutine = StartCoroutine(_GuardianRef.DestroyTorchTime(torch.gameObject));
                }
        } 
    }
}
