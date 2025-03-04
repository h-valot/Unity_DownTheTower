using System;
using System.Collections.Generic;
using System.Linq;

public static class ListExtensions
{
	/// <summary>
	/// "Randomly" shuffle the position of all elements in the list.
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
	/// Return a new list with all elements cloned using linq library.
	/// </summary>
	public static IList<T> Clone<T>(this IList<T> list) where T : ICloneable
	{
		return list.Select(item => (T)item.Clone()).ToList();
	}

	/// <summary>
	/// Return a new list with all elements from the former list.
	/// </summary>
	public static List<T> Duplicate<T>(this List<T> list)
	{
		return new List<T>(list);
	}

	/// <summary>
	/// Append all the elements of the given list into the other list.
	/// </summary>
	public static List<T> Append<T>(this List<T> list, List<T> listToAdd)
	{
		foreach (var item in listToAdd) list.Add(item);
		return list;
	}

	/// <summary>
	/// Append all the elements of the given list into the other list.
	/// </summary>
	public static List<T> Append<T>(this List<T> list, IEnumerable<T> listToAdd)
	{
		foreach (var item in listToAdd) list.Add(item);
		return list;
	}

	/// <summary>
	/// Add the given object to the list only if it does not already contain it.
	/// </summary>
	/// <param name="toAdd">Object that can be added to the list.</param>
	public static void AddUnique<T>(this IList<T> list, T toAdd)
	{
		if (!list.Contains(toAdd)) list.Add(toAdd);
	}

	/// <summary>
	/// Add the given object to the list only if it does not already contain it. 
	/// Return a bool using a callback that stats if the object has been successfully added to the list or not.
	/// </summary>
	/// <param name="toAdd">Object that can be added to the list.</param>
	/// <param name="callback">True if the object have been successfully added to the list.</param>
	public static void AddUnique<T>(this IList<T> list, T toAdd, Action<bool> callback)
	{
		if (!list.Contains(toAdd))
		{
			list.Add(toAdd);
			callback(true);
			return;
		}

		callback(false);
	}
}