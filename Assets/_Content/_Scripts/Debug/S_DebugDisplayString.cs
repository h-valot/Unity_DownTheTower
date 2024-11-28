using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class DebugDisplayString : MonoBehaviour
{
	[Title("Tweakable values")]
	[SerializeField] private int m_rowToDisplay;

    [FoldoutGroup("Scriptable")][SerializeField] private RSO_MovementDatas m_rsoData;
    
    private TMP_Text m_tmpText;

    private void Awake()
    {
        m_tmpText = GetComponentInChildren<TMP_Text>();
    }

	private void OnEnable()
	{
		m_rsoData.OnChanged += UpdateDisplay;
	}

	private void OnDisable()
	{
		m_rsoData.OnChanged -= UpdateDisplay;
	}

    private void UpdateDisplay()
    {
        if (m_rowToDisplay < m_rsoData.value.dataToString.Count)
        {
            m_tmpText.text = m_rsoData.value.dataToString[m_rowToDisplay];
        }
    }
}