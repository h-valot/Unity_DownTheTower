using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OptimizedPropManager : MonoBehaviour
{
	[Header("External references")]
	[SerializeField] private RSO_PlayeGraphicsDirection _rsoPlayerTransform;
	[SerializeField] private WorldConfig _worldConfig;

	private List<OptimizedProp> props;

	// /!\ THAT IS TEMPORARY /!\ //
	// there is very certainly other ways to optimize this aspect of the game
	// this is not important at the moment

	public void Start()
	{
		GatherOptimizedProps();
		StartCoroutine(UpdateLoadability());
	}

	public void GatherOptimizedProps()
	{
		// get every optimized object from the scene
		props = new List<OptimizedProp>(Resources.FindObjectsOfTypeAll<OptimizedProp>().ToList());
	}

	public IEnumerator UpdateLoadability()
	{
		while (true)
		{
			yield return new WaitForSecondsRealtime(_worldConfig.optimizedPropUpdateTimer);

			foreach (var prop in props)
			{
				prop.gameObject.SetActive(Vector3.Distance(_rsoPlayerTransform.value.position, prop.transform.position) < _worldConfig.optimizedPropRenderDistance);
			}
		}
	}
}