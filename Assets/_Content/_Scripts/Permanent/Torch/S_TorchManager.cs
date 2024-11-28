using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class TorchManager : MonoBehaviour
{
    [FoldoutGroup("Scriptable")][SerializeField] private SSO_Torch m_ssoTorch;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_TorchManager m_rsoTorchManager;

    private List<Torch> m_torches = new List<Torch>();

    private void Awake()
    {
        m_rsoTorchManager.value = this;
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
}