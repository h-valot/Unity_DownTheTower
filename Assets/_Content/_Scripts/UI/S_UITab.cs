using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITab : MonoBehaviour, IPointerDownHandler
{
	[FoldoutGroup("Tweakable values")][SerializeField] private Color m_colorHighlighted;
	[FoldoutGroup("Tweakable values")][SerializeField] private Color m_colorWithdrawn;

	[Required("When pressed, all ui tab linked panel will be withdrawn. Then, this one will be highlighted.")]
	[FoldoutGroup("Internal references")][SerializeField] private GameObject m_linkedPanel;
	[FoldoutGroup("Internal references")][SerializeField] private Image m_imgBackground;

	[FoldoutGroup("Scriptables")][SerializeField] private RSE_WithdrawTabs m_rseWithdrawTabs;

	private void OnEnable()
	{
		m_rseWithdrawTabs.action += Withdraw;
	}

	private void OnDisable()
	{
		m_rseWithdrawTabs.action -= Withdraw;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		Highlight();
	}

	public void Highlight()
	{
		m_rseWithdrawTabs.Call();
		m_imgBackground.color = m_colorHighlighted;
		m_linkedPanel.SetActive(true);
	}

	public void Withdraw()
	{
		m_imgBackground.color = m_colorWithdrawn;
		m_linkedPanel.SetActive(false);
	}
}