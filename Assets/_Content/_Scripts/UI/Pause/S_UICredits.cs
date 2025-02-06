using Sirenix.OdinInspector;
using UnityEngine;

public class UICredits : UIWindow
{
    [FoldoutGroup("Scriptable")][SerializeField] private RSE_Cancel m_rseCancel;

    private void OnEnable()
    {
        m_rseCancel.action += OnBack;
    }

    private void OnDisable()
    {
        m_rseCancel.action -= OnBack;
    }

    private void OnBack(bool isPressed)
    {
        if (isPressed) Return();
    }

}