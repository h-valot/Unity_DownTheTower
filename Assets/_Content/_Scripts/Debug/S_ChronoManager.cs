using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class ChronoManager : UIWindow
{
	[FoldoutGroup("Internal references")][SerializeField] private TextMeshProUGUI m_tmpChrono;

	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Game m_ssoGame;

	[FoldoutGroup("Scriptable")][SerializeField] private RSE_CheckpointReached m_rseCheckpointReached;
	[FoldoutGroup("Scriptable")][SerializeField] private RSE_RestartChrono m_rseRestartChrono;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_GamePaused m_rsoGamePaused;
	[FoldoutGroup("Scriptable")][SerializeField] private RSO_StartDate m_rsoStartDate;

	private TimeSpan m_currentTime;
	private TimeSpan m_elapsedOnPaused;
	private bool m_resetOnPaused;
	private DateTime m_startPauseDate;
	private StringBuilder m_output;
	private List<string> m_checkpoints = new List<string>();

	public override void Start()
	{
		base.Start();
		Reset();
	}

	private void OnEnable()
	{
		m_rseCheckpointReached.action += AddCheckpoint;
		m_rsoGamePaused.OnChanged += OnGamePaused;
	}

	private void OnDisable()
	{
		m_rseCheckpointReached.action -= AddCheckpoint;
		m_rsoGamePaused.OnChanged -= OnGamePaused;
	}

	private void Update()
	{
		// Assertions
		if (!m_ssoGame.EnableChrono) return;
		if (m_rsoGamePaused.value) return;

		m_currentTime = DateTime.Now.Subtract(m_rsoStartDate.value);
		UpdateGraphics();
	}

	private void UpdateGraphics()
	{
		m_output = new StringBuilder();

		TimeSpan time = m_currentTime.Subtract(m_elapsedOnPaused);
		m_output.Append($"{time.Minutes}:{time.Seconds}:{time.Milliseconds}");
		
		foreach (var checkpoint in m_checkpoints)
		{
			m_output.Append($"\n{checkpoint}");
		}

		m_tmpChrono.text = m_output.ToString();
	}

	private void OnGamePaused()
	{
		// Assertion
		if (!m_ssoGame.EnableChrono) return;

		if (m_rsoGamePaused.value)
		{
			m_startPauseDate = DateTime.Now;
			Show();
		}
		else
		{
			if (m_resetOnPaused) 
			{
				m_resetOnPaused = false;
				m_startPauseDate = DateTime.Now;
			}

			m_elapsedOnPaused += DateTime.Now.Subtract(m_startPauseDate);
			Hide();
		}
	}

	private void AddCheckpoint(string checkpointName)
	{
		// Assertion
		if (!m_ssoGame.EnableChrono) return;

		TimeSpan time = m_currentTime.Subtract(m_elapsedOnPaused);
		m_checkpoints.Add($"{checkpointName}: {time.Minutes}:{time.Seconds}:{time.Milliseconds}");
	}

	private void Restart()
	{
		m_startPauseDate = DateTime.Now;
		m_rsoStartDate.value = DateTime.Now;

		m_currentTime = TimeSpan.Zero;
		m_elapsedOnPaused = TimeSpan.Zero;

		m_checkpoints = new List<string>();
	}

	public void Reset()
	{
		Restart();
		m_rseRestartChrono.Call();
		UpdateGraphics();
		if (m_rsoGamePaused.value) m_resetOnPaused = true;
		print(m_rsoStartDate.value);
	}
}