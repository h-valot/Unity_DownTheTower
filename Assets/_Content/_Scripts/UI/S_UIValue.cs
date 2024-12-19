using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIValue : MonoBehaviour
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

	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpTitle;
	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpFlavor;
	[FoldoutGroup("Internal references")][SerializeField] private TMP_InputField m_inpInput;

	[Space(10)]
	public UnityEvent OnChanged;

	[HideInInspector] public float Value;

	public void Initialize(float value)
	{
		Value = value;
		m_inpInput.text = $"{value}";
		UpdateTitle();
		UpdateFlavor();
	}

	public void UpdateOutput()
	{
		float.TryParse(m_inpInput.text, out Value);
		OnChanged.Invoke();
	}

	private void UpdateTitle()
	{
		m_tmpTitle.text = m_title;
	}

	private void UpdateFlavor()
	{
		m_tmpFlavor.text = m_flavor;
	}
}