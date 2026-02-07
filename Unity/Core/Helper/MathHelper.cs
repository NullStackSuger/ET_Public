using System.Numerics;

namespace ET;

public static class MathHelper
{
    public const float Rad2Deg = 180.0f / MathF.PI;
    public const float Deg2Rad = MathF.PI / 180.0f;
    
    public static Quaternion ToQuaternion(this Vector3 angles)
    {
        Vector3 rad = angles * Deg2Rad;
        Quaternion rotX = Quaternion.CreateFromAxisAngle(Vector3.UnitX, rad.X);
        Quaternion rotY = Quaternion.CreateFromAxisAngle(Vector3.UnitY, rad.Y);
        Quaternion rotZ = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, rad.Z);
        return Quaternion.Normalize(rotY * rotX * rotZ);
    }
    
    public static Vector3 ToVector3(this Quaternion q)
    {
        q = Quaternion.Normalize(q);

        float sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
        float cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        float x = MathF.Atan2(sinr_cosp, cosr_cosp); // Roll

        float sinp = 2 * (q.W * q.Y - q.Z * q.X);
        float y;
        if (MathF.Abs(sinp) >= 1)
            y = MathF.CopySign(MathF.PI / 2, sinp); // Pitch (gimbal lock)
        else
            y = MathF.Asin(sinp);

        float siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
        float cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        float z = MathF.Atan2(siny_cosp, cosy_cosp); // Yaw

        return new Vector3(x, y, z) * Rad2Deg;
    }

    public static Vector4 ToVector4(this Vector3 v)
    {
        return new Vector4(v.X, v.Y, v.Z, 1);
    }
    public static Vector3 ToVector3(this Vector4 v)
    {
        return new Vector3(v.X, v.Y, v.Z);
    }

    public static Quaternion AngleAxis(float angleRad, Vector3 axis)
    {
        axis = Vector3.Normalize(axis);
        float half = angleRad * 0.5f;
        float sin = (float)Math.Sin(half);
        float cos = (float)Math.Cos(half);

        return new Quaternion(
            axis.X * sin,
            axis.Y * sin,
            axis.Z * sin,
            cos
        );
    }
    
    public static Vector3 Rotate(Quaternion q, Vector3 v)
    {
        // v' = q * v * q^-1
        Quaternion vQuat = new Quaternion(v, 0f);
        Quaternion qConj = Quaternion.Conjugate(q);
        Quaternion result = q * vQuat * qConj;
        return new Vector3(result.X, result.Y, result.Z);
    }
    
    public static Vector4 Mul(Matrix4x4 m, in Vector4 v)
    {
        return new Vector4(
            m.M11*v.X + m.M12*v.Y + m.M13*v.Z + m.M14*v.W,
            m.M21*v.X + m.M22*v.Y + m.M23*v.Z + m.M24*v.W,
            m.M31*v.X + m.M32*v.Y + m.M33*v.Z + m.M34*v.W,
            m.M41*v.X + m.M42*v.Y + m.M43*v.Z + m.M44*v.W
        );
    }

    public static bool Equals(Vector3 v1, Vector3 v2)
    {
        return MathF.Abs(v1.X - v2.X) < float.Epsilon && MathF.Abs(v1.Y - v2.Y) < float.Epsilon && MathF.Abs(v1.Z - v2.Z) < float.Epsilon;
    }
}