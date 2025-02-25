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

    [Space(10)]
    public UnityEvent OnChanged;

    [HideInInspector] public float Value;

    public virtual void Initialize(float value)
    {
        Value = value;
        UpdateTitle();
        UpdateFlavor();
    }

    public virtual void UpdateOutput()
    {
        OnChanged.Invoke();
    }

    protected void UpdateTitle()
    {
        m_tmpTitle.text = m_title;
    }

    protected void UpdateFlavor()
    {
        m_tmpFlavor.text = m_flavor;
    }
}
