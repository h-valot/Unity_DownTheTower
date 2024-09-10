using UnityEngine;

public static class Debug
{
	private static Console _console;
	public static Console console
	{
		get
		{
			// find console if cached value is null
			if (_console == null) _console = GameObject.FindFirstObjectByType<Console>();
			return _console;
		}
	}

	public static void Log(string message)
	{
		UnityEngine.Debug.Log(message);
		console.Log(message);
	}

	public static void LogWarning(string message)
	{
		UnityEngine.Debug.LogWarning(message);
		console.LogWarning(message);
	}

	public static void LogError(string message)
	{
		UnityEngine.Debug.LogError(message);
		console.LogError(message);
	}
}