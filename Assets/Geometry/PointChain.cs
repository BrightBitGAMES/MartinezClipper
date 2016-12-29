using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BrightBit
{

namespace Geometry
{

public class PointChain
{
    LinkedList<Vector2> points = new LinkedList<Vector2>();

    bool _closed; // is the first point linked with the last one

    public PointChain(Edge edge)
    {
        _closed = false;

        Add(edge);
    }

    public override string ToString()
    {
        return System.String.Join(", ", points.Select(p => p.ToString()).ToArray());
    }

    public void Add(Edge edge)
    {
        points.AddLast(edge.From);
        points.AddLast(edge.To);
    }

    public bool LinkSegment(Edge edge)
    {
        if (edge.From == points.First())
        {
            if (edge.To == points.Last()) _closed = true;
            else                          points.AddFirst(edge.To);

            return true;
        }

        if (edge.To == points.Last())
        {
            if (edge.From == points.First()) _closed = true;
            else                             points.AddLast(edge.From);

            return true;
        }

        if (edge.To == points.First())
        {
            if (edge.From == points.Last()) _closed = true;
            else                            points.AddFirst(edge.From);

            return true;
        }

        if (edge.From == points.Last())
        {
            if (edge.To == points.First()) _closed = true;
            else                           points.AddLast(edge.To);

            return true;
        }

        return false;
    }

    public bool LinkPointChain(PointChain chain)
    {
        if (chain.points.First() == points.Last())
        {
            chain.points.RemoveFirst();
            points.AppendRange(chain.points);

            return true;
        }

        if (chain.points.Last() == points.First())
        {
            points.RemoveFirst();
            points.PrependRange(chain.points);

            return true;
        }

        if (chain.points.First() == points.First())
        {
            points.RemoveFirst();
            chain.points.ReverseOrder();
            points.PrependRange(chain.points);

            return true;
        }

        if (chain.points.Last() == points.Last())
        {
            points.RemoveLast();
            chain.points.ReverseOrder();
            points.AppendRange(chain.points);

            return true;
        }

        return false;
    }

    public LinkedListNode<Vector2> First { get { return points.First; } }
    public LinkedListNode<Vector2> Last  { get { return points.Last;  } }

    public bool IsClosed()      { return _closed;      }
    public void Clear()         { points.Clear();      }
    public int GetNumPoints()   { return points.Count; }
};

} // of namespace Geometry

} // of namespace BrightBit
