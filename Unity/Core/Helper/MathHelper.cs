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
        return new Vector4(v.X, v.Y, v.Z, 0);
    }
}