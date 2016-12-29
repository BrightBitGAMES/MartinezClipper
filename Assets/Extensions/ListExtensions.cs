using System;
using System.Collections.Generic;
using System.Linq;

public static class ListExtensions
{
    public static void Swap<T>(this IList<T> list, int a, int b)
	{
		T tmp   = list[a];
        list[a] = list[b];
        list[b] = tmp;
	}

    public static void StableSort<T>(this List<T> list) //where T : IComparable<T>
    {
        IList<T> sortedResult = list.OrderBy(x => x).ToList();

        list.Clear();
        list.AddRange(sortedResult);
    }
}
