using DG.Tweening;
using UnityEngine;

public class Backpack : Interactible
{
    [Header("Internal Variables")]
    [SerializeField] private MeshRenderer _mesh;
    [SerializeField] private SphereCollider _sphereCollider;

    [Header("External Variables")]
    [SerializeField] private RSO_PlayerDeath _rsoPlayerDeath;

    [Header("Scriptable references")]
    [SerializeField] private CharacterConfig _characterConfig;

    // PRIVATE VARIABLES
    private CharacterMotor _character;
    private bool _isPickedUp = false;
    private Vector3 _startPosition;
    private Vector3 _startRotation;
    private Vector3 _startScale;

    private void Awake()
    {
        _startPosition = transform.position;
        _startRotation = transform.eulerAngles;
        _startScale = transform.localScale;
        _mesh.material.SetFloat("_craftingPercent", 1f);
    }

    private void OnEnable()
    {
        _rsoPlayerDeath.OnChanged += ResetBackpack;
    }

    private void OnDisable()
    {
        _rsoPlayerDeath.OnChanged -= ResetBackpack;
    }

    public override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterMotor>(out _character) && !_isPickedUp)
        {
            _character.AddToInteractList(this);
        }
    }

    public override void InteractionTrigger()
    {
        _isPickedUp = true;
        _character.PickupBackpack(true, this);
        _sphereCollider.enabled = false;
    }

    private void ResetBackpack()
    {
        if (!_rsoPlayerDeath.value)
        {
            transform.position = _startPosition;
            transform.rotation = Quaternion.Euler(_startRotation);
            transform.localScale = _startScale;
            _isPickedUp = false;
            _sphereCollider.enabled = true;
        }
    }

    public void ForceSetupBackpack(CharacterMotor _tmpCharacter)
    {
        _character = _tmpCharacter;
        InteractionTrigger();
    }

    public void StartCrafting(float _craftTime)
    {
        _mesh.material.DOFloat(0f, "_craftingPercent", _craftTime).SetEase(Ease.Linear).SetId(gameObject.GetInstanceID() +"craftingPercent");
    }

    public void EndCrafting()
    {
        DOTween.Kill(gameObject.GetInstanceID() + "craftingPercent");
        _mesh.material.SetFloat("_craftingPercent", 1f);
    }
}
