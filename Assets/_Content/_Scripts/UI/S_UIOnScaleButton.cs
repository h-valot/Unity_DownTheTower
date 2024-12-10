using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIOnScaleButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	[Title("Tweakable values")]
	[SerializeField] private float m_scaleDuration = 0.05f;
	[SerializeField] private float m_scaleDownMultiplier = 0.9f;
	[SerializeField] private float m_scaleUpMultiplier = 1.1f;

	[Title("Internal references")]
	[InfoBox("It will be scaled down on pointer down and reset to normal on pointer up", InfoMessageType.None)]
	[SerializeField] private GameObject m_graphicsParent;
	[InfoBox("It will be darken on pointer down and hide on pointer up", InfoMessageType.None)]
	[SerializeField] private Image m_imgBlack = null;

	[Space(10)]
	public UnityEvent OnClick;

	public void OnPointerDown(PointerEventData data)
	{
		m_graphicsParent.transform.DOScale(m_scaleDownMultiplier, m_scaleDuration).SetEase(Ease.OutBack).SetUpdate(true);
		if (m_imgBlack != null) m_imgBlack.DOFade(0.75f, 0).SetUpdate(true);
	}

	public void OnPointerUp(PointerEventData data)
	{
		m_graphicsParent.transform.DOScale(Vector3.one, m_scaleDuration).SetEase(Ease.OutBack).SetUpdate(true);
		if (m_imgBlack != null) m_imgBlack.DOFade(0, 0).SetUpdate(true);
		OnClick.Invoke();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		m_graphicsParent.transform.DOScale(m_scaleUpMultiplier, m_scaleDuration).SetEase(Ease.OutBack).SetUpdate(true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		m_graphicsParent.transform.DOScale(Vector3.one, m_scaleDuration).SetEase(Ease.OutBack).SetUpdate(true);
	}
}