using System.Numerics;

namespace ET.Client;

public struct AABB
{
    public AABB(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }

    public AABB()
    {
        Min = float.MaxValue * Vector3.One;
        Max = float.MinValue * Vector3.One;
    }

    public Vector3 Min;
    public Vector3 Max;
    
    public Vector3 Center => (Min + Max) * 0.5f;
    public Vector3 Size => Max - Min;
    public float SurfaceArea => (Size.X * Size.Y + Size.X * Size.Z + Size.Y * Size.Z) * 2.0f;

    public IEnumerable<Vector3> GetCorners()
    { 
        yield return new Vector3(Min.X, Min.Y, Min.Z); 
        yield return new Vector3(Min.X, Min.Y, Max.Z);
        yield return new Vector3(Min.X, Max.Y, Min.Z);
        yield return new Vector3(Min.X, Max.Y, Max.Z);
        yield return new Vector3(Max.X, Min.Y, Min.Z);
        yield return new Vector3(Max.X, Min.Y, Max.Z);
        yield return new Vector3(Max.X, Max.Y, Min.Z);
        yield return new Vector3(Max.X, Max.Y, Max.Z);
    }

    public AABB Transform(Matrix4x4 model)
    {
        AABB aabb = new AABB();
        foreach (Vector3 corner in GetCorners())
        {
            Vector3 transformed = Vector3.Transform(corner, model);
            aabb.Min = Vector3.Min(aabb.Min, transformed);
            aabb.Max = Vector3.Max(aabb.Max, transformed);
        }
        return aabb;
    }
    
    // 扩展包围盒以包含另一个点
    public void Encapsulate(Vector3 point)
    {
        Min = Vector3.Min(Min, point);
        Max = Vector3.Max(Max, point);
    }
    // 扩展包围盒以包含另一个 AABB
    public void Encapsulate(AABB other)
    {
        Min = Vector3.Min(Min, other.Min);
        Max = Vector3.Max(Max, other.Max);
    }
    public static AABB Encapsulate(AABB a, AABB b)
    {
        return new AABB(Vector3.Min(a.Min, b.Min), Vector3.Max(a.Max, b.Max));
    }
    
    public static AABB GetAABB(MeshComponent self)
    {
        if (self.meshInfo.indices == null || self.meshInfo.indices.Length == 0 || self.meshInfo.positions == null || self.meshInfo.positions.Length == 0)
        {
            return new AABB();
        }

        AABB aabb =  new AABB();
        
        foreach (ushort i in self.meshInfo.indices)
        {
            Vector3 p = self.meshInfo.positions[i];
            aabb.Encapsulate(p);
        }
        
        return aabb;
    }
    
    // 判断是否与另一个 AABB 相交
    public bool Intersects(AABB other)
    {
        return (Min.X <= other.Max.X && Max.X >= other.Min.X) &&
               (Min.Y <= other.Max.Y && Max.Y >= other.Min.Y) &&
               (Min.Z <= other.Max.Z && Max.Z >= other.Min.Z);
    }
    // 判断是否与 Plane 相交
    public bool Intersects(Plane plane)
    {
        // 计算 AABB 中最靠近平面的点
        Vector3 negative = Max;

        if (plane.Normal.X >= 0) negative.X = Min.X;
        if (plane.Normal.Y >= 0) negative.Y = Min.Y;
        if (plane.Normal.Z >= 0) negative.Z = Min.Z;

        // 如果负顶点在平面外，则整个AABB在平面外
        // 否则，AABB 至少部分在或相交于平面
        return plane.GetDistance(negative) >= 0;
    }
    // 判断某个点是否在包围盒内
    public bool Contains(Vector3 point)
    {
        return (point.X >= Min.X && point.X <= Max.X) &&
               (point.Y >= Min.Y && point.Y <= Max.Y) &&
               (point.Z >= Min.Z && point.Z <= Max.Z);
    }
    
    // 获取最长轴方向
    public Vector3 LongestAxis()
    {
        Vector3 size = Size;
        if (size.X > size.Y && size.X > size.Z) return Vector3.UnitX;
        if (size.Y > size.Z) return Vector3.UnitY;
        return Vector3.UnitZ;
    }

    public override string ToString()
    {
        return $"({Min}, {Max})";
    }
}