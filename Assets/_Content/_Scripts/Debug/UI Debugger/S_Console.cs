using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI.Console
{
	public class Console : MonoBehaviour
	{
		[Header("REFERENCES")]
		public RSE_DebugLog m_rseDebugLog;
		public Transform m_graphicsParent;
		public TextMeshProUGUI m_tmpMessage;

		[Header("TWEAKING")]
		public float Duration;
		public float Offset;

		private Vector3 m_basePosition;
		private Coroutine m_coroutine;

		private void OnEnable()
		{
			m_basePosition = m_graphicsParent.localPosition;
			m_rseDebugLog.action += PrintMessage;
		}

		private void OnDisable()
		{
			m_rseDebugLog.action -= PrintMessage;
		}

		private void PrintMessage(string message)
		{
			if (m_coroutine != null) StopCoroutine(m_coroutine);
			m_coroutine = StartCoroutine(AnimateMessage(message));
		}

		private IEnumerator AnimateMessage(string message)
		{
			m_graphicsParent.gameObject.SetActive(true);
			m_tmpMessage.text = message;
			m_graphicsParent.DOLocalMoveY(m_basePosition.y, 0);
			m_graphicsParent.DOLocalMoveY(m_basePosition.y + Offset, Duration);
			yield return new WaitForSeconds(Duration);
			m_graphicsParent.gameObject.SetActive(false);
		}
	}
}