using System.Collections.Generic;

namespace BrightBit
{

namespace Geometry
{

public class EndpointComparer : IComparer<SweepEvent>
{
    public int Compare(SweepEvent e1, SweepEvent e2)
    {
        if      (e1.Point.x < e2.Point.x) return -1;
        else if (e1.Point.x > e2.Point.x) return +1;
        else if (e1.Point.y < e2.Point.y) return -1;
        else if (e1.Point.y > e2.Point.y) return +1;
        else if (e1.Left != e2.Left)      return !e1.Left ? -1 : 1;

        return e1.IsLeftSide(e2.Other.Point) ? -1 : 1;
    }
}

public class EdgeComparer : IComparer<SweepEvent>
{
    static EndpointComparer comparer = new EndpointComparer();

    public int Compare(SweepEvent e1, SweepEvent e2)
    {
        if (e1 == e2) return 0;

        if (!e1.IsCollinearTo(e2))
        {
            if (e1.Point == e2.Point) return e1.IsLeftSide(e2.Other.Point) ? -1 : 1;

            if (comparer.Compare(e1, e2) < 0) return e1.IsLeftSide(e2.Point) ? -1 : 1;

            return !e2.IsLeftSide(e1.Point) ? -1 : 1;
        }

        return comparer.Compare(e1, e2);
    }
}

public class BridgeComparer : IComparer<Bridge>
{
    public int Compare(Bridge b1, Bridge b2)
    {
        if (b1 == b2)               return 0;
        if (b1.Length == b2.Length) return 0;

        return b1.Length < b2.Length ? -1 : +1;
    }
}

public class EdgeLengthComparer : IComparer<ExtendedEdge>
{
    public int Compare(ExtendedEdge e1, ExtendedEdge e2)
    {
        if (e1 == e2)                   return 0;
        if (e1.Length() == e2.Length()) return 0;

        return e1.Length() < e2.Length() ? -1 : +1;
    }
}

} // of namespace Geometry

} // of namespace BrightBit
