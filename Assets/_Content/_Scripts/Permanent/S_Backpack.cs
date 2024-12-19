using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Backpack : Interactable
{
    [Title("Internal Variables")]
    [SerializeField] private SkinnedMeshRenderer m_mesh;
    [SerializeField] private SphereCollider m_sphereCollider;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_BackpackCrafting m_rsoPackbackCrafting;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;

	private CharacterInteract m_characterInteract;
    private Vector3 m_startPosition;
    private Vector3 m_startRotation;
    private Vector3 m_startScale;
    private bool m_isPickedUp;

    private void Awake()
    {
        m_startPosition = transform.position;
        m_startRotation = transform.eulerAngles;
        m_startScale = transform.localScale;
        m_mesh.material.SetFloat("_craftingPercent", 0f);
    }

    private void OnEnable()
    {
        m_rsoCharacterDeath.OnChanged += ResetBackpack;
		m_rsoPackbackCrafting.action += HandleCrafting;
	}

    private void OnDisable()
    {
        m_rsoCharacterDeath.OnChanged -= ResetBackpack;
		m_rsoPackbackCrafting.action -= HandleCrafting;
	}

    public override void OnTriggerEnter(Collider collider)
    {
		// Assertions
		if (m_isPickedUp) return;
        if (!collider.TryGetComponent(out m_characterInteract)) return;
		
		m_characterInteract.Add(this);
    }

    public override void InteractionTrigger()
    {
        m_isPickedUp = true;
		m_characterInteract.Pickup(this);
	}

    private void ResetBackpack()
    {
		// Assertion
        if (m_rsoCharacterDeath.value) return;

		transform.SetPositionAndRotation(m_startPosition, Quaternion.Euler(m_startRotation));
		transform.localScale = m_startScale;

		m_isPickedUp = false;
		m_sphereCollider.enabled = true;
    }

	public void ForceSetupBackpack(CharacterInteract characterInteract)
	{
		m_characterInteract = characterInteract;
		InteractionTrigger();
	}

	public void ToggleCollider(bool isEnabled)
	{
		m_sphereCollider.enabled = isEnabled;
	}

	public void HandleCrafting(bool isStarting, float duration)
	{
		if (isStarting)
		{
            m_mesh.material.DOFloat(1f, "_craftingPercent", duration)
						   .SetEase(Ease.Linear)
						   .SetId(gameObject.GetInstanceID() + "craftingPercent");
		}
		else
		{
			DOTween.Kill(gameObject.GetInstanceID() + "craftingPercent");
            m_mesh.material.DOFloat(0f, "_craftingPercent", duration*2)
                           .SetEase(Ease.InQuint)
                           .SetId(gameObject.GetInstanceID() + "craftingPercent");
        }
	}
}