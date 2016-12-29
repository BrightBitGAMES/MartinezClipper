//////////////////////////////////////////////////////////////////
//                                                              //
//  This file is part of BrightBit's Martinez Clipper package.  //
//                                                              //
//  Copyright (c) 2016 by BrightBit                             //
//                                                              //
//  This software may be modified and distributed under         //
//  the terms of the MIT license. See the LICENSE file          //
//  for details.                                                //
//                                                              //
//////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using BrightBit.Collections;

namespace BrightBit
{

namespace Geometry
{

public enum OperationType { INTERSECTION, UNION, DIFFERENCE, XOR }

/// <summary>
/// 
/// </summary>
public class MartinezClipping
{
    enum Operand { SUBJECT, CLIPPER }

    List<DebugEdge> debugEdges = new List<DebugEdge>();

    public List<DebugEdge> DebugLines { get { return debugEdges; } }

	public Polygon subject;
	public Polygon clipper;

    // Context for the clipping algorithm

    Connector connector                       = new Connector();                                 // to connect the edge solutions
    AVLTree<SweepEvent> sweepLineEvents       = new AVLTree<SweepEvent>(new EdgeComparer());     // all edges that are intersecting the sweep line
    MinHeap<SweepEvent> futureSweepLineEvents = new MinHeap<SweepEvent>(new EndpointComparer()); // all edges right of the sweep line

    AVLNode<SweepEvent> prevPrev;
    AVLNode<SweepEvent> left;
    AVLNode<SweepEvent> prev;
    AVLNode<SweepEvent> next;

    EndpointComparer comparer = new EndpointComparer();

    float minMaxX;
    Rect bbSubject;
    Rect bbClipper;

	/// <summary>
    ///
    /// </summary>
	void ProcessEdge(Edge edge, Operand o)
    {
        int owningPolygon = (int) o;

        if (edge.From == edge.To) return;

        SweepEvent from = new SweepEvent(edge.From, true, owningPolygon, null);
        SweepEvent to   = new SweepEvent(edge.To, true, owningPolygon, from);

        from.Other = to;

        if (from.Point.x < to.Point.x)      to.Left   = false;
        else if (from.Point.x > to.Point.x) from.Left = false;
        else if (from.Point.y < to.Point.y) to.Left   = false; // the line segment is vertical. The bottom endpoint is the left endpoint
        else                                from.Left = false;

        futureSweepLineEvents.Add(from);
        futureSweepLineEvents.Add(to);
    }

    /// <summary>
    /// Process a possible intersection between the segment associated to the left events e1 and e2
    /// </summary>
	void PossibleIntersection(SweepEvent e1, SweepEvent e2)
    {
        Edge edge1 = e1.CreateEdge();
        Edge edge2 = e2.CreateEdge();

        List<Vector2> intersections = edge1.GetIntersectionsWith(edge2);

        if (intersections.Count == 0)                                  return;
        if (intersections.Count == 1 && e1.HasEqualEndpoint(e2))       return;
        if (intersections.Count == 2 && e1.BelongsToSamePolygonAs(e2)) return;

        if (intersections.Count == 1)
        {
            Vector2 ip = intersections[0];

            if (!e1.IsAnEndpoint(ip)) DivideEdge(e1, ip);
            if (!e2.IsAnEndpoint(ip)) DivideEdge(e2, ip);

            return;
        }

        List<SweepEvent> sortedEvents = new List<SweepEvent>(); // the line segments overlap

        if (e1.Point == e2.Point)
        {
            sortedEvents.Add(null);
        }
        else if (comparer.Compare(e1, e2) > 0)
        {
            sortedEvents.Add(e2);
            sortedEvents.Add(e1);
        }
        else
        {
            sortedEvents.Add(e1);
            sortedEvents.Add(e2);
        }

        if (e1.Other.Point == e2.Other.Point)
        {
            sortedEvents.Add(null);
        }
        else if (comparer.Compare(e1.Other, e2.Other) > 0)
        {
            sortedEvents.Add(e2.Other);
            sortedEvents.Add(e1.Other);
        }
        else
        {
            sortedEvents.Add(e1.Other);
            sortedEvents.Add(e2.Other);
        }

        //////////////////////////////////////
        // Degeneracies : Overlapping Edges //
        //////////////////////////////////////

        // 1. Case: overlapping edges that are equal

        if (sortedEvents.Count == 2)
        {
            e1.SetEdgeType(EdgeType.NON_CONTRIBUTING);

            if (e1.InOut == e2.InOut) e2.SetEdgeType(EdgeType.SAME_TRANSITION);
            else                      e2.SetEdgeType(EdgeType.DIFFERENT_TRANSITION);

            return;
        }

        // 2. Case: overlapping edges that share an endpoint

        if (sortedEvents.Count == 3)
        {
            sortedEvents[1].SetEdgeType(EdgeType.NON_CONTRIBUTING);

            int sharedIndex = (sortedEvents[0] != null) ? 0 : 2; // is left endpoint or right endpoint shared

            if (e1.InOut == e2.InOut) sortedEvents[sharedIndex].Other.Type = EdgeType.SAME_TRANSITION;
            else                      sortedEvents[sharedIndex].Other.Type = EdgeType.DIFFERENT_TRANSITION;

            if (sortedEvents[0] != null) DivideEdge(sortedEvents[0],       sortedEvents[1].Point);
            else                         DivideEdge(sortedEvents[2].Other, sortedEvents[1].Point);

            return;
        }

        // 3. Case: overlapping edges where noone totally includes the other

        if (sortedEvents[0] != sortedEvents[3].Other)
        {
            sortedEvents[1].Type = EdgeType.NON_CONTRIBUTING;
            sortedEvents[2].Type = (e1.InOut == e2.InOut) ? EdgeType.SAME_TRANSITION : EdgeType.DIFFERENT_TRANSITION;

            DivideEdge(sortedEvents[0], sortedEvents[1].Point);
            DivideEdge(sortedEvents[1], sortedEvents[2].Point);

            return;
        }

        // one line segment includes the other one
        sortedEvents[1].Type = sortedEvents[1].Other.Type = EdgeType.NON_CONTRIBUTING;
        DivideEdge(sortedEvents[0], sortedEvents[1].Point);

        sortedEvents[3].Other.Type = (e1.InOut == e2.InOut) ? EdgeType.SAME_TRANSITION : EdgeType.DIFFERENT_TRANSITION;
        DivideEdge(sortedEvents[3].Other, sortedEvents[2].Point);
    }

	// /** @brief Divide the segment associated to left event e, updating pq and (implicitly) the status line */
	void DivideEdge(SweepEvent origin, Vector2 p)
    {
        // "Left event" of the "right line segment" resulting from dividing e (the line segment associated to e)
        SweepEvent rightEvent = new SweepEvent(p, false, origin.OwningPolygon, origin, origin.Type);
        SweepEvent leftEvent  = new SweepEvent(p, true, origin.OwningPolygon, origin.Other, origin.Other.Type);

        if (comparer.Compare(leftEvent, origin.Other) > 0) // avoid a rounding error. The left event would be processed after the right event
        {
            origin.Other.Left = true;
            leftEvent.Left    = false;
        }

        origin.Other.Other = leftEvent;
        origin.Other       = rightEvent;

        futureSweepLineEvents.Add(leftEvent);
        futureSweepLineEvents.Add(rightEvent);
    }

    void OnAddEdge(Edge edge)
    {
        debugEdges.Add(new DebugEdge { Edge = edge, Final = true, Sign = Color.yellow } );
    }

    public Polygon Compute(OperationType ot)
    {
        connector.OnAddEdgeEvent += OnAddEdge;

        if (subject == null)      return clipper;
        else if (clipper == null) return subject;

        Polygon result = new Polygon();

        futureSweepLineEvents.Clear();

        // Test 1 for trivial result case
        if (subject.GetNumPaths () * clipper.GetNumPaths () == 0)
        {
            // At least one of the polygons is empty
            if (ot == OperationType.DIFFERENCE) result = subject;
            if (ot == OperationType.UNION)      result = (subject.GetNumPaths () == 0) ? clipper : subject;

            return result;
        }

        // Test 2 for trivial result case
        bbSubject = subject.GetBoundingBox();
        bbClipper = clipper.GetBoundingBox();

        if (bbSubject.min.x > bbClipper.max.x ||
            bbSubject.min.y > bbClipper.max.y ||
            bbClipper.min.x > bbSubject.max.x ||
            bbClipper.min.y > bbSubject.max.y)
        {
            // the bounding boxes do not overlap
            if (ot == OperationType.DIFFERENCE) result = subject;
            if (ot == OperationType.UNION)
            {
                result = subject;

                for (int i = 0; i < clipper.GetNumPaths(); ++i) result.Add(clipper.GetPath(i));
            }

            return result;
        }

        // Insert all the endpoints associated to the line segments into the event queue

        for (int currentPath = 0; currentPath < subject.GetNumPaths(); ++currentPath)
            for (int currentEdge = 0; currentEdge < subject.GetPath(currentPath).GetNumPoints(); ++currentEdge)
                ProcessEdge(subject.GetPath(currentPath).GetEdge(currentEdge), Operand.SUBJECT);

        for (int currentPath = 0; currentPath < clipper.GetNumPaths(); ++currentPath)
            for (int currentEdge = 0; currentEdge < clipper.GetPath(currentPath).GetNumPoints(); ++currentEdge)
                ProcessEdge(clipper.GetPath(currentPath).GetEdge(currentEdge), Operand.CLIPPER);

        minMaxX = Mathf.Min(bbSubject.max.x, bbClipper.max.x);

        while (!futureSweepLineEvents.IsEmpty())
        {
            SweepEvent currentEvent = futureSweepLineEvents.ExtractFirst();

            currentEvent.Number = ++seCounter;

            if (AlreadyFinished(ref result, ot, currentEvent))
            {
                return result;
            }

            if (currentEvent.Left) HandleLeftEvent(currentEvent);
            else                   HandleRightEvent(ot, currentEvent);
        }

        return connector.CreatePolygon();
    }

    static int seCounter = 0;

    bool AlreadyFinished(ref Polygon polygon, OperationType ot, SweepEvent currentEvent)
    {
        if ((ot == OperationType.INTERSECTION && currentEvent.Point.x > minMaxX) ||
            (ot == OperationType.DIFFERENCE   && currentEvent.Point.x > bbSubject.max.x))
        {
            polygon = connector.CreatePolygon();

            return true;
        }

        if (ot == OperationType.UNION && currentEvent.Point.x > minMaxX)
        {
            // add all the non-processed line segments to the result
            if (!currentEvent.Left) connector.Add(currentEvent.CreateEdge());

            while (!futureSweepLineEvents.IsEmpty())
            {
                currentEvent = futureSweepLineEvents.ExtractFirst();

                if (!currentEvent.Left) connector.Add(currentEvent.CreateEdge());
            }

            polygon = connector.CreatePolygon();

            return true;
        }

        return false;
    }

    void HandleLeftEvent(SweepEvent currentEvent)
    {
        AVLNode<SweepEvent> position = sweepLineEvents.Insert(currentEvent); // the line segment must be inserted into S

        currentEvent.positionInS = position;

        prev = position.GetPredecessor();
        next = position.GetSuccessor();

        // Compute the inside and inOut flags
        if (prev == null)
        {
            currentEvent.Inside = false;
            currentEvent.InOut  = false;
        }
        else if (prev.Value.Type != EdgeType.NORMAL)
        {
            if (prev.Value == sweepLineEvents.GetMin()) // currentEvent overlaps with prev // JUST GET! NOT POP!
            {
                currentEvent.Inside = true; // it is not relevant to set true or false
                currentEvent.InOut  = false;
            }
            else // the previous two line segments in S are overlapping line segments
            {
                prevPrev = prev.GetPredecessor();

                if (prev.Value.OwningPolygon == currentEvent.OwningPolygon)
                {
                    currentEvent.InOut  = !prev.Value.InOut;
                    currentEvent.Inside = !prevPrev.Value.InOut;
                }
                else
                {
                    currentEvent.InOut  = !prevPrev.Value.InOut;
                    currentEvent.Inside = !prev.Value.InOut;
                }
            }
        }
        else if (currentEvent.OwningPolygon == prev.Value.OwningPolygon) // edges of same polygon
        {
            currentEvent.Inside = prev.Value.Inside;
            currentEvent.InOut  = !prev.Value.InOut;
        }
        else // edges of different polygons
        {
            currentEvent.Inside = !prev.Value.InOut;
            currentEvent.InOut  = prev.Value.Inside;
        }

        AddDebugEdge(currentEvent);

        if (next != null) PossibleIntersection(currentEvent, next.Value);
        if (prev != null) PossibleIntersection(prev.Value, currentEvent);
    }

    static int counter = 0;
    static List<Color> colors = new List<Color> { Color.red, Color.green, Color.yellow, Color.blue };

    void HandleRightEvent(OperationType ot, SweepEvent currentEvent)
    {
        // the left end/part of the current SweepEvent must be removed from sweepLineEvents
        left = currentEvent.Other.positionInS;
        next = left.GetSuccessor();
        prev = left.GetPredecessor();

        // Check if the line segment belongs to the Boolean operation
        switch (currentEvent.Type)
        {
            case EdgeType.NORMAL:
                switch (ot)
                {
                    case OperationType.INTERSECTION : if (currentEvent.Other.Inside) connector.Add(currentEvent.CreateEdge());  break;
                    case OperationType.UNION        : if (!currentEvent.Other.Inside) connector.Add(currentEvent.CreateEdge()); break;
                    case OperationType.DIFFERENCE   : if ((currentEvent.OwningPolygon == (int)Operand.SUBJECT && !currentEvent.Other.Inside) ||
                                                          (currentEvent.OwningPolygon == (int)Operand.CLIPPER && currentEvent.Other.Inside))
                                                      connector.Add(currentEvent.CreateEdge());                                 break;
                    case OperationType.XOR          : connector.Add(currentEvent.CreateEdge());                                 break;
                }
                break;
            case EdgeType.SAME_TRANSITION      : if (ot == OperationType.INTERSECTION || ot == OperationType.UNION) connector.Add(currentEvent.CreateEdge()); break;
            case EdgeType.DIFFERENT_TRANSITION : if (ot == OperationType.DIFFERENCE) connector.Add(currentEvent.CreateEdge());  break;
        }

        AddDebugEdge(currentEvent);

        sweepLineEvents.RemoveAt(left);

        if (next != null && prev != null) PossibleIntersection(prev.Value, next.Value);
    }

    void AddDebugEdge(SweepEvent currentEvent)
    {
        debugEdges.Add(new DebugEdge { Edge = currentEvent.CreateEdge(), InOut = currentEvent.InOut, Left = currentEvent.Left, Type = currentEvent.Type } );
    }
}

} // of namespace Geometry

} // of namespace BrightBit
