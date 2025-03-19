using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class S_MenuManager : MonoBehaviour
{
    #region REFERENCES AND VARIABLES

    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_imgTitle;
    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_menuTitleParent;
    [FoldoutGroup("Internal references")][SerializeField] private List<TextMeshProUGUI> m_menuTitles;
    [FoldoutGroup("Internal references")][SerializeField] private UIBlinkText m_startText;
    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_defaultSelected;
    [FoldoutGroup("Internal references")][SerializeField] private Image m_imgController;
    [FoldoutGroup("Internal references")][SerializeField] private Image m_pnlFade;
    [FoldoutGroup("Internal references")][SerializeField] private UISettings m_pnlSettings;
    [FoldoutGroup("Internal references")][SerializeField] private UICredits m_pnlCredits;

    [FoldoutGroup("External references")][SerializeField] private CameraMotor m_cameraMotor;
    [FoldoutGroup("External references")][SerializeField] private RSE_StartAction m_rseStartAction;
    [FoldoutGroup("External references")][SerializeField] private RSE_ToggleCursor m_rseToggleCursor;
    [FoldoutGroup("External references")][SerializeField] private RSO_CurrentScheme m_rsoCurrentScheme;
    [FoldoutGroup("External references")][SerializeField] private RSO_CurrentControls m_rsoCurrentControls;
    [FoldoutGroup("External references")][SerializeField] protected RSO_GameStarted m_rsoGameStarted;
    [FoldoutGroup("External references")][SerializeField] private RSO_CancelPriority m_rsoCancelPriority;
    [FoldoutGroup("External references")][SerializeField] private RSO_CameraStyle m_rsoCameraStyle;
    [FoldoutGroup("External references")][SerializeField] private SSO_Game m_ssoGame;


    [FoldoutGroup("Menu animation")][SerializeField] private float m_fadeLength;
    [FoldoutGroup("Menu animation")][SerializeField] private float m_menuStartDelay;
    [FoldoutGroup("Menu animation")][SerializeField] private float m_menuTitleInterval;
    [FoldoutGroup("Menu animation")][SerializeField] private float m_menuSlidePosX;
    [FoldoutGroup("Menu animation")][SerializeField] private float m_individualAnimLength;

    private bool m_inMainMenu;
    private float m_originPosX;
    private bool m_consumedUpdate;
    private bool m_preparedStartScreen;

    #endregion

    #region MONOBEHAVIOR

    private void Awake()
    {
        m_menuTitleParent.SetActive(false);
        m_rsoGameStarted.value = false;
        m_inMainMenu = false;
        m_consumedUpdate = false;
        m_preparedStartScreen = false;
    }

    private void Start()
    {
        if (m_ssoGame.EnableMainMenu) ControllerSequence();
        else StartCoroutine(StartImmediately());
    }

    private void OnEnable()
    {
        m_rsoCurrentControls.OnChanged += ChangeControls;
    }

    private void OnDisable()
    {
        m_rsoCurrentControls.OnChanged -= ChangeControls;
    }

    private void Update()
    {
        if (!m_inMainMenu)
        {
            if (m_rsoCurrentControls.value == ControlType.KEYBOARDMOUSE
                && Keyboard.current.anyKey.wasPressedThisFrame 
                && m_preparedStartScreen 
                && !m_consumedUpdate)
            {
                m_consumedUpdate = true;
                ShowMenu();
            }
        }
            
    }

    #endregion

    #region BUTTONS

    private void StartGame()
    {
        if (!m_inMainMenu) return;

        StartCoroutine(StartGameCoroutine());
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

    #endregion

    #region ANIMATIONS

    public void ControllerSequence()
    {
        m_pnlFade.gameObject.SetActive(true);
        m_imgController.gameObject.SetActive(true);
        Sequence introSequence = DOTween.Sequence();
        introSequence.Pause();
        introSequence.Append(m_pnlFade.DOFade(0, m_fadeLength).SetEase(Ease.InCubic));
        introSequence.AppendInterval(2);
        introSequence.Append(m_pnlFade.DOFade(1, m_fadeLength).SetEase(Ease.OutCubic));
        introSequence.Play().OnComplete(PrepareStartScreen);
    }

    public void PrepareStartScreen()
    {
        m_imgController.gameObject.SetActive(false);
        m_pnlFade.DOFade(0, m_fadeLength).SetEase(Ease.InCubic).OnComplete(() => 
        {
            m_pnlFade.gameObject.SetActive(false);
            m_rseStartAction.action += ShowMenu;
            m_preparedStartScreen = true;
        });
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
            m_rseStartAction.action -= ShowMenu;
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

    private IEnumerator StartGameCoroutine()
    {
        m_pnlFade.gameObject.SetActive(true);

        // Fade to Black
        yield return m_pnlFade.DOFade(1, m_fadeLength).SetEase(Ease.OutCubic).WaitForCompletion();

        // Disable all main menu panels
        m_pnlCredits.gameObject.SetActive(false);
        m_pnlSettings.gameObject.SetActive(false);
        m_imgTitle.SetActive(false);

        // Hide Mouse Cursor
        if (m_rsoCurrentControls.value == ControlType.KEYBOARDMOUSE) m_rseToggleCursor.Call(false);

        // Reset Camera to player
        StartCoroutine(m_cameraMotor.SetZeroDampForSeconds(1f));
        m_rsoCameraStyle.value = CameraStyle.BASIC;
        yield return new WaitForSeconds(1f);

        // Fade to Game
        yield return m_pnlFade.DOFade(0, m_fadeLength).SetEase(Ease.InCubic).WaitForCompletion();

        m_pnlFade.gameObject.SetActive(false);
        m_rsoCurrentScheme.value = InputScheme.GAME;
        m_rsoCancelPriority.value = CancelState.IN_GAME;
        m_rsoGameStarted.value = true;

    }

    #endregion

    #region CONTROLS

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

    #endregion

    #region DEBUG

    private IEnumerator StartImmediately()
    {
        // Disable all main menu panels
        m_imgController.gameObject.SetActive(false);
        m_pnlCredits.gameObject.SetActive(false);
        m_pnlSettings.gameObject.SetActive(false);
        m_imgTitle.SetActive(false);

        // Hide Mouse Cursor
        if (m_rsoCurrentControls.value == ControlType.KEYBOARDMOUSE) m_rseToggleCursor.Call(false);

        // Reset Camera to player
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(m_cameraMotor.SetZeroDampForSeconds(1f));
        m_rsoCameraStyle.value = CameraStyle.BASIC;

        m_pnlFade.gameObject.SetActive(false);
        m_rsoCurrentScheme.value = InputScheme.GAME;
        m_rsoCancelPriority.value = CancelState.IN_GAME;
        m_rsoGameStarted.value = true;
    }

    #endregion

}
