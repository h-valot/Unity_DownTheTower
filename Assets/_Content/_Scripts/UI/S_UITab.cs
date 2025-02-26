using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITab : MonoBehaviour
{
	[FoldoutGroup("Tweakable values")][SerializeField] private Color m_colorHighlighted;
	[FoldoutGroup("Tweakable values")][SerializeField] private Color m_colorWithdrawn;

    [FoldoutGroup("Internal references")][SerializeField] private UITabManager m_parentManager;
    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_linkedPanel;
    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_defaultSelected;
    [FoldoutGroup("Internal references")][SerializeField] private Image m_imgBackground;

	[FoldoutGroup("Scriptables")][SerializeField] private RSO_CurrentControls m_rsoCurrentControls;

	public void Highlight()
	{
		m_imgBackground.color = m_colorHighlighted;
		m_linkedPanel.SetActive(true);
		if(m_rsoCurrentControls.value == ControlScheme.GAMEPAD) EventSystem.current.SetSelectedGameObject(m_defaultSelected);

    }

	public void Withdraw()
	{
		m_imgBackground.color = m_colorWithdrawn;
		m_linkedPanel.SetActive(false);
    }

	public void SwitchTabByClick()
	{
		m_parentManager.GoToTab(this);
    }
}