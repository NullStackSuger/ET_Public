using System.Numerics;

namespace ET.Client;

public struct Plane
{
    public Vector3 Normal { get; private set; }
    public float Distance { get; private set; }

    public Plane(Vector3 normal, float distance)
    {
        Normal = normal;
        Distance = distance;
    }
    
    public Plane(float a, float b, float c, float d)
    {
        Normal = new Vector3(a, b, c);
        Distance = d;
    }
    
    public void Normalize()
    {
        float length = Normal.Length();
        if (length > float.Epsilon)
        {
            Normal /= length;
            Distance /= length;
        }
    }
    
    // >0表示和法线同侧
    public float GetDistance(Vector3 point)
    {
        return Vector3.Dot(Normal, point) + Distance;
    }
    
    public bool IsOutside(Vector3 point)
    {
        return GetDistance(point) < 0;
    }

    public override string ToString()
    {
        return $"(Normal: {Normal}, Distance: {Distance})";
    }
}