using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class UIBlinkText : MonoBehaviour
{

    [FoldoutGroup("Internal references")]
    [InfoBox("Target text to blink", InfoMessageType.None)]
    [SerializeField] private TextMeshProUGUI m_graphicsParent;
    
    void Start()
    {
        m_graphicsParent.DOFade(0, 1f).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
    }
}
