using UnityEngine;

public class DebuggerSingleton : MonoBehaviour
{
	private void Awake()
	{
		// prevent debugger dupe
		if (Resources.FindObjectsOfTypeAll<DebuggerSingleton>().Length - 1 > 1)
		{
			Debug.Log($"DEBUGGER_SINGLETON: found {Resources.FindObjectsOfTypeAll<DebuggerSingleton>().Length - 1} dupe(s)", false);
			Destroy(gameObject);
		}
	}
}