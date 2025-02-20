using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UILogCollection : UIWindow
{
	[FoldoutGroup("Internal references")][SerializeField] private UILogItem m_pfLogItem;
	[FoldoutGroup("Internal references")][SerializeField] private Transform m_container;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Logs m_ssoLogs;
    [FoldoutGroup("Scriptable")][SerializeField] private RSO_Pause m_rsoPause;

	private List<UILogItem> m_items = new List<UILogItem>();

	public override void Start()
	{
		CreateItems();
		base.Start();
	}

    public override void Show()
	{
		UpdateItems();
		base.Show();
	}

	private void CreateItems()
	{
		for (int i = 0; i < m_ssoLogs.Logs.Count; i++)
		{
			var newItem = Instantiate(m_pfLogItem, m_container);
			newItem.Initialize(m_ssoLogs.Logs[i]);
			m_items.Add(newItem);
        }
		UpdateNavigation();

    }

	private void UpdateItems()
	{
		for (int i = 0; i < m_items.Count; i++)
		{
			m_items[i].Toggle(m_ssoLogs.Logs[i].IsDiscovered);
		}
		UpdateNavigation();

    }

	private void UpdateNavigation()
    {
        if (m_items.Count == 0) return;

		// Set first item of list to be automatically selected
		m_defaultSelect = m_items[0].TmpButton.gameObject;

        Navigation nav = new Navigation();
		for (int i = 0; i < m_items.Count; i++)
		{
            nav = m_items[i].TmpButton.navigation;

			if (i == 0) 
			{
                nav.selectOnUp = m_items[m_items.Count - 1].TmpButton;
				// Applyable only if one item in list
				if (i + 1 == m_items.Count) nav.selectOnDown = m_items[0].TmpButton;
                else nav.selectOnDown = m_items[i + 1].TmpButton;
            }
			else if (i == m_items.Count - 1)
			{
                nav.selectOnUp = m_items[i - 1].TmpButton;
                nav.selectOnDown = m_items[0].TmpButton;
            }
			else
            {
                nav.selectOnUp = m_items[i - 1].TmpButton;
                nav.selectOnDown = m_items[i + 1].TmpButton;
            }
            m_items[i].TmpButton.navigation = nav;
        }
    }
}