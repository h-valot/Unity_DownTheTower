using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UITabManager : MonoBehaviour
{
    [FoldoutGroup("Internal references")][SerializeField] private List<UITab> m_tabList;

    [FoldoutGroup("Scriptable")][SerializeField] private RSE_SwitchTabLeft m_rseSwitchTabLeft;
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_SwitchTabRight m_rseSwitchTabRight;

    private int m_currentTabIndex;

    private void Awake()
    {
        m_currentTabIndex = 0;
    }

    private void OnEnable()
    {
        m_rseSwitchTabLeft.action += SwitchTabLeft;
        m_rseSwitchTabRight.action += SwitchTabRight;
    }

    private void OnDisable()
    {
        m_rseSwitchTabLeft.action -= SwitchTabLeft;
        m_rseSwitchTabRight.action -= SwitchTabRight;
    }

    private void SwitchTabLeft()
    {
        if (m_tabList.Count <= 1) return;

        UITab nextTab;
        if (m_currentTabIndex == 0) nextTab = m_tabList[^1];
        else nextTab = m_tabList[m_currentTabIndex - 1];
        GoToTab(nextTab);
    }

    private void SwitchTabRight()
    {
        if (m_tabList.Count <= 1) return;

        UITab nextTab;
        if (m_currentTabIndex == m_tabList.Count - 1) nextTab = m_tabList[0];
        else nextTab = m_tabList[m_currentTabIndex + 1];
        GoToTab(nextTab);
    }

    public void OpenCurrentTab()
    {
        m_tabList[m_currentTabIndex].Highlight();
    }

    public void GoToTab(UITab tab)
    {
        m_tabList[m_currentTabIndex].Withdraw();
        m_currentTabIndex = m_tabList.IndexOf(tab);
        tab.Highlight();
    }

    public void CloseCurrentTab()
    {
        m_tabList[m_currentTabIndex].Withdraw();
    }
}
