using System.Collections.Generic;
using UnityEngine;

public class TorchManager : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] private SSO_Torch m_ssoTorch;
    [SerializeField] private RSO_TorchManager m_rsoTorchManager;

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
