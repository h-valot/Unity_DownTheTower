using UnityEngine;

public static class Debug
{
	private static Console s_console;

	public static Console Console
	{
		get
		{
			// Find console if cached value is null
			if (s_console == null) s_console = GameObject.FindFirstObjectByType<Console>();
			return s_console;
		}
	}

	public static void Log(string message, bool onUnityConsole = true)
	{
		Console.Log(message);
		if (onUnityConsole) UnityEngine.Debug.Log(message);
	}

	public static void LogWarning(string message, bool onUnityConsole = true)
	{
		Console.LogWarning(message);
		if (onUnityConsole) UnityEngine.Debug.LogWarning(message);
	}

	public static void LogError(string message, bool onUnityConsole = true)
	{
		Console.LogError(message);
		if (onUnityConsole) UnityEngine.Debug.LogError(message);
	}
}