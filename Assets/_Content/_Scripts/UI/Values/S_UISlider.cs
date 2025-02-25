using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class UISlider : UIValue
{
    [FoldoutGroup("Tweakable values")]
    [OnValueChanged("UpdateFlavor")]
    [SerializeField] private float m_nbOfIncrements = 8;

    [FoldoutGroup("Internal references")][SerializeField] private Slider m_slider;

    private float m_minValue;
    private float m_maxValue;


    public override void Initialize(float value)
    {
        // Set editor min max values
        if (value >= 0)
        {
            m_minValue = 0;
            m_maxValue = value;
        }
        else
        {
            m_minValue = value;
            m_maxValue = 0;
        }

        // Set slider values
        UpdateIncrements();
        m_slider.value = Mathf.Round(Matha.Remap(m_minValue, m_maxValue, m_slider.minValue, m_slider.maxValue, value));


        // Set editor value and UI text
        base.Initialize(value);
    }

    public void Initialize( float minValue, float maxValue, float value)
    {
        // Set editor min max values
        m_minValue = minValue;
        m_maxValue = maxValue;

        // Set slider min max values
        UpdateIncrements();
        m_slider.value = Mathf.Round(Matha.Remap(m_minValue, m_maxValue, m_slider.minValue, m_slider.maxValue, value));

        base.Initialize(value);
    }

    public override void UpdateOutput()
    {
        Value = Mathf.Round(Matha.Remap(m_slider.minValue, m_slider.maxValue, m_minValue, m_maxValue, m_slider.value));
        base.UpdateOutput();
    }

    private void UpdateIncrements()
    {
        m_slider.minValue = 0;
        m_slider.maxValue = Mathf.Round(m_nbOfIncrements);
    }
}
