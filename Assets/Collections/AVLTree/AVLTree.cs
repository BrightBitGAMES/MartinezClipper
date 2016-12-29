//////////////////////////////////////////////////////////////////
//                                                              //
//  This file is part of BrightBit's Martinez Clipper package.  //
//                                                              //
//  Copyright (c) 2016 by BrightBit                             //
//                                                              //
//  This software may be modified and distributed under         //
//  the terms of the MIT license. See the LICENSE file          //
//  for details.                                                //
//                                                              //
//////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BrightBit
{

namespace Collections
{

/// <summary>
/// 
/// </summary>

public class AVLTree<T> : ICollection<T>
{
    public delegate void VisitHandler(AVLNode<T> n);

    AVLNode<T> root;

    public AVLTree()                          : this(Comparer<T>.Default)             {}
    public AVLTree(IComparer<T> comparer)     : this(Enumerable.Empty<T>(), comparer) {}
    public AVLTree(IEnumerable<T> collection) : this(collection, Comparer<T>.Default) {}

    public AVLTree(IEnumerable<T> collection, IComparer<T> comparer)
    {
        if (collection == null) throw new ArgumentNullException("collection");
        if (comparer == null)   throw new ArgumentNullException("comparer");

        Comparer = comparer;

        if (collection != null)
        {
            foreach (T item in collection)
            {
                this.Add(item);
            }
        }
    }

    protected IComparer<T> Comparer { get; set; }

    /// <summary>
    /// 
    /// Complexity: O(log n)
    /// </summary>

    public void Add(T value)
    {
        Insert(value);
    }

    public AVLNode<T> Insert(T value)
    {
        AVLNode<T> result = null;

        root = Add(root, value, out result);

        return result;
    }

    /// <summary>
    /// 
    /// Complexity: O(log n)
    /// </summary>

    public bool Remove(T value)
    {
        bool foundElement = false;

        root = Remove(root, value, ref foundElement);

        return foundElement;
    }

    public bool RemoveAt(AVLNode<T> node)
    {
        if (node == null) return false;

        if (node.Left == null || node.Right == null)
        {
            AVLNode<T> oldParent = node.Parent;

            bool nodeWasLeft = oldParent != null && oldParent.Left == node;

            AVLNode<T> target = node.Left != null ? node.Left : node.Right;

            if (oldParent != null && nodeWasLeft) oldParent.Left  = target;
            else if (oldParent != null)           oldParent.Right = target;

            if (target != null) target.Parent = oldParent;
            if (target == null) target = oldParent;
            if (target == null) root = null;

            while (target != null)
            {
                BalanceBasedOnBalance(target);

                if (target.Parent == null) root = target;

                target = target.Parent;
            }

            return true;
        }
        else
        {
            AVLNode<T> rightMin = node.Right.GetFarLeft();

            Swap(node, rightMin);

            return RemoveAt(node);
        }
    }

    void Swap(AVLNode<T> a, AVLNode<T> b)
    {
        if (a == null || b == null) return;

        bool aWasLeft = a.Parent != null && a.Parent.Left == a;
        bool bWasLeft = b.Parent != null && b.Parent.Left == b;

        AVLNode<T> tempLeft   = a.Left;
        AVLNode<T> tempRight  = a.Right;
        AVLNode<T> tempParent = a.Parent;
        int tempHeight        = a.Height;

        a.Left   = b.Left;
        a.Right  = b.Right;
        a.Parent = b.Parent;
        a.Height = b.Height;

        b.Left   = tempLeft;
        b.Right  = tempRight;
        b.Parent = tempParent;
        b.Height = tempHeight;

        // if 'b' was the left node, right node or parent of 'a'
        if (b.Left == b)        b.Left   = a;
        else if (b.Right == b)  b.Right  = a;
        else if (b.Parent == b) b.Parent = a;

        // if 'a' was the left node, right node or parent of 'b'
        if (a.Left == a)        a.Left   = b;
        else if (a.Right == a)  a.Right  = b;
        else if (a.Parent == a) a.Parent = b;

        // swapping all pointers as in the following lines
        // keeps all AVLNodes the user might store valid

        if (a.Left != null)  a.Left.Parent  = a;
        if (a.Right != null) a.Right.Parent = a;

        if (b.Left != null)  b.Left.Parent  = b;
        if (b.Right != null) b.Right.Parent = b;

        if (a.Parent != null && bWasLeft) a.Parent.Left  = a;
        else if (a.Parent != null)        a.Parent.Right = a;

        if (b.Parent != null && aWasLeft) b.Parent.Left  = b;
        else if (b.Parent != null)        b.Parent.Right = b;
    }

    /// <summary>
    /// 
    /// Complexity: O(log n)
    /// </summary>

    public T GetMin()
    {
        AVLNode<T> result = GetMinNode();

        return result != null ? result.Value : default(T);
    }

    public AVLNode<T> GetMinNode()
    {
        return root != null ? root.GetFarLeft() : null;
    }

    /// <summary>
    /// 
    /// Complexity: O(log n)
    /// </summary>
    public bool GetMax(out T value)
    {
        if (root != null)
        {
            value = root.GetFarRight().Value;

            return true;
        }

        value = default(T);

        return false;
    }

    public void Traverse(VisitHandler visitor)
    {
        if (root != null && visitor != null) InOrder(root, visitor);
    }

    public override string ToString()
    {
        string result = "";

        List<List<AVLNode<T>>> childrenStack = new List<List<AVLNode<T>>>();
        childrenStack.Add(new List<AVLNode<T>> { root });

        while (childrenStack.Count > 0)
        {
            List<AVLNode<T>> childQueue = childrenStack[childrenStack.Count - 1];

            if (childQueue.Count == 0)
            {
                childrenStack.RemoveAt(childrenStack.Count - 1);
            }
            else
            {
                AVLNode<T> node = childQueue[0];
                childQueue.RemoveAt(0);

                string prefix = "";
                for (int i = 0; i < childrenStack.Count - 1; ++i)
                {
                    prefix += (childrenStack[i].Count > 0) ? "|  " : "   ";
                }

                string side = " ";

                if (node.Parent != null) side = node.Parent.Left == node ? "L" : "R";

                result += prefix + "+-" + side + " " + node.Value + "\n";

                List<AVLNode<T>> children = new List<AVLNode<T>>();

                if (node.Left != null) children.Add(node.Left);
                if (node.Right != null) children.Add(node.Right);

                if (children.Count > 0) childrenStack.Add(children);
            }
        }

        return result;
    }

    void InOrder(AVLNode<T> node, VisitHandler visitor)
    {
        if (node.Left != null) InOrder(node.Left, visitor);

        visitor(node);

        if (node.Right != null) InOrder(node.Right, visitor);
    }

    void PreOrder(AVLNode<T> node, VisitHandler visitor)
    {
        visitor(node);

        if (node.Left != null) PreOrder(node.Left, visitor);
        if (node.Right != null) PreOrder(node.Right, visitor);
    }

    void PostOrder(AVLNode<T> node, VisitHandler visitor)
    {
        if (node.Left != null) PreOrder(node.Left, visitor);
        if (node.Right != null) PreOrder(node.Right, visitor);

        visitor(node);
    }

    /// <summary>
    /// 
    /// Complexity: O(log n)
    /// </summary>
    public bool Contains(T arg)
    {
        return Find(arg) != null;
    }

    /// <summary>
    /// 
    /// Complexity: O(1).
    /// </summary>
    public void Clear()
    {
        root = null;
    }

    public bool IsReadOnly { get { return false; } }

    public int Count
    {
        get
        {
            int result = 0;

            Traverse((node) => { ++result; });

            return result;
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        IEnumerator<T> enumerator = GetEnumerator();

        while (enumerator.MoveNext())
        {
            array[arrayIndex++] = enumerator.Current;
        }
    }

    static int DetermineHeight(AVLNode<T> node)
    {
        if (node == null) return 0;

        return node.Height;
    }

    static int CalculateBalance(AVLNode<T> node)
    {
        if (node == null) return 0;

        return DetermineHeight(node.Left) - DetermineHeight(node.Right);
    }

    AVLNode<T> BalanceBasedOnBalance(AVLNode<T> node)
    {
        if (node == null) return null;

        node.Height = Math.Max(DetermineHeight(node.Left), DetermineHeight(node.Right)) + 1;

        int balance = CalculateBalance(node);

        if (balance >  1 && CalculateBalance(node.Left)  >= 0) return RotateRight(node); // Left Left Case
        if (balance < -1 && CalculateBalance(node.Right) <= 0) return RotateLeft(node);  // Right Right Case

        if (balance > 1 && CalculateBalance(node.Left) < 0) // Left Right Case
        {
            node.Left = RotateLeft(node.Left);
            return RotateRight(node);
        }

        if (balance < -1 && CalculateBalance(node.Right) > 0) // Right Left Case
        {
            node.Right = RotateRight(node.Right);
            return RotateLeft(node);
        }

        return node;
    }

    AVLNode<T> BalanceBasedOnValue(AVLNode<T> node, T value)
    {
        //if (node == null) return null;

        node.Height = Math.Max(DetermineHeight(node.Left), DetermineHeight(node.Right)) + 1;

        int balance = CalculateBalance(node);

        if (balance >  1 && Comparer.Compare(value, node.Left.Value)  <= 0) return RotateRight(node); // Left Left Case
        if (balance < -1 && Comparer.Compare(value, node.Right.Value) >= 0) return RotateLeft(node);  // Right Right Case

        if (balance > 1 && Comparer.Compare(value, node.Left.Value) > 0) // Left Right Case
        {
            node.Left = RotateLeft(node.Left);
            return RotateRight(node);
        }

        if (balance < -1 && Comparer.Compare(value, node.Right.Value) < 0) // Right Left Case
        {
            node.Right = RotateRight(node.Right);
            return RotateLeft(node);
        }

        return node;
    }

    void UpdateHeight(AVLNode<T> node, AVLNode<T> parent)
    {
        if (node != parent)
        {
            node.Height = Math.Max(DetermineHeight(node.Left), DetermineHeight(node.Right)) + 1;

            UpdateHeight(node.Parent, parent);
        }
    }

    AVLNode<T> Add(AVLNode<T> node, T value, out AVLNode<T> result)
    {
        if (node == null)
        {
            result = new AVLNode<T>(value);
            return result;
        }

        if (Comparer.Compare(value, node.Value) < 0)
        {
            node.Left        = Add(node.Left, value, out result);
            node.Left.Parent = node;
        }
        else
        {
            node.Right        = Add(node.Right, value, out result);
            node.Right.Parent = node;
        }

        return BalanceBasedOnValue(node, value);
    }

    AVLNode<T> Remove(AVLNode<T> node, T value, ref bool wasFound)
    {
        if (node == null) return null;

        if (Comparer.Compare(value, node.Value) < 0)      node.Left  = Remove(node.Left, value, ref wasFound);
        else if (Comparer.Compare(value, node.Value) > 0) node.Right = Remove(node.Right, value, ref wasFound);
        else
        {
            if (node.Left == null || node.Right == null)
            {
                AVLNode<T> oldParent = node.Parent;

                if (node.Left == null) node = node.Right;
                else                   node = node.Left;

                if (node != null) node.Parent = oldParent;

                wasFound = true;
            }
            else
            {
                AVLNode<T> rightMin = node.Right.GetFarLeft();
                node.Value          = rightMin.Value;
                node.Right          = Remove(node.Right, rightMin.Value, ref wasFound);
            }
        }

        return BalanceBasedOnBalance(node);
    }

    AVLNode<T> Find(T value)
    {
        AVLNode<T> current = root;

        while (current != null)
        {
            if (Comparer.Compare(value, current.Value) < 0)      current = current.Left;
            else if (Comparer.Compare(value, current.Value) > 0) current = current.Right;
            else                                                 return current;
        }

        return null;
    }

    static AVLNode<T> RotateLeft(AVLNode<T> node)
    {
        AVLNode<T> right     = node.Right;
        AVLNode<T> rightLeft = right.Left;
        node.Right           = rightLeft;

        AVLNode<T> parent = node.Parent;

        if (rightLeft != null) rightLeft.Parent = node;

        right.Left  = node;
        node.Parent = right;

        if (parent != null)
        {
            if (parent.Left == node) parent.Left  = right;
            else                     parent.Right = right;
        }

        right.Parent = parent;

        node.Height  = Math.Max(DetermineHeight(node.Left),  DetermineHeight(node.Right))  + 1;
        right.Height = Math.Max(DetermineHeight(right.Left), DetermineHeight(right.Right)) + 1;

        return right;
    }

    static AVLNode<T> RotateRight(AVLNode<T> node)
    {
        AVLNode<T> left      = node.Left;
        AVLNode<T> leftRight = left.Right;
        AVLNode<T> parent    = node.Parent;

        node.Left   = leftRight;
        node.Parent = left;
        left.Parent = parent;
        left.Right  = node;

        if (leftRight != null) leftRight.Parent = node;

        if (parent != null)
        {
            if (parent.Left == node) parent.Left  = left;
            else                     parent.Right = left;
        }

        node.Height = Math.Max(DetermineHeight(node.Left), DetermineHeight(node.Right)) + 1;
        left.Height = Math.Max(DetermineHeight(left.Left), DetermineHeight(left.Right)) + 1;

        return left;
    }

    public IEnumerator<T> GetEnumerator()   { return new AVLNodeEnumerator(this); }
    IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator();             }

    class AVLNodeEnumerator : IEnumerator<T>
    {
        AVLTree<T> container       = null;
        AVLNode<T> currentPosition = null;

        bool isReset = true;

        public AVLNodeEnumerator(AVLTree<T> container)
        {
            this.container = container;

            Reset();
        }

        public bool MoveNext()
        {
            if (!isReset && currentPosition == null) return false;

            if (isReset == true) currentPosition = container.root != null ? container.root.GetFarLeft() : null;
            else                 currentPosition = currentPosition.GetSuccessor();

            isReset = false;

            return (currentPosition != null);
        }

        void IDisposable.Dispose() { }

        public void Reset()
        {
            isReset = true;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public T Current
        {
            get
            {
                try
                {
                    return currentPosition.Value;
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}

} // of namespace Collections

} // of namespace BrightBit
