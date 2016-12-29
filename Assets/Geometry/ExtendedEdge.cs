using UnityEngine;

namespace BrightBit
{

namespace Geometry
{

public class ExtendedEdge : Edge
{
    int OwnerOfFrom;
    int OwnerOfTo;

    public ExtendedEdge(Vector2 from, Vector2 to) : base(from, to)
    {
        
    }

    public float Length()
    {
//        return (to - from).sqrMagnitude;
        return 0;
    }
}

} // of namespace Geometry

} // of namespace BrightBit
