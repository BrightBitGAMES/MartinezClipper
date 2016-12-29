using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BrightBit
{

namespace Collections
{

/// <summary>
/// Heap base class used by MinHeap and MaxHeap
/// </summary>
public abstract class Heap<T> : IEnumerable<T>
{
    List<T> heap = new List<T>();

    public int Count { get { return heap.Count; } }

    protected Heap()                          : this(Comparer<T>.Default)             {}
    protected Heap(IComparer<T> comparer)     : this(Enumerable.Empty<T>(), comparer) {}
    protected Heap(IEnumerable<T> collection) : this(collection, Comparer<T>.Default) {}

    protected Heap(IEnumerable<T> collection, IComparer<T> comparer)
    {
        if (collection == null) throw new ArgumentNullException("collection");
        if (comparer == null)   throw new ArgumentNullException("comparer");

        Comparer = comparer;

        foreach (var item in collection) heap.Add(item);

        heap.StableSort();
    }

    /// <summary>
    /// Template method defined by derived classes that will be used
    /// to decide which items of the heap are 'more important'
    /// </summary>
    protected abstract bool Dominates(T x, T y);

    protected IComparer<T> Comparer { get; set; }

    /// <summary>
    /// 
    /// Complexity: O(1)
    /// </summary>
    public bool IsEmpty()
    {
        return Count == 0;
    }

    /// <summary>
    /// 
    /// Complexity: O(log n)
    /// </summary>
    public void Add(T item)
    {
        heap.Add(item);

        BubbleUp(Count - 1);
    }

    /// <summary>
    /// 
    /// Complexity: O(1)
    /// </summary>
    public T First()
    {
        if (IsEmpty()) throw new InvalidOperationException("The Heap instance is empty!");

        return heap[0];
    }

    /// <summary>
    /// 
    /// Complexity: O(log n)
    /// </summary>
    public T ExtractFirst()
    {
        T result = First();

        heap.Swap(0, Count - 1);
        heap.RemoveAt(Count - 1);

        BubbleDown(0);

        return result;
    }

    public void Clear()
    {
        heap.Clear();
    }

    /// <summary>
    /// This method repositions an element of the heap
    /// to an earlier location in the heap, i.e. the element at 'i'
    /// is known to be more important.
    /// Complexity: O(log n)
    /// </summary>
    void BubbleUp(int i)
    {
        if (i == 0 || Dominates(heap[Parent(i)], heap[i])) return;

        heap.Swap(i, Parent(i));

        BubbleUp(Parent(i));
    }

    /// <summary>
    /// This method repositions an element of the heap
    /// to a later location in the heap, i.e. the element at 'i'
    /// is known to be less important.
    /// Complexity: O(log n)
    /// </summary>
    void BubbleDown(int i)
    {
        int dominatingNode = Dominating(i);

        if (dominatingNode == i) return;

        heap.Swap(i, dominatingNode);

        BubbleDown(dominatingNode);
    }

    int Dominating(int i)
    {
        int dominatingNode = i;

        dominatingNode = GetDominating(LeftChild(i), dominatingNode);
        dominatingNode = GetDominating(RightChild(i), dominatingNode);

        return dominatingNode;
    }

    int GetDominating(int indexToCheck, int indexOfDominating)
    {
        if (indexToCheck < Count && Dominates(heap[indexToCheck], heap[indexOfDominating])) return indexToCheck;
        else                                                                                return indexOfDominating;
    }

    /// Indices: |    0   |   1  |   2   |       3     |       4      |       5      | ...
    ///          | Parent | Left | Right | Left's Left | Left's Right | Right's Left | ...
    ///
    ///                        0
    ///                      /   \
    ///                     1     2
    ///                    / \   / \
    ///                   3  4  5   6

    static int LeftChild(int i)  { return RightChild(i) - 1; }
    static int RightChild(int i) { return ((i + 1) * 2);     }
    static int Parent(int i)     { return ((i + 1) / 2) - 1; }

    public IEnumerator<T> GetEnumerator()   { return heap.GetEnumerator(); } // the enumeration won't be sorted!
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator();      }
}

public sealed class MinHeap<T> : Heap<T>
{
    public MinHeap()                                                 : this(Comparer<T>.Default)  {}
    public MinHeap(IComparer<T> comparer)                            : base(comparer)             {}
    public MinHeap(IEnumerable<T> collection)                        : base(collection)           {}
    public MinHeap(IEnumerable<T> collection, IComparer<T> comparer) : base(collection, comparer) {}

    protected override bool Dominates(T x, T y) { return Comparer.Compare(x, y) < 0; }
}

public sealed class MaxHeap<T> : Heap<T>
{
    public MaxHeap()                                                 : this(Comparer<T>.Default)  {}
    public MaxHeap(IComparer<T> comparer)                            : base(comparer)             {}
    public MaxHeap(IEnumerable<T> collection)                        : base(collection)           {}
    public MaxHeap(IEnumerable<T> collection, IComparer<T> comparer) : base(collection, comparer) {}

    protected override bool Dominates(T x, T y) { return Comparer.Compare(x, y) > 0; }
}

} // of namespace Collections

} // of namespace BrightBit
