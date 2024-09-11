using UnityEngine;

public class DebuggerSingleton : MonoBehaviour
{
	private void Awake()
	{
		// prevent debugger dupe
		if (Resources.FindObjectsOfTypeAll<DebuggerSingleton>().Length - 1 > 1)
		{
			Debug.LogWarning($"DEBUGGER_SINGLETON: found {Resources.FindObjectsOfTypeAll<DebuggerSingleton>().Length - 1} dupe(s). deleting the new one.", false);
			Destroy(gameObject);
		}
	}
}