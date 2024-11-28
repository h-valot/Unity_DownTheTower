using System.Collections.Generic;
using UnityEngine;

public class PermanentSaver : MonoBehaviour
{
	public List<Permanent> Placeables;
	public List<PermanentData> PlaceableDatas;

	public void Save()
	{
		// TODO - Save the position of all placeable placed 
	}

	public void Load()
	{
		// TODO - Instantiate a placeable at the position of each former placeable stored
	}
}