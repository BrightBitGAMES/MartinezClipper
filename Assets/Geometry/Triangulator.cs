using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrightBit
{

namespace Geometry
{

public class Triangulator
{
    Connector connector = new Connector();

    bool IsJoiningHole(SimpleClosedPath path, Edge joiningEdge)
    {
        foreach (Edge e in path.AllEdges())
        {
            List<Vector2> intersections = joiningEdge.GetIntersectionsWith(e);

            if (intersections.Count > 0 && !e.HasEqualEndpoint(joiningEdge)) return false;
        }

        return true;
    }

    /// <summary>
    /// Converts a polygon with holes to a polygon without holes
    /// </summary>

    public Polygon Simplify(Polygon inputPolygon)
    {
        Polygon result = new Polygon();

        foreach (SimpleClosedPath exterior in inputPolygon.Exteriors())
        {
            Vector2 firstPointOnExterior = exterior.GetPoint(0);

            SimpleClosedPath newExterior = new SimpleClosedPath(exterior, result);

            foreach (SimpleClosedPath hole in exterior.Holes())
            {
                float minDistance          = hole.Points().Min(p => Vector2.Distance(firstPointOnExterior, p));
                Vector2 closestPointOnHole = hole.Points().Where(p => Vector2.Distance(firstPointOnExterior, p) == minDistance).FirstOrDefault();

                Edge edge = new Edge(firstPointOnExterior, closestPointOnHole);

                if (IsJoiningHole(exterior, edge))
                {
                    newExterior = Join(exterior, hole, edge);
                }
                else // find correct pair : O(n³) where n equals max(points of exterior, points of interior)
                {
                    foreach (Vector2 outside in exterior.Points())
                    {
                        foreach (Vector2 inside in hole.Points())
                        {
                            edge = new Edge(outside, inside);

                            if (IsJoiningHole(exterior, edge)) // O(n)
                            {
                                newExterior = Join(newExterior, hole, edge);
                            }
                        }
                    }
                }

            } // foreach hole

            result.Add(newExterior);

        } // foreach exterior

        return result;
        //throw new InvalidOperationException("Can't find edges to join exteriors and interiors!");
    }

    SimpleClosedPath Join(SimpleClosedPath exterior, SimpleClosedPath hole, Edge edge)
    {
        connector.Clear();

        connector.Add(new Edge(edge.To, edge.From));

        foreach (Edge e in exterior.OwnEdgesOnly()) connector.Add(e);

        foreach (Edge e in hole.OwnEdgesOnly()) connector.Add(e);

        connector.Add(edge);

        return connector.CreatePolygon().GetPath(0);
    }

    // public int[] Triangulate(Polygon p)
    // {
        // Polygon shell = Simplify(p);

        // foreach (SimpleClosedPath path in shell)
        // {
            
        // }
    // }

    List<int> TriangulateSimpleClosedPath(SimpleClosedPath path)
    {
        List<Vector2> points = new List<Vector2>();
        List<int> indices    = new List<int>();

        int n = points.Count;

        if (n < 3) return indices;

        int[] V = new int[n];

        // path needs to be counter clockwise

        int nv = n;
        int count = 2 * nv;

        for (int m = 0, v = nv - 1; nv > 2;)
        {
            if (count-- <= 0) return indices;

            int u = v;
            if (nv <= u) u = 0;

            v = u + 1;
            if (nv <= v) v = 0;

            int w = v + 1;
            if (nv <= w) w = 0;

            if (Snip(u, v, w, nv, V, points))
            {
                int a, b, c, s, t;
                a = V[u];
                b = V[v];
                c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                ++m;

                for (s = v, t = v + 1; t < nv; ++s, ++t)
                    V[s] = V[t];

                --nv;
                count = 2 * nv;
            }
        }

        return indices;
    }

    bool Snip(int u, int v, int w, int n, int[] V, List<Vector2> points)
    {
        int p;
        Vector2 A = points[V[u]];
        Vector2 B = points[V[v]];
        Vector2 C = points[V[w]];

        if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x)))) return false;

        for (p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w)) continue;

            Vector2 P = points[V[p]];

            if (InsideTriangle(A, B, C, P)) return false;
        }

        return true;
    }

    bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;

        ax = C.x - B.x; ay = C.y - B.y;
        bx = A.x - C.x; by = A.y - C.y;
        cx = B.x - A.x; cy = B.y - A.y;
        apx = P.x - A.x; apy = P.y - A.y;
        bpx = P.x - B.x; bpy = P.y - B.y;
        cpx = P.x - C.x; cpy = P.y - C.y;

        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;

        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }
}

} // of namespace Geometry

} // of namespace BrightBit
