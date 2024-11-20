using DG.Tweening;
using UnityEngine;

public class Backpack : Interactable
{
    [Header("Internal Variables")]
    [SerializeField] private MeshRenderer m_mesh;
    [SerializeField] private SphereCollider m_sphereCollider;

    [Header("Scriptable references")]
    [SerializeField] private NewCharacterConfig m_characterConfig;
	[Space(5)]
	[SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;

	private NewCharacterMotor m_character;
    private bool m_isPickedUp = false;
    private Vector3 m_startPosition;
    private Vector3 m_startRotation;
    private Vector3 m_startScale;

    private void Awake()
    {
        m_startPosition = transform.position;
        m_startRotation = transform.eulerAngles;
        m_startScale = transform.localScale;
        m_mesh.material.SetFloat("_craftingPercent", 1f);
    }

    private void OnEnable()
    {
        m_rsoCharacterDeath.OnChanged += ResetBackpack;
    }

    private void OnDisable()
    {
        m_rsoCharacterDeath.OnChanged -= ResetBackpack;
    }

    public override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out m_character) 
		&& !m_isPickedUp)
        {
            m_character.Add(interactable: this);
        }
    }

    public override void InteractionTrigger()
    {
        m_isPickedUp = true;
        m_character.Pickup(this);
        m_sphereCollider.enabled = false;
    }

    private void ResetBackpack()
    {
        if (m_rsoCharacterDeath.value) return;

		transform.position = m_startPosition;
		transform.rotation = Quaternion.Euler(m_startRotation);
		transform.localScale = m_startScale;
		m_isPickedUp = false;
		m_sphereCollider.enabled = true;
    }

	public void ForceSetupBackpack(NewCharacterMotor character)
	{
		m_character = character;
		InteractionTrigger();
	}

	public void StartCrafting(float craftTime)
    {
        m_mesh.material.DOFloat(0f, "_craftingPercent", craftTime)
					   .SetEase(Ease.Linear)
					   .SetId(gameObject.GetInstanceID() +"craftingPercent");
    }

    public void EndCrafting()
    {
        DOTween.Kill(gameObject.GetInstanceID() + "craftingPercent");
        m_mesh.material.SetFloat("_craftingPercent", 1f);
    }
}