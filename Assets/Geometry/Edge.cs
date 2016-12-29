using UnityEngine;
using System.Collections.Generic;

namespace BrightBit
{

namespace Geometry
{

public enum EdgeType { NORMAL, NON_CONTRIBUTING, SAME_TRANSITION, DIFFERENT_TRANSITION }

public class Edge
{
    const double EPSILON        = 0.00000001;
    const double SQUARE_EPSILON = 0.0000001;

	Vector2 from;
    Vector2 to;

    public Vector2 From
    {
        get { return from;  }
        set { from = value; }
    }

    public Vector2 To
    {
        get { return to;  }
        set { to = value; }
    }

	public Edge(Vector2 from, Vector2 to)
    {
        this.from = from;
        this.to   = to;
    }

	public void ToggleDirection()
    {
        Vector2 tmp = from;
        from        = to;
        to          = tmp;
    }

    public bool HasEqualEndpoint(Edge other)
    {
        return (from == other.from || from == other.to || to == other.from || to == other.to);
    }

    public List<Vector2> GetIntersectionsWith(Edge edge)
    {
        List<Vector2> result = new List<Vector2>(2);

        Vector2 p0 = this.From;
        Vector2 p1 = edge.From;

        Vector2 d0 = new Vector2(this.To.x - p0.x, this.To.y - p0.y);
        Vector2 d1 = new Vector2(edge.To.x - p1.x, edge.To.y - p1.y);

        Vector2 E = new Vector2(p1.x - p0.x, p1.y - p0.y);

        double kross    = d0.x * d1.y - d0.y * d1.x;
        double sqrKross = kross * kross;
        double sqrLen0  = d0.x * d0.x + d0.y * d0.y;
        double sqrLen1  = d1.x * d1.x + d1.y * d1.y;

        if (sqrKross > SQUARE_EPSILON * sqrLen0 * sqrLen1)
        {
            // lines of the edges aren't parallel
            double s = (E.x * d1.y - E.y * d1.x) / kross;

            if ((s < 0) || (s > 1)) return result;

            double t = (E.x * d0.y - E.y * d0.x) / kross;

            if ((t < 0) || (t > 1)) return result;

            // intersection of lines is a point an each edge
            Vector2 pi0 = new Vector2((float)(p0.x + s * d0.x), (float)(p0.y + s * d0.y));

            if (Vector2.Distance(pi0, this.From) < EPSILON) pi0 = this.From;
            if (Vector2.Distance(pi0, this.To)   < EPSILON) pi0 = this.To;
            if (Vector2.Distance(pi0, edge.From) < EPSILON) pi0 = edge.From;
            if (Vector2.Distance(pi0, edge.To)   < EPSILON) pi0 = edge.To;

            result.Add(pi0);

            return result;
        }

        // lines of the edges are parallel
        double sqrLenE = E.x * E.x + E.y * E.y;
        kross          = E.x * d0.y - E.y * d0.x;
        sqrKross       = kross * kross;

        if (sqrKross > SQUARE_EPSILON * sqrLen0 * sqrLenE) return result; // lines of the edge are different

        // Lines of the edges are the same. Need to test for overlap of edges.
        double s0   = (d0.x * E.x + d0.y * E.y) / sqrLen0;  // so = Dot (D0, E) * sqrLen0
        double s1   = s0 + (d0.x * d1.x + d0.y * d1.y) / sqrLen0;  // s1 = s0 + Dot (D0, D1) * sqrLen0
        double smin = Mathf.Min((float)s0, (float)s1);
        double smax = Mathf.Max((float)s0, (float)s1);
        double[] w  = new double[2];
        int imax    = FindIntersection(0.0, 1.0, smin, smax, w);

        if (imax > 0)
        {
            Vector2 pi0 = new Vector2((float)(p0.x + w[0] * d0.x), (float)(p0.y + w[0] * d0.y));

            if (Vector2.Distance(pi0, this.From) < EPSILON) pi0 = this.From;
            if (Vector2.Distance(pi0, this.To)   < EPSILON) pi0 = this.To;
            if (Vector2.Distance(pi0, edge.From) < EPSILON) pi0 = edge.From;
            if (Vector2.Distance(pi0, edge.To)   < EPSILON) pi0 = edge.To;

            result.Add(pi0);

            if (imax > 1)
            {
                result.Add(new Vector2((float)(p0.x + w[1] * d0.x), (float)(p0.y + w[1] * d0.y)));
            }
        }

        return result;
    }

    int FindIntersection(double u0, double u1, double v0, double v1, double[] w)
    {
        if ((u1 < v0) || (u0 > v1)) return 0;

        if (u1 > v0)
        {
            if (u0 < v1)
            {
                w[0] = (u0 < v0) ? v0 : u0;
                w[1] = (u1 > v1) ? v1 : u1;
                return 2;
            }
            else // if (u0 == v1)
            {
                w[0] = u0;
                return 1;
            }
        }
        else // if (u1 == v0)
        {
            w[0] = u1;
            return 1;
        }
    }
}

} // of namespace Geometry

} // of namespace BrightBit
