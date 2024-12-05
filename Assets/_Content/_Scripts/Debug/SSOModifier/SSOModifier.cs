using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class SSOModifier : MonoBehaviour
{
	[Title("Internal references")]
    [SerializeField] private GameObject m_graphicsParent;
	[SerializeField] private UIToggleable m_toggleableActivateBreakAnim;
	[SerializeField] private UIToggleable m_toggleableLightIntensity;
	[SerializeField] private UIToggleable m_toggleableDesactivatingTime;

	[FoldoutGroup("Static variables")][SerializeField] private Color m_colorTabSelected;
	[FoldoutGroup("Static variables")][SerializeField] private Color m_colorTabUnselected;
	[FoldoutGroup("Static variables")][SerializeField] private List<TabContainer> m_tabContainers;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Game m_ssoGame;
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Debug m_ssoDebug;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_ToggleCursor m_rseToggleCursor;

    // ----- PRIVATE VARIABLES -----
    private bool m_isPressed;
    private bool m_isEnabled;

    private void Start()
    {
        Hide();

		// Update toggleables
		m_toggleableActivateBreakAnim.Initialize(
			m_ssoDebug.OverrideActivateBreakAnim, 
			nameof(m_ssoDebug.OverrideActivateBreakAnim).Substring(8), 
			m_ssoDebug.FlavorActivateBreakAnim
		);

		m_toggleableLightIntensity.Initialize(
			m_ssoDebug.OverrideLightIntensity,
			nameof(m_ssoDebug.OverrideLightIntensity).Substring(8),
			m_ssoDebug.FlavorLightIntensity
		);

		m_toggleableDesactivatingTime.Initialize(
			m_ssoDebug.OverrideDesactivatingTime,
			nameof(m_ssoDebug.OverrideDesactivatingTime).Substring(8),
			m_ssoDebug.FlavorDesactivatingTime
		);
	}

    private void Update()
    {
        HandleShortcut();
    }

    private void HandleShortcut()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            if (!m_isPressed)
			{
				if (m_isEnabled)
				{
					Hide();
					Time.timeScale = 1f;
				}
				else
				{
					Show();
					Time.timeScale = 0.001f;
				}
			}
            m_isPressed = true;
        }

        if (Input.GetKeyUp(KeyCode.F3))
        {
            m_isPressed = false;
        }
    }

	public void Hide()
	{
		m_rseToggleCursor.Call(false);
		m_graphicsParent.SetActive(false);
        m_isEnabled = false;
    }

    private void Show()
	{
		// Assertion
		if (m_ssoGame.BuildType == BuildType.RELEASE) return;

		m_rseToggleCursor.Call(true);
		m_graphicsParent.SetActive(true);
        m_isEnabled = true;

		PressTab(m_tabContainers[0].tab);
    }

	public void PressTab(Image tabPressed)
	{
		foreach (var tabContainer in m_tabContainers)
		{
			tabContainer.tab.color = m_colorTabUnselected;
			tabContainer.container.SetActive(false);
		}

		tabPressed.color = m_colorTabSelected;
		m_tabContainers.Where(tc => tc.tab.name == tabPressed.name).ToList()[0].container.SetActive(true);
	}

    public void ToggleActivateBreakAnim() => m_ssoDebug.OverrideActivateBreakAnim = m_toggleableActivateBreakAnim.Value;
	public void UpdateLightIntensity() => m_ssoDebug.OverrideLightIntensity = m_toggleableLightIntensity.Value;
	public void UpdateDesactivatingTime() => m_ssoDebug.OverrideDesactivatingTime = m_toggleableDesactivatingTime.Value;
}