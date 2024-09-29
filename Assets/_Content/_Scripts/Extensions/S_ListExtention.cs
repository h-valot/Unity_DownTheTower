using System;
using System.Collections.Generic;
using System.Linq;

public static class ListExtensions
{
	/// <summary>
	/// 	"randomly" shuffle the position of all elements in the list.
	/// </summary>
	public static void Shuffle<T>(this IList<T> list)
	{
		int count = list.Count;
		int last = count - 1;

		for (var i = 0; i < last; ++i)
		{
			int rnd = UnityEngine.Random.Range(i, count);
			(list[i], list[rnd]) = (list[rnd], list[i]);
		}
	}

	/// <summary>
	/// 	returns a new list with all elements cloned using linq library.
	/// </summary>
	public static IList<T> Clone<T>(this IList<T> list) where T : ICloneable
	{
		return list.Select(item => (T)item.Clone()).ToList();
	}

	/// <summary>
	/// 	add the given object to the list only if it does not already contain it.
	/// </summary>
	/// <param name="toAdd">object that can be added to the list</param>
	public static void AddUnique<T>(this IList<T> list, T toAdd)
	{
		if (!list.Contains(toAdd)) list.Add(toAdd);
	}
}