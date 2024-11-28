using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class OptimizedPropManager : MonoBehaviour
{
	[FoldoutGroup("Scriptable")][SerializeField] private SSO_Optimization m_ssoOptimization;

	[FoldoutGroup("Scriptable")][SerializeField] private RSO_CharacterPosition m_rsoPlayerTransform;

	private List<OptimizedProp> m_props;

	// /!\ THAT IS TEMPORARY /!\ //
	// There is very certainly other ways to optimize this aspect of the game.
	// This is not important at the moment.

	public void Start()
	{
		GatherOptimizedProps();
		StartCoroutine(UpdateLoadability());
	}

	public void GatherOptimizedProps()
	{
		// Get every optimized object from the scene
		m_props = new List<OptimizedProp>(Resources.FindObjectsOfTypeAll<OptimizedProp>().ToList());
	}

	public IEnumerator UpdateLoadability()
	{
		while (true)
		{
			yield return new WaitForSecondsRealtime(m_ssoOptimization.OptimizedPropUpdateTimer);

			foreach (var prop in m_props)
			{
				prop.gameObject.SetActive(Vector3.Distance(m_rsoPlayerTransform.value, prop.transform.position) < m_ssoOptimization.OptimizedPropRenderDistance);
			}
		}
	}
}