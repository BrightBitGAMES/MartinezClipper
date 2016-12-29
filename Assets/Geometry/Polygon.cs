using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using BrightBit.Collections;

namespace BrightBit
{

namespace Geometry
{

public class SimpleClosedPath
{
    Polygon owner;

	List<Vector2> points = new List<Vector2>();
    List<int> holes      = new List<int>();

	bool isHole;
	bool alreadyCalled;
	bool isCounterClockwise;

	public SimpleClosedPath(Polygon owner)
    {
        this.owner    = owner;

        isHole        = false;
        alreadyCalled = false;
    }

    public SimpleClosedPath(SimpleClosedPath c, Polygon owner)
    {
        this.owner = owner;

        foreach (Vector2 p in c.points) points.Add(new Vector2(p.x, p.y));
        foreach (int holeIndex in c.holes) holes.Add(holeIndex);

        isHole             = c.isHole;
        alreadyCalled      = c.alreadyCalled;
        isCounterClockwise = c.isCounterClockwise;
    }

    public IEnumerable<Vector2> Points()
    {
        for (int i = 0; i < points.Count; ++i) yield return points[i];
    }

	public Vector2 GetPoint(int i)
    {
        return points[i];
    }

	public Edge GetEdge(int i)
    {
        if (i == GetNumPoints() - 1) return new Edge(points.Last(), points.First());
        else                         return new Edge(points[i], points[i + 1]);
    }

    public IEnumerable<Edge> OwnEdgesOnly()
    {
        for (int i = 0; i < points.Count; ++i) yield return GetEdge(i);
    }

    public IEnumerable<Edge> AllEdges()
    {
        foreach (Edge e in OwnEdgesOnly()) yield return e;

        for (int i = 0; i < holes.Count; ++i)
        {
            SimpleClosedPath hole = GetHole(i);

            foreach (Edge e in hole.OwnEdgesOnly()) yield return e;
        }
    }

	public int GetNumPoints() { return points.Count; }
	public int GetNumEdges()  { return points.Count; }

	public Rect GetBoundingBox()
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (Vector2 p in points)
        {
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
        }

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }

    public void ToggleDirection()
    {
        points.Reverse();

        isCounterClockwise = !isCounterClockwise;
    }

	public bool IsCounterClockwise()
    {
        if (alreadyCalled) return isCounterClockwise;

        alreadyCalled = true;

        double area = 0.0;

        for (int i = 0; i < points.Count - 1; ++i)
        {
            area += points[i].x * points[i+1].y - points[i+1].x * points[i].y;
        }

        area += points[points.Count - 1].x * points[0].y - points[0].x * points[points.Count - 1].y;

        return isCounterClockwise = area >= 0.0;
    }

	public bool IsClockwise()          { return !IsCounterClockwise();                }
	public void SetClockwise()         { if (IsCounterClockwise()) ToggleDirection(); }
	public void SetCounterClockwise()  { if (IsClockwise()) ToggleDirection();        }

	public void Add(Vector2 point)     { points.Add(point);           }
	public void Remove(int pointIndex) { points.RemoveAt(pointIndex); }

	public void Clear()
    {
        points.Clear();
        holes.Clear();
    }

    public override string ToString()
    {
        string result = isHole ? "Hole\t: " : "Exterior\t: ";

        foreach (Vector2 p in points)
        {
            result += p.ToString("F2") + "; ";
        }

        return result;
    }

    public IEnumerable<SimpleClosedPath> Holes()
    {
        for (int i = 0; i < GetNumHoles(); ++i) yield return GetHole(i);
    }

	public void AddHole(int i)  { holes.Add(i);       }
	public int GetNumHoles()    { return holes.Count; }
	//public int GetHole(int i)   { return holes[i];    }
    public SimpleClosedPath GetHole(int i)
    {
        return owner.GetPath(holes[i]);
    }
	public bool IsHole()        { return isHole;      }
	public void SetHole(bool h) { isHole = h;         }
};

public class Polygon
{
    List<SimpleClosedPath> paths = new List<SimpleClosedPath>();
    List<int> exteriors = new List<int>();

    public Polygon()
    {
        
    }

    public Polygon(Polygon p)
    {
        foreach (SimpleClosedPath c in p.paths) paths.Add(new SimpleClosedPath(c, this));
    }

    public SimpleClosedPath GetExterior(int i)
    {
        return paths[exteriors[i]];
    }

    public int GetNumExteriors()
    {
        return exteriors.Count;
    }

    public IEnumerable<SimpleClosedPath> Exteriors()
    {
        for (int i = 0; i < GetNumExteriors(); ++i)
        {
            yield return GetExterior(i);
        }
    }

    public override string ToString()
    {
        string result = "";

        foreach (SimpleClosedPath c in paths)
        {
            result += c.ToString() + "\n";
        }

        return result;
    }

	public SimpleClosedPath GetPath(int i)
    {
        return paths[i];
    }

	public int GetNumPaths()
    {
        return paths.Count;
    }

	public int GetNumPoints()
    {
        int result = 0;

        for (int i = 0; i < paths.Count; ++i) result += paths[i].GetNumPoints();

        return result;
    }

	public Rect GetBoundingBox()
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        for (int i = 0; i < paths.Count; ++i)
        {
            Rect bbox = paths[i].GetBoundingBox();

            if (bbox.min.x < minX) minX = bbox.min.x;
            if (bbox.max.x > maxX) maxX = bbox.max.x;
            if (bbox.min.y < minY) minY = bbox.min.y;
            if (bbox.max.y > maxY) maxY = bbox.max.y;
        }

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }

    public void Split(Edge e)
    {
        
    }

	public void Add(SimpleClosedPath c)
    {
        paths.Add(c);
    }

	public void RemoveLast()
    {
        paths.RemoveAt(paths.Count - 1);
    }

	public void Clear()
    {
        paths.Clear();
    }

    List<SweepEvent> CreateHoleEvents()
    {
        List<SweepEvent> result = new List<SweepEvent>(GetNumPoints() * 2);

        for (int i = 0; i < paths.Count; ++i)
        {
            paths[i].SetCounterClockwise();

            for (int j = 0; j < paths[i].GetNumEdges(); ++j)
            {
                Edge edge = paths[i].GetEdge(j);

                if (edge.From.x == edge.To.x) continue; // vertical edges can be ignored

                SweepEvent from = new SweepEvent(edge.From, true, i, null);
                SweepEvent to   = new SweepEvent(edge.To, true, i, from);

                from.Other = to;

                result.Add(from);
                result.Add(to);

                if (from.Point.x < to.Point.x)
                {
                    to.Left    = false;
                    from.InOut = false;
                }
                else
                {
                    from.Left = false;
                    to.InOut  = true;
                }
            }
        }

        result.Sort(new EndpointComparer());

        return result;
    }

    public void ComputeHoles()
    {
        exteriors.Clear();

        if (GetNumPaths() < 2)
        {
            if (GetNumPaths() == 1 && GetPath(0).IsClockwise())
            {
                GetPath(0).ToggleDirection();
                exteriors.Add(0);
            }

            return;
        }

        List<SweepEvent> events = CreateHoleEvents();

        AVLTree<SweepEvent> sweepLineEvents = new AVLTree<SweepEvent>(new EdgeComparer());

        List<bool> processed = new List<bool>(Enumerable.Repeat(false, GetNumPaths()));
        List<int> holeOf     = new List<int>(Enumerable.Repeat(-1, GetNumPaths()));

        int numProcessedEvents = 0;

        for (int i = 0; i < events.Count && numProcessedEvents < GetNumPaths(); ++i)
        {
            SweepEvent currentEvent = events[i];

            if (currentEvent.Left)
            {
                currentEvent.positionInS = sweepLineEvents.Insert(currentEvent);

                if (!processed[currentEvent.OwningPolygon])
                {
                    processed[currentEvent.OwningPolygon] = true;

                    ++numProcessedEvents;

                    AVLNode<SweepEvent> prevNode = currentEvent.positionInS.GetPredecessor();

                    if (prevNode == null)
                    {
                        GetPath(currentEvent.OwningPolygon).SetCounterClockwise();
                        exteriors.Add(currentEvent.OwningPolygon);
                    }
                    else
                    {
                        SweepEvent prev = prevNode.Value;

                        if (!prev.InOut)
                        {
                            holeOf[currentEvent.OwningPolygon] = prev.OwningPolygon;
                            GetPath(currentEvent.OwningPolygon).SetHole(true);
                            GetPath(prev.OwningPolygon).AddHole(currentEvent.OwningPolygon);

                            if (GetPath(prev.OwningPolygon).IsCounterClockwise()) GetPath(currentEvent.OwningPolygon).SetClockwise();
                            else
                            {
                                GetPath(currentEvent.OwningPolygon).SetCounterClockwise();
                                exteriors.Add(currentEvent.OwningPolygon);
                            }
                        }
                        else if (holeOf[prev.OwningPolygon] != -1)
                        {
                            holeOf[currentEvent.OwningPolygon] = holeOf[prev.OwningPolygon];
                            GetPath(currentEvent.OwningPolygon).SetHole(true);
                            GetPath(holeOf[currentEvent.OwningPolygon]).AddHole(currentEvent.OwningPolygon);

                            if (GetPath(holeOf[currentEvent.OwningPolygon]).IsCounterClockwise()) GetPath(currentEvent.OwningPolygon).SetClockwise();
                            else
                            {
                                GetPath(currentEvent.OwningPolygon).SetCounterClockwise();
                                exteriors.Add(currentEvent.OwningPolygon);
                            }
                        }
                        else
                        {
                            GetPath(currentEvent.OwningPolygon).SetCounterClockwise();
                            exteriors.Add(currentEvent.OwningPolygon);
                        }
                    }
                }
            }
            else // remove the edge from the sweep line status
            {
                // sweepLineEvents.Delete(currentEvent.Other.positionInS);
                sweepLineEvents.Remove(currentEvent.Other);
            }
        }
    }
}

} // of namespace Geometry

} // of namespace BrightBit
