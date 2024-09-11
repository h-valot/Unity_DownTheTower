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

	public static void Log(string message, bool onUnityConsole = true)
	{
		console.Log(message);
		if (onUnityConsole) UnityEngine.Debug.Log(message);
	}

	public static void LogWarning(string message, bool onUnityConsole = true)
	{
		console.LogWarning(message);
		if (onUnityConsole) UnityEngine.Debug.LogWarning(message);
	}

	public static void LogError(string message, bool onUnityConsole = true)
	{
		console.LogError(message);
		if (onUnityConsole) UnityEngine.Debug.LogError(message);
	}
}