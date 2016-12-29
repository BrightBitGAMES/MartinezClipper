using System;
using System.Collections.Generic;
using System.Linq;

public static class LinkedListExtensions   
{
    public static void AppendRange<T>(this LinkedList<T> source, IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            source.AddLast(item);
        }
    }

    public static void PrependRange<T>(this LinkedList<T> source, IEnumerable<T> items)
    {
        foreach (T item in items.Reverse())
        {
            source.AddFirst(item);
        }
    }

    public static void ReverseOrder<T>(this LinkedList<T> source)
    {
        LinkedListNode<T> head    = source.First;
        LinkedListNode<T> current = head.Next;

        while (current != null)
        {
            LinkedListNode<T> next = current.Next;

            source.AddBefore(head, current.Value);
            source.Remove(current);

            head    = head.Previous;
            current = next;
        }
    }
}
