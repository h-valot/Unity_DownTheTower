using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnScaleButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	[Header("Tweakable values")]
	[SerializeField] private float _scaleDuration = 0.1f;
	[SerializeField] private float _scaleDownMultiplier = 0.8f;
	[SerializeField] private float _scaleUpMultiplier = 1.2f;

	[Header("Internal references")]
	[Tooltip("It will be scaled down on pointer down and reset to normal on pointer up")]
	[SerializeField] private GameObject _graphicsParent;
	[Tooltip("It will be darken on pointer down and hide on pointer up")]
	[SerializeField] private Image _imgBlack = null;

	[Space(10)]
	public UnityEvent OnClick;

	public void OnPointerDown(PointerEventData data)
	{
		_graphicsParent.transform.DOScale(_scaleDownMultiplier, _scaleDuration).SetEase(Ease.OutBack);
		if (_imgBlack != null) _imgBlack.DOFade(0.75f, 0);
	}

	public void OnPointerUp(PointerEventData data)
	{
		_graphicsParent.transform.DOScale(Vector3.one, _scaleDuration).SetEase(Ease.OutBack);
		if (_imgBlack != null) _imgBlack.DOFade(0, 0);
		OnClick.Invoke();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_graphicsParent.transform.DOScale(_scaleUpMultiplier, _scaleDuration).SetEase(Ease.OutBack);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_graphicsParent.transform.DOScale(Vector3.one, _scaleDuration).SetEase(Ease.OutBack);
	}
}