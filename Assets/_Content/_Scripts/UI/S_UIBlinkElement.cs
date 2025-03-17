using DG.Tweening;
using DG.Tweening.Core;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class UIBlinkText : MonoBehaviour
{

    [FoldoutGroup("Internal references")]
    [InfoBox("Target text to blink", InfoMessageType.None)]
    [SerializeField] private TextMeshProUGUI m_graphicsParent;

    // PRIVATE
    private Tweener blinkEffect;


    private void Start()
    {
        blinkEffect = m_graphicsParent.DOFade(0, 1f).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
    }

    public void StopEffect()
    {
        blinkEffect.Kill();
        m_graphicsParent.alpha = 1f;
        m_graphicsParent.DOFade(0, .5f).SetEase(Ease.InCubic);
    }
}
