using UnityEngine;

namespace BrightBit
{

namespace Geometry
{

public class Bridge
{
    public int OwnerFrom  { get; set; }
    public int OwnerTo    { get; set; }

    public int IndexFrom  { get; set; }
    public int IndexTo    { get; set; }

    public float Length   { get; set; }

    public Bridge(int pointIndexFrom, int pointIndexTo, int ownerFrom, int ownerTo, Polygon p)
    {
        Length = (p.GetPath(ownerTo).GetPoint(pointIndexTo) - p.GetPath(ownerFrom).GetPoint(pointIndexFrom)).sqrMagnitude;

        this.OwnerFrom = ownerFrom;
        this.OwnerTo   = ownerTo;

        this.IndexFrom = pointIndexFrom;
        this.IndexTo   = pointIndexTo;
    }
}

} // of namespace Geometry

} // of namespace BrightBit
