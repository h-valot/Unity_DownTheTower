using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIInput : UIValue
{
	[FoldoutGroup("Internal references")][SerializeField] private TMP_InputField m_inpInput;

    public override void Initialize(float value)
	{
		m_inpInput.text = $"{value}";
		base.Initialize(value);
	}

	public override void UpdateOutput()
	{
		float.TryParse(m_inpInput.text, out Value);
		base.UpdateOutput();
	}
}