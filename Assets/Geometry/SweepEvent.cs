using UnityEngine;
using UnityEngine.Assertions;
using System;
using BrightBit.Collections;

namespace BrightBit
{

namespace Geometry
{

public class SweepEvent
{
    Vector2 point;
    Vector2 uv;

    int owningPolygon;      // depends on context (e.g.: 0 for subject, 1 for clipper)

    bool left;              // Is "point" the left endpoint of the edge from "point" to "other.point"
    bool inside;            // Is the edge inside of another polygon
    bool inOut;             // Does the edge represent an inside-outside transition in the polygon for
                            // a vertical ray from (p.x, -infinite) that crosses the edge

    public int Number;

    public override string ToString() { return Number.ToString(); }

    EdgeType   type;
    SweepEvent other;       // Event associated to the other endpoint of the segment

    public AVLNode<SweepEvent> positionInS;

    public int OwningPolygon    { get { return owningPolygon; } }
    public Vector2 Point        { get { return point;         } }

    public EdgeType Type
    {
        get { return type;  }
        set { type = value; }
    }

    public bool Inside
    {
        get { return inside;  }
        set { inside = value; }
    }

    public bool InOut
    {
        get { return inOut;  }
        set { inOut = value; }
    }

    public SweepEvent Other
    {
        get { return other;  }
        set { other = value; }
    }

    public bool Left
    {
        get { return left;  }
        set { left = value; }
    }

    public SweepEvent(Vector2 p, bool left, int owningPolygon, SweepEvent other, EdgeType t = EdgeType.NORMAL)
    {
        this.point         = p;
        this.left          = left;
        this.owningPolygon = owningPolygon;
        this.other         = other;
        this.type          = t;
        this.positionInS   = null;
    }

    public void SetEdgeType(EdgeType et)
    {
        this.type  = et;
        other.type = et;
    }

    public Edge CreateEdge()
    {
        return new Edge(point, other.point);
    }

    public bool BelongsToSamePolygonAs(SweepEvent o)
    {
        return o.owningPolygon == this.owningPolygon;
    }

    public bool HasEqualEndpoint(SweepEvent other) // will only be called for "left" events, i.e. "this" and "other" will have the "left" attribute set to TRUE!
    {
        Assert.IsTrue(this.left);
        Assert.IsTrue(other.left);

        return (this.Point == other.Point) || (this.Other.Point == other.Other.Point);
    }

    public bool IsAnEndpoint(Vector2 p)
    {
        return Point == p || Other.Point == p;
    }

    /// <summary>
    /// Return value < 0 means p2 is to the left of the line defined by p0 to p1
    /// Return value > 0 means p2 is to the right of the line defined by p0 to p1
    /// Return value == 0 means p2 is on the line defined by p0 to p1
    /// </summary>
    float Orientation(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        return (p1.x - p2.x) * (p0.y - p2.y) - (p1.y - p2.y) * (p0.x - p2.x);
    }

    /// <summary>
    /// Is "p" on the left side of the edge defined by "point" to "other.point"?
    /// </summary>
    public bool IsLeftSide(Vector2 p)
    {
        if (left) return Orientation(point, other.point, p) < 0;
        else      return Orientation(other.point, point, p) < 0;
    }

    public bool IsCollinearTo(SweepEvent se)
    {
        return Orientation(Point, Other.Point, se.Point)       == 0 &&
               Orientation(Point, Other.Point, se.Other.Point) == 0;
    }
};

} // of namespace Geometry

} // of namespace BrightBit
