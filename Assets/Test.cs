using UnityEngine;
using System.Collections.Generic;

using BrightBit.Collections;
using BrightBit.Geometry;

public class Test : MonoBehaviour
{
    [SerializeField] PolygonRenderer polyRenderer;
    [SerializeField] OperationType operationType = OperationType.DIFFERENCE;

    [SerializeField] Camera cam;
    [SerializeField, Range(0, 50)] int range;
    [SerializeField] bool showOperands = false;
    [SerializeField] bool showResult   = false;
    [SerializeField] bool showGradient = false;

    [SerializeField] List<Vector2> subjectVerts = new List<Vector2>
                                                  {
                                                      new Vector2( 1,  0),
                                                      new Vector2( 0, -1),
                                                      new Vector2(-1,  0),
                                                      new Vector2( 0,  1)
                                                  };

    [SerializeField] List<Vector2> clipperVerts = new List<Vector2>
                                                  {
                                                      new Vector2(-0.05f, -0.05f),
                                                      new Vector2(-0.05f,  0.05f),
                                                      new Vector2( .05f,  0.05f),
                                                      new Vector2( .05f, -0.05f)
                                                  };

    float Orientation(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        return (p1.x - p2.x) * (p0.y - p2.y) - (p1.y - p2.y) * (p0.x - p2.x);
    }

    void Start()
    {
        Triangulator tri = new Triangulator();

        // Vector2 p0 = new Vector2( 0.0f, 0.0f);
        // Vector2 p1 = new Vector2( 0.0f, 1.0f);
        // Vector2 p2 = new Vector2(-0.5f, 0.0f);

        // Debug.Log("Orientation of [" + p0 + p1 + "] [" + p2 + "] : " + Orientation(p0, p1, p2));

        // p0 = new Vector2( 0.0f,  0.0f);
        // p1 = new Vector2( 0.0f,  1.0f);
        // p2 = new Vector2(+4.5f, -1.0f);

        // Debug.Log("Orientation of [" + p0 + p1 + "] [" + p2 + "] : " + Orientation(p0, p1, p2));

        MartinezClipping clippingAlgo = new MartinezClipping();

        Polygon subject = new Polygon();
        Polygon clipper = new Polygon();

        SimpleClosedPath subjectContour = new SimpleClosedPath(subject);
        SimpleClosedPath clipperContour = new SimpleClosedPath(clipper);

        foreach (Vector2 vertex in subjectVerts) subjectContour.Add(vertex);
        foreach (Vector2 vertex in clipperVerts) clipperContour.Add(vertex);

        subject.Add(subjectContour);
        clipper.Add(clipperContour);

        subject.ComputeHoles();
        clipper.ComputeHoles();

        clippingAlgo.subject = subject;
        clippingAlgo.clipper = clipper;

        Polygon result = clippingAlgo.Compute(operationType);

        result.ComputeHoles();

        //result = tri.Simplify(result);

        polyRenderer.AddPolygon(subject, Color.red);
        polyRenderer.AddPolygon(clipper, Color.blue);
        polyRenderer.AddPolygon(result, Color.green);
        polyRenderer.AddEdges(clippingAlgo.DebugLines);
    }

    void Update()
    {
        polyRenderer.ShowResult(showResult);
        polyRenderer.ShowOperands(showOperands);
        polyRenderer.ShowGradients(showGradient);

        if (showResult || showOperands)
        {
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 10.0f;
        }
        else
        {
            cam.nearClipPlane = 2.975f - ((float) range * 0.05f + 0.025f);
            cam.farClipPlane  = 2.975f - ((float) range * 0.05f - 0.025f);
        }
    }
}
