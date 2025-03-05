using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class S_MenuManager : MonoBehaviour
{

    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_imgTitle;
    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_menuTitleParent;
    [FoldoutGroup("Internal references")][SerializeField] private List<TextMeshProUGUI> m_menuTitles;
    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_imgController;
    [FoldoutGroup("Internal references")][SerializeField] private UISettings m_pnlSettings;
    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_pnlCredits;

    [FoldoutGroup("External references")][SerializeField] private RSE_Start m_rseStart;
    [FoldoutGroup("External references")][SerializeField] private RSO_CurrentScheme m_rsoCurrentScheme;

    [FoldoutGroup("Menu animation")][SerializeField] private float m_sequenceStartDelay;
    [FoldoutGroup("Menu animation")][SerializeField] private float m_menuTitleInterval;
    [FoldoutGroup("Menu animation")][SerializeField] private float m_menuSlidePosX;
    [FoldoutGroup("Menu animation")][SerializeField] private float m_individualAnimLength;

    private bool m_hasStarted;
    private float m_originPosX;

    // Start is called before the first frame update
    void Start()
    {
        m_hasStarted = false;
        PrepareMenu();
    }

    private void OnEnable()
    {
        m_rseStart.action += ShowMenu;
    }

    private void OnDisable()
    {
        m_rseStart.action -= ShowMenu;
    }

    public void OpenCredits()
    {
        m_imgTitle.SetActive(false);
        m_pnlCredits.SetActive(true);
    }

    public void OpenSettings()
    {
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
        m_menuTitleParent.SetActive(true);

        float delay = 0f;
        Sequence menuEffect = DOTween.Sequence();

        foreach (TextMeshProUGUI menu in m_menuTitles)
        {
            menuEffect.Insert(delay, menu.DOFade(255, m_individualAnimLength)).SetEase(Ease.OutCirc);
            menuEffect.Insert(delay, menu.transform.DOLocalMoveX(m_originPosX, m_individualAnimLength)).SetEase(Ease.OutCirc);
            delay += m_menuTitleInterval;
        }

        menuEffect.Play();
        print("play !!");
    }

}
