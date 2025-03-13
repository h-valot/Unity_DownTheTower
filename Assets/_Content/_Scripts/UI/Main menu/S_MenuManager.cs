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
    [FoldoutGroup("Internal references")][SerializeField] private UICredits m_pnlCredits;

    [FoldoutGroup("External references")][SerializeField] private RSE_StartAction m_rseStartAction;
    [FoldoutGroup("External references")][SerializeField] private RSE_ToggleCursor m_rseToggleCursor;
    [FoldoutGroup("External references")][SerializeField] private RSO_CurrentScheme m_rsoCurrentScheme;
    [FoldoutGroup("External references")][SerializeField] private RSO_CurrentControls m_rsoCurrentControls;
    [FoldoutGroup("External references")][SerializeField] protected RSO_GameStarted m_rsoGameStarted;
    [FoldoutGroup("External references")][SerializeField] private RSO_CancelPriority m_rsoCancelPriority;

    [FoldoutGroup("Menu animation")][SerializeField] private float m_menuStartDelay;
    [FoldoutGroup("Menu animation")][SerializeField] private float m_menuTitleInterval;
    [FoldoutGroup("Menu animation")][SerializeField] private float m_menuSlidePosX;
    [FoldoutGroup("Menu animation")][SerializeField] private float m_individualAnimLength;

    private bool m_inMainMenu;
    private float m_originPosX;

    // Start is called before the first frame update

    private void Awake()
    {
        m_menuTitleParent.SetActive(false);
        m_rsoGameStarted.value = false;
        m_inMainMenu = false;
    }

    private void OnEnable()
    {
        m_rseStartAction.action += ShowMenu;
        m_rsoCurrentControls.OnChanged += ChangeControls;
    }

    private void OnDisable()
    {
        m_rseStartAction.action -= ShowMenu;
        m_rsoCurrentControls.OnChanged -= ChangeControls;
    }



    public void StartGame()
    {
        if (!m_inMainMenu) return;

        m_imgTitle.SetActive(false);
        m_rsoCurrentScheme.value = InputScheme.GAME;
        m_rsoCancelPriority.value = CancelState.IN_GAME;
        m_rsoGameStarted.value = true;

        // Disable all panels
        m_pnlCredits.gameObject.SetActive(false);
        m_pnlSettings.gameObject.SetActive(false);
    }

    public void OpenCredits()
    {
        if (!m_inMainMenu) return;

        m_imgTitle.SetActive(false);
        m_rsoCancelPriority.value = CancelState.UI_CREDITS;
        m_pnlCredits.Show();
    }

    public void OpenSettings()
    {
        if (!m_inMainMenu) return;

        m_imgTitle.SetActive(false);
        m_rsoCancelPriority.value = CancelState.UI_SETTINGS;
        m_pnlSettings.Show();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PrepareMenu()
    {
        m_originPosX = m_menuTitles[0].transform.localPosition.x;

        foreach (TextMeshProUGUI menu in m_menuTitles)
        {
            // Setting up placement and alpha
            menu.transform.DOLocalMoveX(m_originPosX + m_menuSlidePosX, 0);
            menu.DOFade(0, 0);
        }

    }

    public void ShowMenu()
    {
        if (m_rsoGameStarted.value) return;

        if (!m_inMainMenu)
        {
            PrepareMenu();

            m_inMainMenu = true;
            m_menuTitleParent.SetActive(true);
            StartCoroutine(MenuAnimation());
        }
        else
        {
            ResetTitlePositions();
            m_imgTitle.SetActive(true);
            ChangeControls();
        }

        m_rsoCancelPriority.value = CancelState.NONE;
    }

    public void ResetTitlePositions()
    {
        foreach (TextMeshProUGUI menu in m_menuTitles)
        {
            menu.transform.DOLocalMoveX(m_originPosX, 0);
        }
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
