using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class UILogCollection : UIWindow
{
	[FoldoutGroup("Internal references")][SerializeField] private UILogItem m_pfLogItem;
	[FoldoutGroup("Internal references")][SerializeField] private Transform m_container;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Logs m_ssoLogs;

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
	}

	private void UpdateItems()
	{
		for (int i = 0; i < m_items.Count; i++)
		{
			m_items[i].Toggle(m_ssoLogs.Logs[i].IsDiscovered);
		}
	}
}