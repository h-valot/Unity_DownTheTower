using TMPro;
using UnityEngine;

public class HPPC_DisplayString : MonoBehaviour
{
    [Header("External References")]
    [SerializeField] private RSO_HPPC_MovementDatas _rso;
    [Header("Config")]
    [SerializeField] private int rowToDisplay;
    
    private TMP_Text _text;

    private void Awake()
    {
        _rso.OnChanged += UpdateDisplay;
        _text = GetComponentInChildren<TMP_Text>();
    }

    void UpdateDisplay()
    {
        if(rowToDisplay < _rso.value.dataToString.Count)
        {
            _text.text = _rso.value.dataToString[rowToDisplay];
        }
    }
}
