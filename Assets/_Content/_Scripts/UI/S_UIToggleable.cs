using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIToggleable : MonoBehaviour
{
	[Title("Title")]
	[HideLabel]
	[MultiLineProperty(2)]
	[OnValueChanged("UpdateTitle")]
	[SerializeField] private string m_title;

	[Title("Flavor")]
	[HideLabel]
	[MultiLineProperty(5)]
	[OnValueChanged("UpdateFlavor")]
	[SerializeField] private string m_flavor;

	[FoldoutGroup("Internal references")][SerializeField] private Sprite m_spToggleEnabled;
	[FoldoutGroup("Internal references")][SerializeField] private Sprite m_spToggleDisabled;
	[FoldoutGroup("Internal references")][SerializeField] private Image m_imgToggle;
	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpTitle;
	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpFlavor;

	[Space(10)]
	public UnityEvent<bool> OnToggled;

	[HideInInspector] public bool Value;

	public void Initialize(bool value)
	{
		Value = value;
		UpdateGraphics();
		UpdateTitle();
		UpdateFlavor();
	}

	public void Initialize(bool value, string title, string flavor)
	{
		Value = value;
		m_title = title;
		m_flavor = flavor;
		UpdateGraphics();
		UpdateTitle();
		UpdateFlavor();
	}

	public void Toggle()
	{
		Value = !Value;
		OnToggled.Invoke(Value);
		UpdateGraphics();
	}

	private void UpdateTitle()
	{
		m_tmpTitle.text = m_title;
	}

	public void SetFlavor(string flavor)
	{
		m_flavor = flavor;
		UpdateFlavor();
	}

	private void UpdateFlavor()
	{
		m_tmpFlavor.text = m_flavor;
	}

	private void UpdateGraphics()
	{
		m_imgToggle.sprite = Value ? m_spToggleEnabled : m_spToggleDisabled;
	}
}