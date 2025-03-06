using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class S_MenuManager : MonoBehaviour
{

    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_imgTitle;
    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_menuTitleParent;
    [FoldoutGroup("Internal references")][SerializeField] private List<TextMeshProUGUI> m_menuTitles;
    [FoldoutGroup("Internal references")][SerializeField] private UIBlinkText m_startText;
    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_defaultSelected;
    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_imgController;
    [FoldoutGroup("Internal references")][SerializeField] private UISettings m_pnlSettings;
    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_pnlCredits;

    [FoldoutGroup("External references")][SerializeField] private RSE_Start m_rseStart;
    [FoldoutGroup("External references")][SerializeField] private RSE_ToggleCursor m_rseToggleCursor;
    [FoldoutGroup("External references")][SerializeField] private RSO_CurrentScheme m_rsoCurrentScheme;
    [FoldoutGroup("External references")][SerializeField] private RSO_CurrentControls m_rsoCurrentControls;

    [FoldoutGroup("Menu animation")][SerializeField] private float m_menuStartDelay;
    [FoldoutGroup("Menu animation")][SerializeField] private float m_menuTitleInterval;
    [FoldoutGroup("Menu animation")][SerializeField] private float m_menuSlidePosX;
    [FoldoutGroup("Menu animation")][SerializeField] private float m_individualAnimLength;

    private bool m_hasStarted;
    private float m_originPosX;

    // Start is called before the first frame update

    private void Awake()
    {
        m_hasStarted = false;
        PrepareMenu();
    }

    private void OnEnable()
    {
        m_rseStart.action += ShowMenu;
        m_rsoCurrentControls.OnChanged += ChangeControls;
    }

    private void OnDisable()
    {
        m_rseStart.action -= ShowMenu;
        m_rsoCurrentControls.OnChanged -= ChangeControls;
    }

    public void OpenCredits()
    {
        if (!m_hasStarted) return;

        m_imgTitle.SetActive(false);
        m_pnlCredits.SetActive(true);
    }

    public void OpenSettings()
    {
        if (!m_hasStarted) return;

        m_imgTitle.SetActive(false);
        m_pnlSettings.Show();
    }

    public void PrepareMenu()
    {
        m_originPosX = m_menuTitles[0].transform.localPosition.x;

        foreach (TextMeshProUGUI menu in m_menuTitles)
        {
            // Setting up placement and alpha
            menu.transform.localPosition = new Vector3(menu.transform.localPosition.x + m_menuSlidePosX, menu.transform.localPosition.y, menu.transform.localPosition.z);
            menu.DOFade(0, 0);
        }

        m_menuTitleParent.SetActive(false);
    }

    public void ShowMenu()
    {
        if (m_hasStarted) return;

        m_hasStarted = true;
        StartCoroutine(MenuAnimation());

    }

    private IEnumerator MenuAnimation()
    {
        m_startText.StopEffect();
        yield return new WaitForSeconds(m_menuStartDelay);
        m_menuTitleParent.SetActive(true);

        foreach (TextMeshProUGUI menu in m_menuTitles)
        {
            menu.DOFade(1, m_individualAnimLength).SetEase(Ease.InQuint);
            menu.transform.DOLocalMoveX(m_originPosX, m_individualAnimLength).SetEase(Ease.OutCirc);
            yield return new WaitForSeconds(m_menuTitleInterval);
        }

        ChangeControls();
    }

    private void ChangeControls()
    {
        if (m_rsoCurrentControls.value == ControlType.GAMEPAD)
        {
            m_rseToggleCursor.Call(false);
            EventSystem.current.SetSelectedGameObject(m_defaultSelected);
        }
        else if (m_rsoCurrentControls.value == ControlType.KEYBOARDMOUSE)
        {
            EventSystem.current.SetSelectedGameObject(null);
            m_rseToggleCursor.Call(true);
        }

    }

}
