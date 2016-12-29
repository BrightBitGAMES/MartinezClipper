using UnityEngine;
using System.Collections.Generic;
using BrightBit.Geometry;
using System.Linq;

public class PolygonRenderer : MonoBehaviour
{
    [SerializeField] Material lineMat;

    List<Polygon> polygons   = new List<Polygon>();
    List<Color> colors       = new List<Color>();

    static readonly List<Color> DebugColors = new List<Color> { Color.red, Color.blue, Color.white, Color.green, Color.yellow, Color.gray };

    List<DebugEdge> edges = new List<DebugEdge>();

    bool showResult    = false;
    bool showOperands  = false;
    bool showGradients = false;

    public void AddPolygon(Polygon p, Color c)
    {
        polygons.Add(p);
        colors.Add(c);
    }

    public void AddEdges(List<DebugEdge> edges)
    {
        this.edges = edges;
    }

    public void Clear()
    {
        polygons.Clear();
        colors.Clear();
    }

    public void ShowResult(bool show)
    {
        showResult = show;
    }

    public void ShowOperands(bool show)
    {
        showOperands = show;
    }

    public void ShowGradients(bool show)
    {
        showGradients = show;
    }

    void DrawPolygons()
    {
        float show = System.Convert.ToSingle(showGradients);

        float z = 3.0f;

        for (int polygonIndex = 0; polygonIndex < polygons.Count; ++polygonIndex)
        {
            if (!showOperands && polygonIndex <= 1) continue;
            if (!showResult && polygonIndex == 2) continue;

            Polygon polygon = polygons[polygonIndex];
            Color polygonColor = colors[polygonIndex];

            int depth = 0;
            float maxDepth = 3.0f;

            for (int pathIndex = 0; pathIndex < polygon.GetNumPaths(); ++pathIndex)
            {
                SimpleClosedPath path = polygon.GetPath(pathIndex);

                maxDepth = path.GetNumEdges() + 1;

                z -= 0.05f;

                //foreach (Edge edge in path.OwnEdgesOnly())
                foreach (Edge edge in path.AllEdges())
                {
                    GL.Begin(GL.LINES);
                    lineMat.SetPass(0);
                    GL.Color(polygonColor * (1.0f - ((float) depth / maxDepth) * show));
                    GL.Vertex3(edge.From.x, edge.From.y, z);
                    GL.Color(polygonColor * (1.0f - ((float) ++depth / maxDepth) * show));
                    GL.Vertex3(edge.To.x,   edge.To.y,   z);
                    GL.End();
                }
            }
        }

        if (showResult || showOperands) return;

        z = 3.0f;

        for (int i = 0; i < edges.Count; ++i)
        {
            Edge edge = edges[i].Edge;

            Color color = !edges[i].InOut ? Color.white : Color.green;

            if (edges[i].Type == EdgeType.NON_CONTRIBUTING)     color = Color.red;
            else if (edges[i].Type == EdgeType.SAME_TRANSITION) color = Color.blue;

            if (edges[i].Final) color = edges[i].Sign;

            Vector2 middle = Vector2.Lerp(edge.From, edge.To, 0.75f);

            z -= 0.05f;

            GL.Begin(GL.LINES);
            lineMat.SetPass(0);
            GL.Color(color);
            GL.Vertex3(edge.From.x, edge.From.y, z);
            GL.Vertex3(middle.x, middle.y, z);
            GL.Vertex3(middle.x, middle.y, z);
            GL.Color(edges[i].Final ? color : color * 0.25f);
            GL.Vertex3(edge.To.x,   edge.To.y,   z);
            GL.End();
        }
    }

    void OnPostRender() { DrawPolygons(); }
    void OnDrawGizmos() { DrawPolygons(); }
}
