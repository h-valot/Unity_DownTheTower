using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class UITutoPanel : MonoBehaviour
{
	[FoldoutGroup("Tweakable values")][SerializeField] protected int m_index;

	[FoldoutGroup("Internal references")][SerializeField] protected GameObject m_graphicsParent;
	[FoldoutGroup("Internal references")][SerializeField] protected TextMeshProUGUI m_text;

	[FoldoutGroup("Scriptables")][SerializeField] protected RSO_CurrentTutoIndex m_rsoCurrentTutoIndex;
	[FoldoutGroup("Scriptables")][SerializeField] protected SSO_Interface m_ssoInterface;

	protected bool m_isCompleted;
	protected float m_baseLocalPosX;

	public bool IsActive => m_graphicsParent.activeInHierarchy;

	protected virtual void OnEnable()
	{
		m_rsoCurrentTutoIndex.OnChanged += OnTutoChanged;
	}

	protected virtual void OnDisable()
	{
		m_rsoCurrentTutoIndex.OnChanged -= OnTutoChanged;
	}

	protected virtual void Start()
	{
		m_baseLocalPosX = m_graphicsParent.transform.localPosition.x;
		m_isCompleted = false;
		Hide();
	}

	private void OnTutoChanged()
	{
		if (m_rsoCurrentTutoIndex.value != m_index)
		{
			Hide();
		}
	}

	protected virtual void Complete()
	{
		m_isCompleted = true;
		StartCoroutine(AnimateComplete());
	}

	private IEnumerator AnimateComplete()
	{
		m_text.DOColor(m_ssoInterface.HighlightColor, 0f);
		yield return new WaitForSeconds(m_ssoInterface.TutoDisappearDuration);
		
		m_graphicsParent.transform.DOLocalMoveX(m_baseLocalPosX - m_ssoInterface.TutoLocalXOffset, m_ssoInterface.TutoDisappearDuration).SetEase(Ease.OutQuint);
		m_text.DOFade(0f, m_ssoInterface.TutoDisappearDuration).SetEase(Ease.OutQuint);
		yield return new WaitForSeconds(m_ssoInterface.TutoDisappearDuration);

		m_graphicsParent.SetActive(false);
	}

	protected virtual void Hide()
	{
		m_graphicsParent.SetActive(false);
	}

	protected virtual void Show()
	{
		m_text.DOFade(0f, 0f);
		m_graphicsParent.transform.localPosition = new Vector3(
			m_baseLocalPosX - m_ssoInterface.TutoLocalXOffset,
			m_graphicsParent.transform.localPosition.y,
			m_graphicsParent.transform.localPosition.z
		);

		m_graphicsParent.SetActive(true);
		m_rsoCurrentTutoIndex.value = m_index;

		m_graphicsParent.transform.DOLocalMoveX(m_baseLocalPosX, m_ssoInterface.TutoAppearDuration).SetEase(Ease.OutQuint);
		m_text.DOFade(1f, m_ssoInterface.TutoAppearDuration).SetEase(Ease.OutQuint);
	}
}