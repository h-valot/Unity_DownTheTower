using Sirenix.OdinInspector;
using UnityEngine;

public class ChronoCheckpoint : MonoBehaviour
{
	[Title("Tweakable values")]
	[SerializeField] private string m_checkpointName;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_CheckpointReached m_rseCheckpointReached;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_RestartChrono m_rseRestartChrono;

	private bool m_isActivated;

	private void OnEnable()
	{
		m_rseRestartChrono.action += Restart;
	}

	private void OnDisable()
	{
		m_rseRestartChrono.action -= Restart;
	}

	private void OnTriggerEnter(Collider collider)
	{
		// Assertion
		if (m_isActivated) return;

		if (collider.TryGetComponent<CharacterMotor>(out var character))
		{
			m_isActivated = true;
			m_rseCheckpointReached.Call(m_checkpointName);
		}
	}

	private void Restart()
	{
		m_isActivated = false;
	}
}