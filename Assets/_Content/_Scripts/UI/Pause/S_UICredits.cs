using Sirenix.OdinInspector;
using UnityEngine;

public class UICredits : UIWindow
{
    [ShowIf("m_isMainMenu")][FoldoutGroup("Tweakable values")][SerializeField] protected bool m_isMainMenu;

    [FoldoutGroup("Internal references")][SerializeField] private S_MenuManager m_mainMenu;

    public override void Return(bool isPressed)
    {
        base.Return(isPressed);
        if (m_isMainMenu) m_mainMenu.ShowMenu();
    }
}