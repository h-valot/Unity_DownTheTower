using UnityEngine;

public class DebuggerSingleton : MonoBehaviour
{
	private void Awake()
	{
		// Prevent debugger dupe
		if (Resources.FindObjectsOfTypeAll<DebuggerSingleton>().Length - 1 > 1)
		{
			Debug.LogWarning($"DEBUGGER_SINGLETON: Found {Resources.FindObjectsOfTypeAll<DebuggerSingleton>().Length - 1} dupe(s). Deleting the new one.");
			Destroy(gameObject);
		}
	}
}