using System.Collections.Generic;
using UnityEngine;

public class PermanentSaver : MonoBehaviour
{
	public List<Permanent> placeables;
	public List<PermanentData> placeableDatas;

	public void Save()
	{
		// save the position of all placeable placed 
	}

	public void Load()
	{
		// instantiate a placeable at the position of each former placeable stored
	}
}