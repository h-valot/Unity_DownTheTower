using System.Collections.Generic;
using UnityEngine;

public class PlaceableSaver : MonoBehaviour
{
	public List<Placeable> placeables;
	public List<PlaceableData> placeableDatas;

	public void Save()
	{
		// save the position of all placeable placed 
	}

	public void Load()
	{
		// instantiate a placeable at the position of each former placeable stored
	}
}