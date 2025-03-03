using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class S_MenuManager : MonoBehaviour
{

    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_imgTitle;
    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_imgController;
    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_pnlSettings;
    [FoldoutGroup("Internal references")][SerializeField] private GameObject m_pnlCredits;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenCredits()
    {
        m_imgTitle.SetActive(false);
        m_pnlCredits.SetActive(true);
    }

}
