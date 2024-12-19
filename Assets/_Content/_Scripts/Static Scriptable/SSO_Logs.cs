using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SSO_Logs", menuName = "Static Scriptable/Logs")]
public class SSO_Logs : ScriptableObject
{
	public List<SSO_Log> Logs = new List<SSO_Log>();

#if UNITY_EDITOR

	[ReadOnly][ShowInInspector][HideLabel] 
	[PropertySpace(SpaceAfter = 0, SpaceBefore = 15)]
	private const string k_logsPath = "Assets/_Content/Static Scriptable/Logs";

	[Button]
	public void GatherLogs()
	{
		Logs.Clear();
		string[] assets = Directory.GetFiles(k_logsPath, "*.asset", SearchOption.AllDirectories);
		foreach (var asset in assets)
		{
			var ssoLog = AssetDatabase.LoadAssetAtPath<SSO_Log>(asset);
			if (ssoLog)
			{
				Logs.Add(ssoLog);
			}
		}
	}

#endif

}