using UnityEngine;

public class BackpackPickup : Interactible
{
    [Header("Internal Variables")]
    [SerializeField] private MeshRenderer _mesh;

    [Header("External Variables")]
    [SerializeField] private RSO_PlayerDeath _rsoPlayerDeath;

    [Header("Scriptable references")]
    [SerializeField] private CharacterConfig _characterConfig;

    // PRIVATE VARIABLES
    private CharacterMotor _character;
    private bool _isAvailable = true;

    private void Start()
    {
        if (_characterConfig.startWithBag)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        _rsoPlayerDeath.OnChanged += ResetInteraction;
    }

    private void OnDisable()
    {
        _rsoPlayerDeath.OnChanged -= ResetInteraction;
    }

    public override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out _character) && _isAvailable)
        {
            _character.AddToInteractList(this);
        }
    }

    public override void InteractionTrigger()
    {
        _isAvailable = false;
        _character.PickupBackpack();
        _mesh.enabled = false;
    }

    private void ResetInteraction()
    {
        if (!_rsoPlayerDeath.value)
        {
            _isAvailable = true;
            _mesh.enabled = true;
            _character = null;
        }
    }
}
