using UnityEngine;
using System.Collections.Generic;

namespace BrightBit
{

namespace Geometry
{

public class Connector
{
    public delegate void OnAddEdge(Edge edge);
    public event OnAddEdge OnAddEdgeEvent;

    public void Add(Edge edge)
    {
        OnAddEdgeEvent(edge);

        LinkedListNode<PointChain> current = openPolygons.First;

        while (current != null)
        {
            LinkedListNode<PointChain> next = current.Next;

            PointChain currentChain = current.Value;

            if (currentChain.LinkSegment(edge))
            {
                if (currentChain.IsClosed())
                {
                    closedPolygons.AddLast(currentChain);
                    openPolygons.Remove(current);
                }
                else
                {
                    LinkedListNode<PointChain> innerCurrent = current.Next;

                    while (innerCurrent != null)
                    {
                        LinkedListNode<PointChain> innerNext = innerCurrent.Next;

                        if (currentChain.LinkPointChain(innerCurrent.Value))
                        {
                            openPolygons.Remove(innerCurrent);
                            break;
                        }

                        innerCurrent = innerNext;
                    }
                }

                return;
            }

            current = next;
        }

        openPolygons.AddLast(new PointChain(edge));
    }

    public Polygon CreatePolygon()
    {
        Polygon result = new Polygon();

        LinkedListNode<PointChain> current = closedPolygons.First;

        while (current != null)
        {
            SimpleClosedPath path = new SimpleClosedPath(result);

            LinkedListNode<Vector2> innerCurrent = current.Value.First;

            while (innerCurrent != null)
            {
                path.Add(innerCurrent.Value);

                innerCurrent = innerCurrent.Next;
            }

            result.Add(path);

            current = current.Next;
        }

        return result;
    }

	LinkedListNode<PointChain> begin() { return closedPolygons.First; }
	LinkedListNode<PointChain> end()   { return closedPolygons.Last;  }

	public void Clear()
    {
        closedPolygons.Clear();
        openPolygons.Clear();
    }

	int GetNumClosedPolygons() { return closedPolygons.Count; }

	LinkedList<PointChain> openPolygons   = new LinkedList<PointChain>();
	LinkedList<PointChain> closedPolygons = new LinkedList<PointChain>();
}

} // of namespace Geometry

} // of namespace BrightBit
