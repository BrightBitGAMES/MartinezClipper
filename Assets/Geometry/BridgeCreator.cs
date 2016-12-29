using UnityEngine;
using System.Collections.Generic;
using BrightBit.Collections;

namespace BrightBit
{

namespace Geometry
{

public class BridgeCreator
{
    MinHeap<ExtendedEdge> possibleBridges = new MinHeap<ExtendedEdge>(new EdgeLengthComparer());

    public Polygon Simplify(Polygon polygon)
    {
        Polygon result = new Polygon();

        foreach (SimpleClosedPath exterior in polygon.Exteriors())
        {
            result.Add(Simplify(exterior, result));
        }

        return result;
    }

    SimpleClosedPath Simplify(SimpleClosedPath exterior, Polygon newPolygon)
    {
        SimpleClosedPath result = new SimpleClosedPath(newPolygon);

        foreach (SimpleClosedPath hole in exterior.Holes())
        {
            UpdatePossibleBridges(exterior, hole);

            while (!possibleBridges.IsEmpty())
            {
                ExtendedEdge bridge = possibleBridges.ExtractFirst();

                if (IsBridgeValid(exterior, bridge))
                {
                    //result.JoinByBridge(bridge);
                }
            }
        }

        return result;
    }

    bool IsBridgeValid(SimpleClosedPath exterior, ExtendedEdge bridge)
    {
        foreach (Edge e in exterior.AllEdges())
        {
            List<Vector2> intersections = bridge.GetIntersectionsWith(e);

            if (intersections.Count > 0 && !e.HasEqualEndpoint(bridge)) return false;
        }

        return true;
    }

    void UpdatePossibleBridges(SimpleClosedPath exterior, SimpleClosedPath mainHole)
    {
        possibleBridges.Clear();

        foreach (Vector2 from in mainHole.Points())
        {
            foreach (SimpleClosedPath hole in exterior.Holes())
            {
                if (hole == mainHole) continue;

                foreach (Vector2 to in hole.Points())
                    possibleBridges.Add(new ExtendedEdge(from, to ));
            }

            foreach (Vector2 to in exterior.Points())
                possibleBridges.Add(new ExtendedEdge(from, to ));
        }

        // possibleBridges.Sort(bridgeComparer);
    }
}

} // of namespace Geometry

} // of namespace BrightBit
