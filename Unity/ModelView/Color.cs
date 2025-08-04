using System.Numerics;

namespace ET;

public readonly struct Color
{
    public readonly float r, g, b, a;

    public Color()
    {
        r = g = b = 0;
        a = 1;
    }

    public Color(float rgb, float a = 1)
    {
        r = g = b = rgb;
        this.a = a;
    }

    public Color(float r, float g, float b, float a = 1)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }
    
    public static Color Gray => new Color(0.2f, 0.2f, 0.2f);
    public static Color Black => new Color(0.0f, 0.0f, 0.0f);
    public static Color White => new Color(1.0f, 1.0f, 1.0f);
    public static Color Red => new Color(1.0f, 0.0f, 0.0f);
    public static Color Green => new Color(0.0f, 1.0f, 0.0f);
    public static Color Blue => new Color(0.0f, 0.0f, 1.0f);
    public static Color Yellow => new Color(1.0f, 1.0f, 0.0f);
    
    public static implicit operator Vector4(Color color)
    {
        return new Vector4(color.r, color.g, color.b, color.a);
    }
    
    public static implicit operator Vector3(Color color)
    {
        return new Vector3(color.r, color.g, color.b);
    }

    public static implicit operator Color(Vector4 color)
    {
        return new Color(color.X, color.Y, color.Z, color.W);
    }

    public static implicit operator Color(Vector3 color)
    {
        return new Color(color.X, color.Y, color.Z);
    }
}