using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class TorchManager : MonoBehaviour
{
    [FoldoutGroup("Scriptable")][SerializeField] private SSO_Torch m_ssoTorch;
	
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_TorchManager m_rsoTorchManager;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterDeath m_rsoCharacterDeath;

	private List<Torch> m_torches = new List<Torch>();

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
        m_torches.Insert(0, torch);

        while (m_torches.Count > m_ssoTorch.MaxTorchesSoft)
        {
            m_torches[4].Deactivate();
            Remove(m_torches[4]);
        }
    }

    public void Remove(Torch torch)
    {
        m_torches.Remove(torch);
	}

	public void Clear()
	{
		// Assertion
		if (!m_rsoCharacterDeath.value) return;

		for (int i = m_torches.Count - 1; i >= 0; i--)
		{
			m_torches[i].Deactivate();
			Remove(m_torches[i]);
		}
	}
}