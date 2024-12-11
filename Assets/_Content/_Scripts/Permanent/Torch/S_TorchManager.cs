using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class TorchManager : MonoBehaviour
{
    [FoldoutGroup("Scriptable")][SerializeField] private SSO_Torch m_ssoTorch;
	
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_TorchManager m_rsoTorchManager;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;

	[HideInInspector] public List<Torch> Torches = new List<Torch>();

    private void Awake()
    {
        m_rsoTorchManager.value = this;
    }

	private void OnEnable()
	{
		m_rsoCharacterDeath.OnChanged += Clear;
	}

	private void OnDisable()
	{
		m_rsoCharacterDeath.OnChanged -= Clear;
	}

    public void Add(Torch torch)
    {
        Torches.Insert(0, torch);

        while (Torches.Count > m_ssoTorch.MaxTorchesSoft)
        {
            Remove(Torches[4]);
        }
    }

    public void Remove(Torch torch)
	{
		torch.Deactivate();
		Torches.Remove(torch);
	}

	public void Clear()
	{
		// Assertion
		if (!m_rsoCharacterDeath.value) return;

		for (int i = Torches.Count - 1; i >= 0; i--)
		{
			Remove(Torches[i]);
		}
	}
}